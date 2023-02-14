using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RVIClient;
using System.IO;
using RVIClient.Models;
using System.Web;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Timers;


namespace RVIClient
{
    public partial class RVIClient : Form
    {
        Socket T;
        Thread Th;
        //Socket T_forEPSLog;
        Thread Th_forEPSLog;
        Thread Th_forCCTV;
        Thread Th_SocketConnectionSupervisor;
        string User;
        static string PhotosPath;
        static Queue<string> CCTVWorkQueue = new Queue<string>();
        static Boolean debugMode = false;
        private System.Timers.Timer timer;
        private int CountDownTimeInSecond = 5;
        private Boolean SocketConnectionState = false;
        private Boolean ConnectTimerIsUsing = false;
        private Boolean SuperVisorIsStart = false;
        public RVIClient()
        {
            InitializeComponent();
            using (ReadINI oTINI = new ReadINI(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini")))
            {
                tb_Port.Text = oTINI.getKeyValue("ServerPort", "Value"); //Section name=ServerPort；Key name=Value
                tb_IP.Text = oTINI.getKeyValue("ServerIP", "Value");
                tb_UserName.Text = oTINI.getKeyValue("UserName", "Value");
                PhotosPath = oTINI.getKeyValue("PhotosPath", "Value");
            }
            debugMode = cb_debug.Checked;
            StartConnectTimer();
        }
        public void SupervisorStart()
        {
            if (SuperVisorIsStart == false)
            {
                Th_SocketConnectionSupervisor = new Thread(SocketConnectionSupervisor);    //建立連線狀態監控執行緒
                Th_SocketConnectionSupervisor.IsBackground = true;     //設定為背景執行緒
                Th_SocketConnectionSupervisor.Start();
                SuperVisorIsStart = true;
            }
        }
        public void SocketConnectionSupervisor()
        {
            while (true)
            {
                if (SocketConnectionState == false) StartConnectTimer();
                Thread.Sleep(10000);
            }
        }
        public void StartConnectTimer()
        {
            CheckForIllegalCrossThreadCalls = false;
            ConnectTimerIsUsing = true;
            timer = new System.Timers.Timer();
            timer.Interval = 1000; // 5 seconds
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
            tb_log.AppendText($"{CountDownTimeInSecond}秒後開始連線\r\n");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (CountDownTimeInSecond > 0)
            {
                // Update tb_log to show the remaining countdown
                CountDownTimeInSecond--;
                tb_log.AppendText(CountDownTimeInSecond.ToString() + "...\r\n");
            }
            else
            {
                // Show message box when the countdown reaches 0
                timer.Enabled = false;
                CountDownTimeInSecond = 5;
                ConnectTimerIsUsing = false;
                StartConnecting();
            }
            // Code to be executed when the timer elapses
        }
        private void StartConnecting()
        {
            CheckForIllegalCrossThreadCalls = false;    //忽略跨執行緒錯誤
            string IP = tb_IP.Text;
            int port = int.Parse(tb_Port.Text);
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), port);

            //建立可雙向通訊的TCP連線
            T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            User = tb_UserName.Text;

            Communicate.T = T;
            try
            {

                Communicate.T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Communicate.T.Connect(EP);              //連上伺服器的端點EP(類似撥號給電話總機)
                //Communicate.Send("0" + "|" + User + "\\");
                TCPClientData LoginMsg = new TCPClientData
                {
                    Command = "NewUserLogin",
                    Sender = User
                };
                Communicate.SendJSON(LoginMsg);
                Th = new Thread(Listen);    //建立監聽執行緒
                Th.IsBackground = true;     //設定為背景執行緒
                Th.Start();

                //此電腦需上傳EPS LOG至遠端伺服器。
                //遠端伺服器需檢查上傳項目是否重複。
                //成功上傳後，EPS電腦須發送訊息給CCTV電腦，通知其進行拍照存檔。
                Th_forEPSLog = new Thread(ContinueReadEPSLog);    //建立監聽執行緒
                Th_forEPSLog.IsBackground = true;     //設定為背景執行緒
                Th_forEPSLog.Start();

                tb_log.AppendText(DateTime.Now + " " + "已連線伺服器!" + "\r\n");
                SocketConnectionState = true;
                ButtonStateCtrl();
                SupervisorStart();
            }
            catch (Exception ex)
            {
                if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + ex.ToString() + "\n");
                tb_log.AppendText(DateTime.Now + " " + "無法連上伺服器!-StartConnecting" + "\r\n");
                SocketConnectionState = false;
                ButtonStateCtrl();
                SupervisorStart();
                return;
            }
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            StartConnectTimer();
        }
        private void Listen()
        {
            //監聽伺服器回傳訊息
            EndPoint ServerEP = (EndPoint)Communicate.T.RemoteEndPoint;     //Server 的 EndPoint
            byte[] B = new byte[1023];
            int inLen = 0;
            string Msg_JSON;                                         //接收到的完整訊息

            while (true)
            {
                try
                {
                    inLen = Communicate.T.ReceiveFrom(B, ref ServerEP);     //收聽資訊並取得位元組數
                    Msg_JSON = Encoding.Default.GetString(B, 0, inLen);  //解讀完整訊息
                    TCPClientData JsonData = JsonConvert.DeserializeObject<TCPClientData>(Msg_JSON);
                    string Cmd = JsonData.Command;    //取出命令碼
                    switch (Cmd)
                    {
                        case "UpdateUserList":                                   //接收線上名單
                            lb_UserList.Items.Clear();                 //清除名單
                            string[] M = JsonData.UserList.Split(',');            //拆解名單成陣列
                            for (int i = 0; i < M.Length; i++)
                            {
                                lb_UserList.Items.Add(M[i]);           //逐一加入名單
                            }
                            break;
                        default:
                            tb_log.AppendText($"({Cmd} from {JsonData.Sender}) : {JsonData.Message}\r\n");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + ex.ToString() + "\n");
                    Communicate.T.Close();                                  //關閉通訊器
                    lb_UserList.Items.Clear();                     //清除線上名單
                    tb_log.AppendText(DateTime.Now + " " + "無法連上伺服器!-Listen" + "\r\n");
                    SocketConnectionState = false;
                    ButtonStateCtrl();
                    T.Close();
                    //if (ConnectTimerIsUsing == false) StartConnectTimer();
                    Th_forEPSLog.Abort();
                    Th.Abort();
                    return;
                }

            }
        }
        private void ContinueReadEPSLog()
        {
            ////for developer testing
            //while (true)
            //{
            //    tb_log.AppendText(DateTime.Now + " " + "讀取EPSLOG-ContinueReadEPSLog" + "\r\n");
            //    Thread.Sleep(1000);
            //}
            ////for developer testing 

            EPS.lb_UserList = lb_UserList;
            EPS.User = User;
            EPS.dayShift = Double.Parse(tb_DayShift.Text);
            EPS.debugMode = debugMode;
            int ReadCount = 0;
            while (true)
            {
                tb_log.AppendText(EPS.ImportImmediately());
                //Communicate.SendTakePicCMD(listBox1, tb_ConnectTarget.Text, $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F", User);
                Thread.Sleep(1000);
                if (ReadCount >= 1000)
                {
                    //if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkLog.txt"), tb_log.Text);
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkLog.txt"), tb_log.Text);
                    tb_log.Clear();
                    ReadCount = 0;
                }
                ReadCount++;
            }
        }
        private void btn_sendMessage_Click(object sender, EventArgs e)
        {
            if (tb_message.Text == "")
            {
                tb_log.AppendText("(系統訊息)訊息欄空白");
                return;  //訊息欄空白則不傳送資料
            }
            if (tb_message.Text.IndexOf("\\") != -1)
            {
                tb_log.AppendText("(系統訊息)訊息欄含非法字元");
                return;  //訊息欄含非法字元則不傳送資料
            }
            if (tb_message.Text.IndexOf("|") != -1)
            {
                tb_log.AppendText("(系統訊息)訊息欄含非法字元");
                return;  //訊息欄含非法字元則不傳送資料
            }
            if (lb_UserList.SelectedIndex < 0)      //未選取傳送對象(廣播)，命令碼：1
            {
                //Communicate.Send("1" + "|" + User + "公告：" + tb_message.Text + "\\");
                Communicate.Send("1" + "|" + User + "公告：" + tb_message.Text);
            }
            else
            {
                //Communicate.Send("2" + "|" + "來自" + User + "：" + tb_message.Text + "|" + lb_UserList.SelectedItem + "\\");
                Communicate.Send("2" + "|" + "來自" + User + "：" + tb_message.Text + "|" + lb_UserList.SelectedItem);
                tb_log.AppendText(DateTime.Now + " " + "告訴" + lb_UserList.SelectedItem + "：" + tb_message.Text + "\n");
                tb_message.Text = "";           //清除發言框
            }
        }
        private void btn_broadCast_Click(object sender, EventArgs e)
        {
            lb_UserList.ClearSelected();
        }
        private void btn_takePic_Click(object sender, EventArgs e)
        {
            //Communicate.SendTakePicCMD(lb_UserList, tb_ConnectTarget.Text, $"：{DateTime.Now.ToString("yyyyMMdd_HHmmss")}_KLE1234F", User);
        }
        private void btn_disconnect_Click(object sender, EventArgs e)
        {
            try
            {
                TCPClientData LogoutMsg = new TCPClientData
                {
                    Command = "UserLogOut",
                    Sender = User
                };
                Communicate.SendJSON(LogoutMsg);//傳送自己的離線訊息給伺服器    
                tb_log.AppendText(DateTime.Now + " " + "已從伺服器離線!" + "\r\n");
                lb_UserList.Items.Clear();
                SocketConnectionState = false;
                ButtonStateCtrl();
                Th_forEPSLog.Abort();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void ButtonStateCtrl()
        {
            if (SocketConnectionState == true)
            {
                btn_connect.Enabled = false;
                btn_sendMessage.Enabled = true;
                btn_disconnect.Enabled = true;
                btn_takePic.Enabled = true;
                btn_cancel_connect.Enabled = false;
                tb_IP.Enabled = false;
                tb_UserName.Enabled = false;
                tb_Port.Enabled = false;
            }
            else
            {
                btn_connect.Enabled = true;
                btn_sendMessage.Enabled = false;
                btn_disconnect.Enabled = false;
                btn_takePic.Enabled = false;
                btn_cancel_connect.Enabled = true;
                tb_IP.Enabled = true;
                tb_UserName.Enabled = true;
                tb_Port.Enabled = true;
            }
        }

        private void btn_exportLog_Click(object sender, EventArgs e)
        {
            if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkLog.txt"), tb_log.Text);
        }

        private void btn_Cleartb_log_Click(object sender, EventArgs e)
        {
            tb_log.Clear();
        }

        private void btn_cancel_connect_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
            CountDownTimeInSecond = 5;
        }



        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RecoverMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void RVIClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                //隱藏程式本身的視窗
                //this.Hide();
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                //notifyIcon1.Tag = string.Empty;

                ////讓程式在工具列中隱藏
                //this.ShowInTaskbar = false;
                ////通知欄顯示Icon
                notifyIcon1.Visible = true;

                //通知欄提示 (顯示時間毫秒，標題，內文，類型)
                notifyIcon1.ShowBalloonTip(1000, this.Text, "縮小至工作列圖示區", ToolTipIcon.Info);
                //notifyIcon1.ShowBalloonTip(3000, this.Text,
                //     "程式並未結束，要結束請在圖示上按右鍵，選取結束功能!", ToolTipIcon.Info);
            }
        }

        private void RVIClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (btn_connect.Enabled == false)
            {
                TCPClientData LogoutMsg = new TCPClientData
                {
                    Command = "UserLogOut",
                    Sender = User
                };
                Communicate.SendJSON(LogoutMsg);//傳送自己的離線訊息給伺服器    
                T.Close();
            }

            try
            {
                using (ReadINI oTINI = new ReadINI("./Config.ini"))
                {
                    oTINI.setKeyValue("ServerPort", "Value", tb_Port.Text);
                    oTINI.setKeyValue("ServerIP", "Value", tb_IP.Text);
                    oTINI.setKeyValue("UserName", "Value", tb_UserName.Text);
                }
            }
            catch
            {

            }
            Close();
        }
    }


}
