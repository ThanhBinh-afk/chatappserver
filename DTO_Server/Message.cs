using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    public class Message
    {
        public string message { get; set; }
        public string mess_From { get; set; }
        public string mess_To { get; set; }
        public bool state { get; set; } = false;
        public DateTime time { get; set; } = DateTime.Now;

        public Message(string message, string mess_From, string mess_To, DateTime time)
        {
            this.message = message;
            this.mess_From = mess_From;
            this.mess_To = mess_To;
            this.time = time;
        }
        public Message(string message, string mess_From, string mess_To)
        {
            this.message = message;
            this.mess_From = mess_From;
            this.mess_To = mess_To;
        }
        public Message() { }

    }
}
