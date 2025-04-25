using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    internal class Client
    {
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public IPAddress realtime_IP { get; set; } = IPAddress.Loopback;
        public bool state { get; set; } = true;

        public Client(string username,string password)
        {
            this.username = username;
            this.password = password;
        }
        public Client(string username,string password,IPAddress realtime_IP)
        {
            this.username = username;
            this.password = password;
            this.realtime_IP = realtime_IP;
        }
        public Client() { }
    }
}
