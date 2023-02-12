using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVIClient.Models
{
    internal class TCPClientData
    {
        public string Command { get; set; }
        public string DateAndTime { get; set; }
        public string CarId { get; internal set; }
        public string Target { get; set; }
        public string Sender { get; set; }
        public string UserList { get; set; }
        public string Message { get; set; }


    }
}
