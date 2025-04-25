using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    public class Send_Form:Data_Form
    {
        public override string des { get; set; } = "Server";//nơi nhận dữ liệu
        public Send_Form(Type_Packet type_Data, string des_Data, string data)
        {
            this.type = type_Data;
            this.des = des_Data;
            this.data = data;
        }
    }
}
