using Newtonsoft.Json;
using RVIClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RVIClient
{
    internal class Communicate
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
        static public string SendTakePicCMDJSON(ListBox listbox, TCPClientData PicCMDJSON, string User)
        {
            string tb_log_text = "";

            //PicCMD = $"：{DateTime.Now.ToString("yyyyMMdd_hhmmss")}_KLE1234F";
            //Send("3" + "|" + "來自" + User + PicCMD + "|" + ConnectTarget + "\\");
            SendJSON(PicCMDJSON);
            tb_log_text += "Client send command (" + PicCMDJSON.Command + ") to Server\r\n";

            return tb_log_text;
        }
    }
}
