using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    class Receive_Form:Data_Form
    {
        public override string source { get; set; } = "Server";//nơi gửi dữ liệu
    }
}
