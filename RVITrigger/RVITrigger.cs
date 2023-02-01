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
    public partial class RVITrigger : Form
    {
        Socket T;
        Thread Th;
        //Socket T_forEPSLog;
        Thread Th_forEPSLog;
        Thread Th_forCCTV;
        string User;
        static string PhotosPath;
        static Queue<string> CCTVWorkQueue = new Queue<string>();
        static Boolean debugMode = false;
        private System.Timers.Timer timer;
        private int CountDownTimeInSecond = 5;
        public RVITrigger()
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

        public void StartConnectTimer()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000; // 5 seconds
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
            tb_log.AppendText($"{CountDownTimeInSecond}秒後開始執行程式\r\n");
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
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
                TCPClientData LoginMsg= new TCPClientData{
                    Command = "NewUserLogin",
                    Sender = User
                };
                Communicate.SendJSON(LoginMsg);
                Th = new Thread(Listen);    //建立監聽執行緒
                Th.IsBackground = true;     //設定為背景執行緒
                Th.Start();

                if (tb_UserName.Text.IndexOf("EPS", 0) != -1)
                {
                    //若此程式的使用者名稱含"EPS"，表示其為EPS電腦，此電腦需上傳EPS LOG至遠端伺服器。
                    //遠端伺服器需檢查上傳項目是否重複。
                    //成功上傳後，EPS電腦須發送訊息給CCTV電腦，通知其進行拍照存檔。
                    Th_forEPSLog = new Thread(ContinueReadEPSLog);    //建立監聽執行緒
                    Th_forEPSLog.IsBackground = true;     //設定為背景執行緒
                    Th_forEPSLog.Start();
                }
                if (tb_UserName.Text.IndexOf("CCTV", 0) != -1)
                {
                    //若此程式的使用者名稱含"CCTV"，表示其為CCTV電腦，此電腦需至特定區域網路位置下載圖片。
                    Th_forCCTV = new Thread(ContinueDoCCTVWork);    //建立監聽執行緒
                    Th_forCCTV.IsBackground = true;     //設定為背景執行緒
                    Th_forCCTV.Start();
                }
                tb_log.AppendText(DateTime.Now + " " + "已連線伺服器!" + "\r\n");
                ToggleButtonState();
            }
            catch (Exception ex)
            {
                if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + ex.ToString() + "\n");
                tb_log.AppendText(DateTime.Now + " " + "無法連上伺服器!" + "\r\n");
                StartConnectTimer();
                //return;
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
                }
                catch (Exception ex)
                {
                    if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + ex.ToString() + "\n");
                    Communicate.T.Close();                                  //關閉通訊器
                    lb_UserList.Items.Clear();                     //清除線上名單
                    tb_log.AppendText(DateTime.Now + " " + "無法連上伺服器!" + "\r\n");
                    ToggleButtonState();
                    StartConnectTimer();
                    Th.Abort();
                    //return;
                }
                Msg_JSON = Encoding.Default.GetString(B, 0, inLen);  //解讀完整訊息
                TCPClientData JsonData = JsonConvert.DeserializeObject<TCPClientData>(Msg_JSON);
                string Cmd = JsonData.Command;    //取出命令碼(第一個|之前的字)
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
                    case "TakePicture":
                        tb_log.AppendText("(銷帳拍照)" + JsonData.DateAndTime + "_" + JsonData.CarId + "\r\n");      //顯示訊息並換行
                        tb_IP.SelectionStart = tb_IP.Text.Length;//游標移到最後
                        tb_IP.ScrollToCaret();                    //捲動到游標位置
                        CCTVWorkQueue.Enqueue(JsonData.DateAndTime + "_" + JsonData.CarId);
                        break;
                }
            }
        }
        private void ContinueDoCCTVWork()
        {
            while (true)
            {
                if (CCTVWorkQueue.Count != 0)
                {
                    CCTV.TakePic(CCTVWorkQueue.Dequeue());
                    tb_log.AppendText(CCTV.errMessage);
                }
            }
        }
        private void ContinueReadEPSLog()
        {
            //EPS.lb_UserList = lb_UserList;
            //EPS.tb_ConnectTarget = tb_ConnectTarget;
            //EPS.User = User;
            //EPS.dayShift = Double.Parse(tb_DayShift.Text);
            //int ReadCount = 0;
            //while (true)
            //{
            //    tb_log.AppendText(EPS.ImportImmediately());
            //    //Communicate.SendTakePicCMD(listBox1, tb_ConnectTarget.Text, $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F", User);
            //    Thread.Sleep(1000);
            //    if (ReadCount >= 100)
            //    {
            //        if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkLog.txt"), tb_log.Text);
            //        tb_log.Clear();
            //        ReadCount = 0;
            //    }
            //    ReadCount++;
            //}
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btn_connect.Enabled == false)
            {
                Communicate.Send("9" + "|" + User);   //傳送自己的離線訊息給伺服器
                T.Close();
            }

            using (ReadINI oTINI = new ReadINI("./Config.ini"))
            {
                oTINI.setKeyValue("ServerPort", "Value", tb_Port.Text);
                oTINI.setKeyValue("ServerIP", "Value", tb_IP.Text);
                oTINI.setKeyValue("UserName", "Value", tb_UserName.Text);
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
                Communicate.Send("9" + "|" + User);   //傳送自己的離線訊息給伺服器                
                tb_log.AppendText(DateTime.Now + " " + "已從伺服器離線!" + "\r\n");
                lb_UserList.Items.Clear();
                ToggleButtonState();
                Th_forEPSLog.Abort();
            }
            catch (Exception ex)
            {
                return;
            }
        }
        private void ToggleButtonState()
        {
            btn_connect.Enabled = !btn_connect.Enabled;
            btn_sendMessage.Enabled = !btn_sendMessage.Enabled;
            btn_disconnect.Enabled = !btn_disconnect.Enabled;
            btn_takePic.Enabled = !btn_takePic.Enabled;
            tb_IP.Enabled = !tb_IP.Enabled;
            tb_UserName.Enabled = !tb_UserName.Enabled;
            tb_Port.Enabled = !tb_Port.Enabled;
        }
        private class CCTV
        {
            public static string errMessage;
            public static void TakePic(string flowTimeAndCarId)
            {
                string username = "admin";
                string password = "Admin123";
                string url = "";
                string i_str = "";
                CreateFolder(PhotosPath, flowTimeAndCarId);
                for (int i = 1; i <= 13; i++)
                {
                    i_str = i.ToString().PadLeft(2, '0');
                    url = $"http://192.168.7.1{i_str}/images/snapshot.jpg";
                    errMessage = HttpGetRequest_SaveCamPic(username, password, url, i_str, flowTimeAndCarId, PhotosPath);
                }
                i_str = "14";
                url = $"http://192.168.7.1{i_str}/PictureCatch.cgi?username={username}&password={password}&channel=1";
                errMessage = HttpGetRequest_SaveCamPic(username, password, url, i_str, flowTimeAndCarId, PhotosPath);

                //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini")
            }
            static void CreateFolder(string photosPath, string flowTimeAndCarId)
            {
                try
                {
                    string foldrePath = "";
                    foldrePath = photosPath + $@"\\{flowTimeAndCarId}";
                    //判斷檔案路徑是否存在，不存在則建立資料夾 
                    if (!System.IO.Directory.Exists(foldrePath))
                    {
                        System.IO.Directory.CreateDirectory(foldrePath);//不存在就建立目錄 
                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.ToString();
                    if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + errMessage.ToString() + "\n");
                }
            }
            static string HttpGetRequest_SaveCamPic(string username, string password, string url, string i_str, string flowTimeAndCarId, string folderPath)
            {
                CookieContainer cookieContainer = new CookieContainer();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                request.Credentials = new NetworkCredential(username, password);
                request.CookieContainer = cookieContainer;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.Method = "GET";
                request.ContentType = "image/jpeg";

                try
                {
                    //看到.GetResponse()才代表真正把 request 送到 伺服器
                    using (FileStream fs = new FileStream($@"{folderPath}{flowTimeAndCarId}//{flowTimeAndCarId}_cam{i_str}.jpeg", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        using (WebResponse response = request.GetResponse())
                        {
                            using (var sr = response.GetResponseStream())
                            {
                                // sr 就是伺服器回覆的資料
                                sr.CopyTo(fs);
                                return "Photos Saved.\r\n";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMessage = ex.ToString();
                    if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + errMessage.ToString() + "\n");
                    return errMessage;
                }
            }
        }
        private class Communicate
        {
            public static Socket T;

            static public void Send(string Str)
            {
                byte[] B = Encoding.Default.GetBytes(Str);  //翻譯字串Str為Byte陣列B
                T.Send(B, 0, B.Length, SocketFlags.None);   //使用連線物件傳送資料
            }
            static public void SendJSON(TCPClientData CMDJSON)
            {
                string Msg_JSON = JsonConvert.SerializeObject(CMDJSON);
                byte[] B = Encoding.Default.GetBytes(Msg_JSON);  //翻譯字串Str為Byte陣列B
                T.Send(B, 0, B.Length, SocketFlags.None);   //使用連線物件傳送資料
            }
            static public string SendTakePicCMD(ListBox listbox, string ConnectTarget, string PicCMD, string User)
            {
                string tb_log_text = "";
                bool ConnectTargetFound = false;
                foreach (var item in listbox.Items)
                {
                    if (item.ToString() == ConnectTarget)
                    {
                        //PicCMD = $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F";
                        //Send("3" + "|" + "來自" + User + PicCMD + "|" + ConnectTarget + "\\");
                        Send("3" + "|" + "來自" + User + PicCMD + "|" + ConnectTarget);
                        tb_log_text += DateTime.Now + " " + "告訴" + ConnectTarget + PicCMD + "\r\n";

                        ConnectTargetFound = true;
                        break;
                    }
                }
                if (ConnectTargetFound == false)
                {
                    tb_log_text += DateTime.Now + " " + "(系統訊息)找不到" + ConnectTarget + "!" + "\r\n";
                }
                return tb_log_text;
            }
            static public string SendTakePicCMDJSON(ListBox listbox, string ConnectTarget, TCPClientData PicCMDJSON, string User)
            {
                string tb_log_text = "";
                bool ConnectTargetFound = false;
                foreach (var item in listbox.Items)
                {
                    if (item.ToString() == ConnectTarget)
                    {
                        //PicCMD = $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F";
                        //Send("3" + "|" + "來自" + User + PicCMD + "|" + ConnectTarget + "\\");
                        SendJSON(PicCMDJSON);
                        tb_log_text += DateTime.Now + " " + "告訴" + ConnectTarget + PicCMDJSON.Command + "\r\n";
                        ConnectTargetFound = true;
                        break;
                    }
                }
                if (ConnectTargetFound == false)
                {
                    tb_log_text += DateTime.Now + " " + "(系統訊息)找不到" + ConnectTarget + "!" + "\r\n";
                }
                return tb_log_text;
            }
        }
        private class EPS
        {
            public static string RequestVerificationToken = "";
            public static CookieContainer cookieContainer = new CookieContainer();

            int i = 0;
            static int lastTimeReadLineIndex = 0;
            static int allLinesCount = 0;
            static string carId = "";
            static List<string> coilList = new List<string>();

            static DateTime CrossDay;

            public static ListBox lb_UserList;
            public static TextBox tb_ConnectTarget;
            public static string User;
            public static string creatorId;
            public static string tb_log_text;
            public static double dayShift;
            //SendTakePicCMD(listBox1, tb_ConnectTarget.Text, $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F", User);

            static public string ImportImmediately()
            {
                tb_log_text = "";
                DateTime workingDate = DateTime.Now.AddDays(dayShift);
                if (CrossDay.Date != workingDate.Date)
                {
                    CrossDay = workingDate.Date;
                    lastTimeReadLineIndex = 0;
                }
                //string myDate = DateTime.Now.Date.AddDays(dayShift).ToString("yyyy-MM-dd");
                string myDate = workingDate.ToString("yyyy-MM-dd");
                //string myDate = "2022-08-23";

                string myLogPath = "";
                //string myDest = "";
                string myTail;
                string myLogFolder;
                using (ReadINI oTINI = new ReadINI(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.ini")))
                {
                    myLogFolder = oTINI.getKeyValue("EPSWorklogPath", "Value"); //Section name=Worklog；Key name=Value              
                    myTail = myDate + "_WorkLog.log";
                    //myPath = myInIPath + myTail;
                    myLogPath = Path.Combine(myLogFolder, myTail);
                    //myDest = Path.Combine(appLocation, "ActiveWorkLog.log");
                }

                List<remote_visual_inspection> rviList = new List<remote_visual_inspection>();
                allLinesCount = 0;
                rviList = readLogOneDay(myLogPath);
                foreach (var rvi in rviList)
                {
                    if (upload(rvi))
                    {
                        //$"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F"
                        string PicCMD = $"：{rvi.tdate.Value.ToString("yyyyMMdd_HHmmss")}_{rvi.carId}";

                        //嘗試改以JSON作為通訊的格式
                        TCPClientData PicCMDJSON = new TCPClientData
                        {
                            Command = PicCMD,
                            DateAndTime = $"{rvi.tdate.Value.ToString("yyyyMMdd_HHmmss")}",
                            CarId = $"{rvi.carId}",
                            Sender = ""
                        };
                        //嘗試改以JSON作為通訊的格式

                        Boolean IsLogEmpty = tb_log_text == "" ? true : false;
                        if (!IsLogEmpty) tb_log_text = tb_log_text + "\n";
                        //tb_log_text = tb_log_text + DateTime.Now + " " + Communicate.SendTakePicCMD(lb_UserList, tb_ConnectTarget.Text, PicCMD, User);
                        tb_log_text = tb_log_text + DateTime.Now + " " + Communicate.SendTakePicCMDJSON(lb_UserList, tb_ConnectTarget.Text, PicCMDJSON, User);
                        Thread.Sleep(100);
                    }
                }
                rviList = new List<remote_visual_inspection>();
                return tb_log_text;
            }
            static List<remote_visual_inspection> readLogOneDay(string myLogPath)
            {
                DateTime workingDate = DateTime.Now.AddDays(dayShift);
                List<remote_visual_inspection> rviList = new List<remote_visual_inspection>();
                var appLocation = AppDomain.CurrentDomain.BaseDirectory;
                List<string> allLines = new List<string>();
                try
                {
                    using (var fs = new FileStream(myLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var logFileReader = new StreamReader(fs, ASCIIEncoding.Default))
                    {
                        while (!logFileReader.EndOfStream)
                        {
                            allLines.Add(logFileReader.ReadLine());
                            allLinesCount++;
                            // Your code here
                        }
                        // read the stream
                        //...
                    }

                    //上次讀取log檔行數與這次讀取log檔行數相同，直接結束
                    if (allLinesCount == lastTimeReadLineIndex)
                    {
                        tb_log_text = tb_log_text + DateTime.Now + " " + $"allLinesCount={allLinesCount}=lastTimeReadLineIndex={lastTimeReadLineIndex},代表log未新增紀錄，結束本次讀取作業。" + "\r\n";
                        return new List<remote_visual_inspection>();
                    }

                    int thisTimeReadLineIndex = 0;
                    foreach (var item in allLines)
                    {
                        if (thisTimeReadLineIndex >= lastTimeReadLineIndex)//直接快速跳過前面曾經讀過的log，從lastTimeReadLineIndex開始
                        {
                            if (item.IndexOf("車輛報到") != -1)
                            {
                                var tt = item.IndexOf("車輛報到");
                                int commaPos1 = item.IndexOf(",");
                                int car_checkin_pos = commaPos1 + 7;
                                int car_Num_start_pos = car_checkin_pos + 4;
                                carId = item.Substring(car_Num_start_pos, 8);
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 報到成功" + "\r\n";
                            }
                            else if (item.IndexOf("掃瞄車上鋼品") != -1)
                            {
                                var tt = item.IndexOf("掃瞄車上鋼品");

                                var commaPos1 = item.IndexOf(",");
                                var commaPos2 = item.IndexOf("顆", commaPos1 + 1);
                                var commaPos3 = item.IndexOf(",", commaPos1 + 1);
                                //line += item.Substring(commaPos1 + 1, commaPos2 - commaPos1 - 1) + "\n";
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"掃瞄車上鋼品 {item.Substring(commaPos2 + 1, commaPos3 - commaPos2 - 1)}" + "\r\n";
                                coilList.Add(item.Substring(commaPos2 + 1, commaPos3 - commaPos2 - 1));
                            }
                            else if (item.IndexOf("放行") != -1)
                            {
                                var tt = item.IndexOf("放行");
                                var commaPos1 = item.IndexOf(",");
                                //line += item.Substring(commaPos1 + 1) + "\n" + "等待車輛報到中........\n";

                                //keep_Queue.Clear();
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 放行";
                                remote_visual_inspection rvi = new remote_visual_inspection();
                                rvi.carId = carId;
                                rvi.creator = creatorId;
                                foreach (var coil in coilList)
                                {
                                    if (rvi.coil1 is null)
                                        rvi.coil1 = coil;
                                    else if (rvi.coil2 is null)
                                        rvi.coil2 = coil;
                                    else if (rvi.coil3 is null)
                                        rvi.coil3 = coil;
                                    else if (rvi.coil4 is null)
                                        rvi.coil4 = coil;
                                    else if (rvi.coil5 is null)
                                        rvi.coil5 = coil;
                                    else if (rvi.coil6 is null)
                                        rvi.coil6 = coil;
                                    else if (rvi.coil7 is null)
                                        rvi.coil7 = coil;
                                    else if (rvi.coil8 is null)
                                        rvi.coil8 = coil;
                                }
                                rvi.tdate = DateTime.Parse(item.Substring(0, commaPos1));
                                rviList.Add(rvi);
                                carId = "";
                                coilList = new List<string>();
                            }
                            else if (item.IndexOf("已退車") != -1)
                            {
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 放行" + "\r\n";
                                carId = "";
                                coilList = new List<string>();
                            }
                            else if (item.IndexOf("無內銷車籍記錄") != -1)
                            {
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 無內銷車籍記錄" + "\r\n";
                                carId = "";
                                coilList = new List<string>();
                            }
                            else if (item.IndexOf("無外銷車籍記錄") != -1)
                            {
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 無外銷車籍記錄" + "\r\n";
                                carId = "";
                                coilList = new List<string>();
                            }
                            else if (item.IndexOf("登入") != -1)
                            {
                                var creatorId_Num_start_pos = item.IndexOf("檢核員");
                                creatorId = item.Substring(creatorId_Num_start_pos + 3, 6);
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"檢核員 {creatorId} 已登入" + "\r\n";
                            }
                            else if (item.IndexOf("登出") != -1)
                            {
                                var tt = item.IndexOf("登出");
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"檢核員 {creatorId} 已登出" + "\r\n";
                                creatorId = "";
                            }
                            //
                        }
                        thisTimeReadLineIndex++;
                    }
                    //將本次讀取到的行數紀錄起來，下次讀取時可知道上次讀取到哪一行，避免重複讀取
                    lastTimeReadLineIndex = thisTimeReadLineIndex;

                    if (rviList.Count == 0)
                    {                        //allLinesCount = 0;
                        return new List<remote_visual_inspection>();
                    }
                    return rviList;
                }
                catch (Exception ex)
                {
                    tb_log_text = tb_log_text + DateTime.Now + " " + ex.ToString() + "\r\n";
                    //File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now +" "+ ex.ToString() +"\n");
                    return new List<remote_visual_inspection>();
                }
            }
            static Boolean upload(remote_visual_inspection rvi)
            {

                try
                {
                    string url = "http://c34web.csc.com.tw/C349WebMVC/api/RVI_API";
                    //string url = "http://c34web.csc.com.tw/C349WebMVC/api/RVI_API";
                    //string url = "http://localhost:1954/RVI_CreateWithEPSLOG/Create";
                    //string url = "http://localhost:1954/api/RVI_API";



                    Boolean log_exist = false;  //此flag用來檢查server端是否存在相同log，若有則取消此次上傳
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + $"?tdate={rvi.tdate.Value.ToString("yyyy-MM-dd HH:mm:ss")}&carId={rvi.carId}");
                    //set the cookie container object
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/45.0.2454.101 Safari/537.36";
                    request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                    //set method POST and content type application/x-www-form-urlencoded
                    request.Method = "GET";
                    request.ContentType = "application/json";


                    //看到.GetResponse()才代表真正把 request 送到 伺服器
                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default))
                        {
                            var z = sr.ReadToEnd().Split(',')[0].Split(':')[1];
                            if (z.IndexOf("null", 0) >= 0)
                            {
                                log_exist = true;
                            }
                        }
                    }
                    if (!log_exist) return false;

                    string Param = $"carId={rvi.carId}&tdate={HttpUtility.UrlEncode(rvi.tdate.Value.ToString("yyyy-MM-dd HH:mm:ss"), Encoding.UTF8)}" +
                        $"&coil1={rvi.coil1}" +
                        $"&coil2={rvi.coil2}" +
                        $"&coil3={rvi.coil3}" +
                        $"&coil4={rvi.coil4}" +
                        $"&coil5={rvi.coil5}" +
                        $"&coil6={rvi.coil6}" +
                        $"&coil7={rvi.coil7}" +
                        $"&coil8={rvi.coil8}" +
                        $"&creator={rvi.creator}";
                    request = (HttpWebRequest)HttpWebRequest.Create(url + $"/create?{Param}");
                    request.CookieContainer = cookieContainer;

                    request.Method = "GET";
                    request.ContentType = "application/json";
                    string EIPLoginPostData = "";
                    EIPLoginPostData = Param;

                    Boolean UpLoadAgain = false;

                    //看到.GetResponse()才代表真正把 request 送到 伺服器
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                UpLoadAgain = true;
                            }
                            var yy = sr.ReadToEnd();
                            // sr 就是伺服器回覆的資料
                            //Response.Write(sr.ReadToEnd()); //將 sr 寫入到 html中，呈現給客戶端看
                        }
                    }
                    while (UpLoadAgain)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"{rvi.tdate}/{rvi.carId}/{rvi.coil1}/{rvi.coil2}/{rvi.coil3}/{rvi.coil4}/{rvi.coil5}/{rvi.coil6}/{rvi.coil7}/{rvi.coil8} UpLoad Fail.");
                        if (debugMode) File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "upload_err_log.txt"), DateTime.Now + " " + sb + "\n");
                        sb.Clear();
                        System.Threading.Thread.Sleep(1000);
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                            {
                                if (response.StatusCode != HttpStatusCode.OK)
                                {
                                    UpLoadAgain = true;
                                }
                            }
                        }
                    }

                    return true;


                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            static string[] GetAllFileInDirectory(string Path)
            {
                try
                {
                    string[] entries = Directory.GetFiles(Path);
                    return entries;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
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
    }

    class TCPClientData
    {
        public string Command { get; set; }
        public string DateAndTime { get; set; }
        public string CarId { get; internal set; }
        public string Target { get; set; }
        public string Sender { get; set; }
        public string UserList { get; set; }

    }
}
