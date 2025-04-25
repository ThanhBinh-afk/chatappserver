using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    public class Data_Form
    {
        public Type_Packet type { get; set; } = Type_Packet.DATA;//loại gói tin : kết nối , thông báo , gửi dữ liệu ,...
        public string data { get; set; } = "";//nội dung gói tin

        public virtual string source { get; set; }
        public virtual string des { get; set; }
    }
}
