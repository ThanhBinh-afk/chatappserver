using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO_Server
{
    public enum Type_Packet
    {
        MESSAGE = 1,//gói tin thông báo(ví dụ : địa chỉ username gửi tin không tồn tại,...)
        DELETE = 2,//gói tin yêu cầu xoá người dùng
        SIGNUP = 3,//gói tin yêu cầu tạo tài khoản
        LOGIN = 4,//gói tin yêu cầu đăng nhập
        ALLUSER = 5, //gói tin yêu cầu gửi tên tất cả user có trong database
        ALLUSERCONNECTED = 6, // gói tin yêu cầu gửi tên các user đã từng kết nối với user hiện tại
        CHATHISTORY = 7,//gói tin yêu cầu gửi lịch sử đoạn chat
        IMAGE = 8,//gói tin gửi ảnh 
        VIDEO = 9,//gói tin gửi video
        DATA = 10,//gói tin gửi tin nhắn
    }
}
