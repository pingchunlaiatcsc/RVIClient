using RVIClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace RVIClient
{
    internal class EPS
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
        public static string User;
        public static string creatorId;
        public static string tb_log_text;
        public static double dayShift;
        //SendTakePicCMD(listBox1, tb_ConnectTarget.Text, $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F", User);
        public static Boolean debugMode;
        public static string Location;
        public static string EPSVersion;
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
            string myDate;
            //string myDate = "2022-08-23";

            string myLogPath = "";
            //string myDest = "";
            string myTail;
            string myLogFolder = "";
            using (ReadINI oTINI = new ReadINI(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "APP_Config.ini")))
            {
                string config_file;
                switch (Location)
                {
                    case "Y423":
                    case "C349_1":
                    case "C349_2":
                    case "TestEnv":
                        config_file = $"{Location}_Config";
                        break;
                    default:
                        config_file = $"Spare_Config";
                        break;
                }
                myLogFolder = oTINI.getKeyValue(config_file, "EPSWorklogPath"); //Section name=Worklog；Key name=Value
                string worklog_filename = oTINI.getKeyValue(config_file, "EPSWorkLogFileName"); //Section name=Date；Key name=Value
                string worklog_filename_extension = oTINI.getKeyValue(config_file, "EPSWorkLogFileNameExtension"); //Section name=Date；Key name=Value
                EPSVersion = oTINI.getKeyValue(config_file, "EPSVersion"); //Section name=Date；Key name=Value

                if (EPSVersion == "before2023")
                {
                    myDate = workingDate.ToString("yyyy-MM-dd");
                    myTail = myDate + "_WorkLog.log";
                }
                else
                {
                    myDate = workingDate.ToString("yyyyMMdd");
                    myTail = worklog_filename + myDate + worklog_filename_extension;
                }
                myLogPath = Path.Combine(myLogFolder, myTail);
            }

            List<remote_visual_inspection> rviList = new List<remote_visual_inspection>();
            allLinesCount = 0;
            rviList = readLogOneDay(myLogPath);
            foreach (var rvi in rviList)
            {
                if (upload(rvi))
                {
                    //$"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F"
                    //string PicCMD = $"：{rvi.tdate.Value.ToString("yyyyMMdd_HHmmss")}_{rvi.carId}";

                    //嘗試改以JSON作為通訊的格式
                    TCPClientData PicCMDJSON = new TCPClientData
                    {
                        Command = "TakePicture",
                        DateAndTime = $"{rvi.tdate.Value.ToString("yyyyMMdd_HHmmss")}",
                        CarId = $"{rvi.carId}",
                        Sender = $"{User}"
                    };
                    //嘗試改以JSON作為通訊的格式

                    Boolean IsLogEmpty = tb_log_text == "" ? true : false;
                    if (!IsLogEmpty) tb_log_text = tb_log_text + "\n";
                    tb_log_text = tb_log_text + DateTime.Now + " " + Communicate.SendTakePicCMDJSON(lb_UserList, PicCMDJSON, User);
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
                    //tb_log_text = tb_log_text + DateTime.Now + " " + $"allLinesCount={allLinesCount}=lastTimeReadLineIndex={lastTimeReadLineIndex},代表log未新增紀錄，結束本次讀取作業。" + "\r\n";
                    //tb_log_text = tb_log_text + DateTime.Now + "等待log更新中...\r\n";
                    return new List<remote_visual_inspection>();
                }

                int thisTimeReadLineIndex = 0;
                foreach (var item in allLines)
                {
                    if (thisTimeReadLineIndex >= lastTimeReadLineIndex)//直接快速跳過前面曾經讀過的log，從lastTimeReadLineIndex開始
                    {
                        if (item.IndexOf("車輛報到") != -1)
                        {
                            if (EPSVersion == "before2023")
                            {
                                var tt = item.IndexOf("車輛報到");
                                int commaPos1 = item.IndexOf(",");
                                int car_checkin_pos = commaPos1 + 7;
                                int car_Num_start_pos = car_checkin_pos + 4;
                                carId = item.Substring(car_Num_start_pos, 8);
                                tb_log_text = tb_log_text + DateTime.Now + " " + $"{carId} 報到成功" + "\r\n";
                            }
                            else
                            {
                                int firstColonPos = item.IndexOf(":");
                                int secondColonPos = item.IndexOf(":", firstColonPos + 1);
                                int thirdColonPos = item.IndexOf(":", secondColonPos + 1);
                                int fourthColonPos = item.IndexOf(":", thirdColonPos + 1);
                                int fifthColonPos = item.IndexOf(":", fourthColonPos + 1);
                                int car_Num_start_pos = fifthColonPos + 1;
                                carId = item.Substring(car_Num_start_pos, 8).Trim();
                                tb_log_text = item.Substring(car_Num_start_pos, 8).Trim() + "\n報到成功";
                            }
                        }
                        else if (item.IndexOf("掃瞄車上鋼品") != -1)
                        {
                            //EPSVersion == "before2023"
                            var tt = item;
                            var yy = item.IndexOf("掃瞄車上鋼品");
                            var commaPos1 = item.IndexOf(",");
                            var commaPos2 = item.IndexOf("顆", commaPos1 + 1);
                            var commaPos3 = item.IndexOf(",", commaPos1 + 1);
                            tb_log_text = tb_log_text + DateTime.Now + " " + $"掃瞄車上鋼品 {item.Substring(commaPos2 + 1, commaPos3 - commaPos2 - 1)}" + "\r\n";
                            coilList.Add(item.Substring(commaPos2 + 1, commaPos3 - commaPos2 - 1));
                        }
                        else if (item.IndexOf("掃描車上鋼品") != -1)
                        {
                            //EPSVersion == "after2023"
                            var tt = item;
                            int firstColonPos = item.IndexOf(":");
                            int secondColonPos = item.IndexOf(":", firstColonPos + 1);
                            int thirdColonPos = item.IndexOf(":", secondColonPos + 1);
                            var commaPos1 = item.IndexOf("顆", thirdColonPos + 1);
                            var commaPos2 = item.IndexOf(",", thirdColonPos + 1);
                            tb_log_text = tb_log_text + DateTime.Now + " " + $"掃描車上鋼品 {item.Substring(commaPos1 + 1, commaPos2 - commaPos1 - 1)}" + "\r\n";
                            coilList.Add(item.Substring(commaPos1 + 1, commaPos2 - commaPos1 - 1));
                        }
                        else if (item.IndexOf("放行") != -1)
                        {
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
                                else if (rvi.coil9 is null)
                                    rvi.coil9 = coil;
                                else if (rvi.coil10 is null)
                                    rvi.coil10 = coil;
                                else if (rvi.coil11 is null)
                                    rvi.coil11 = coil;
                                else if (rvi.coil12 is null)
                                    rvi.coil12 = coil;
                                else if (rvi.coil13 is null)
                                    rvi.coil13 = coil;
                                else if (rvi.coil14 is null)
                                    rvi.coil14 = coil;
                                else if (rvi.coil15 is null)
                                    rvi.coil15 = coil;
                            }

                            if (EPSVersion == "before2023")
                            {
                                var commaPos1 = item.IndexOf(",");
                                rvi.tdate = DateTime.Parse(item.Substring(0, commaPos1));
                            }
                            else
                            {
                                int firstColonPos = item.IndexOf(":");
                                int secondColonPos = item.IndexOf(":", firstColonPos + 1);
                                int thirdColonPos = item.IndexOf(":", secondColonPos + 1);
                                rvi.tdate = DateTime.Parse(item.Substring(0, thirdColonPos));
                            }
                            rvi.location = Location;
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
                    $"&coil9={rvi.coil9}" +
                    $"&coil10={rvi.coil10}" +
                    $"&coil11={rvi.coil11}" +
                    $"&coil12={rvi.coil12}" +
                    $"&coil13={rvi.coil13}" +
                    $"&coil14={rvi.coil14}" +
                    $"&coil15={rvi.coil15}" +
                    $"&coil16={rvi.coil16}" +
                    $"&location={rvi.location}" +
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
                    sb.Append($"{rvi.tdate}/{rvi.carId}/{rvi.coil1}/{rvi.coil2}/{rvi.coil3}/{rvi.coil4}/{rvi.coil5}/{rvi.coil6}/{rvi.coil7}/{rvi.coil8}/{rvi.coil9}/{rvi.coil10}/{rvi.coil11}/{rvi.coil12}/{rvi.coil13}/{rvi.coil14}/{rvi.coil15}/{rvi.coil16} UpLoad Fail.");
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
}
