using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;

namespace Client
{
    internal class Program
    {
        public enum Type_Packet//Loại gói tin
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
        public class Data_Form//Định dạng gói tin 
        {
            public Type_Packet type { get; set; } = Type_Packet.DATA;//loại gói tin : kết nối , thông báo , gửi dữ liệu ,...
            public string data { get; set; } = "";//nội dung gói tin
            public string source { get; set; } = "";//nơi gửi 
            public string des { get; set; } = "";//nơi nhận
        }
        public class Send_Form : Data_Form
        {
            public Send_Form(Type_Packet type, string des, string source, string data)
            {
                this.type = type;
                this.des = des;
                this.source = source;
                this.data = data;
            }
        }
        public class Receive_Form : Data_Form
        {
        }
        public static async Task Send_Request(TcpClient client, Send_Form send_form)//gửi tin đến server
        {
            var Net_Stream = client.GetStream();// tạo luồng network
            var writer = new StreamWriter(Net_Stream, Encoding.Unicode);//tạo luồng ghi
            writer.AutoFlush = true;//tự động đẩy dữ liệu trong buffer gửi đi 
            try
            {
                var send_data = JsonConvert.SerializeObject(send_form);//chuyển Send_Form sang dạng string
                await writer.WriteLineAsync(send_data);//gửi chuỗi đã chuyển sang string lên server
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);//in ra ngoại lệ nếu có 
            }
            finally
            {
                //writer.Close();//giải phóng và đóng luồng ghi
            }
        }
        public static async Task<Receive_Form> Receive_Request(TcpClient client)//nhận tin từ server
        {
            var Net_Stream = client.GetStream();// tạo luồng network stream
            var reader = new StreamReader(Net_Stream, Encoding.Unicode);//tạo luồng đọc 
            var raw_data = await reader.ReadLineAsync();//lấy chuỗi tin từ server
            try
            {
                var receive_data = JsonConvert.DeserializeObject<Receive_Form>(raw_data);//chuyển chuỗi về kiểu data nhận
                if (receive_data.type == Type_Packet.DATA)// nếu là dạng tin gửi data
                {
                    Console.WriteLine($"{receive_data.source} : {receive_data.data}");//thì in ra người gửi : tin nhắn
                }
                return receive_data;
            }
            catch (Exception ex)// bắt lỗi khi không chuyển được sang Receive_Form
            {
                Console.WriteLine(ex.Message);// in ra thông báo lỗi
                return new Receive_Form();
            }
            finally
            {
               // reader.Close();// giải phóng và đóng luồng đọc
            }
        }
        public static async Task<string> Log_In(TcpClient client)//hàm xử lý khi người dùng log in 
        {
            Console.Write("Enter username : ");
            string username = Console.ReadLine();//nhập username
            Console.Write("Enter password : ");
            string password = Console.ReadLine();//nhập mật khẩu
            await Send_Request(client, new Send_Form(Type_Packet.LOGIN, "Server", username, username + "," + password));// gửi yêu cầu đăng nhập lên server
            var receive_data = await Receive_Request(client);//nhận trả lời từ server
            if (receive_data.type == Type_Packet.MESSAGE)
            {
                Console.WriteLine(receive_data.data);//in ra thông báo từ server(thành công/không thành công) (=> GUI : message box)
            }
            return username;
        }
        public static void Disconnect(TcpClient client)//hàm đóng kết nối 
        {// GUI => message box có muốn đóng kết nối không ?
            client.Dispose();//giải phóng và đóng kết nối
        }
        public static async Task Delete_User(TcpClient client, string source)//hàm xoá người dùng
        {
            await Send_Request(client, new Send_Form(Type_Packet.DELETE, "Server", source, "delete account"));
            var receiver_data = await Receive_Request(client);
            if (receiver_data.type == Type_Packet.MESSAGE)
            {
                Console.WriteLine(receiver_data);// in ra thông báo từ server (GUI => mess box)
            }
        }
        public static async Task<string> Sign_Up(List<string> user_Exsits, TcpClient client)//Đăng ký tài khoản mới
        {
            bool isExsit = false;//biến lưu xem username đã tồn tại chưa
            string username = "";//khởi tạo chuỗi lưu username
            do
            {
                Console.Write("Enter user's name : ");
                username = Console.ReadLine();//nhập username
                foreach (var name in user_Exsits)//duyệt kiểm tra username
                {
                    if (name == username)
                    {
                        isExsit = true;//username đã tồn tại
                        Console.WriteLine($"{username} is exsist");//thông báo cho người dùng biết 
                        break;
                    }
                }
            } while (isExsit);//nhập lại username
            Console.Write("Enter password : ");
            string password = Console.ReadLine();//nhập password
            await Send_Request(client, new Send_Form(Type_Packet.SIGNUP, "Server", username, username + "," + password));//gửi yêu cầu tạo tài khoản với username và password đã nhập
            var receive_data = await Receive_Request(client);//nhận trả lời từ server
            if (receive_data.type == Type_Packet.MESSAGE)
            {
                Console.WriteLine(receive_data.data);//in ra thông báo từ server (GUI => mess box)
            }
            return username;
        }
        public static async Task<List<string>> Get_All_User(TcpClient client, string source, bool connected = false)//lấy về list tên các user đã đăng ký
        {
            if (!connected)
                await Send_Request(client, new Send_Form(Type_Packet.ALLUSER, "Server", source, "get all username in database"));//gửi yêu cầu lấy tên tất cả user có trong database
            else
                await Send_Request(client, new Send_Form(Type_Packet.ALLUSERCONNECTED, "Server", source, "get all username which was connected to this user"));//gửi yêu cầu lấy tên tất cả user có trong database đã từng kết nối đến user hiện tại
            var receive_data = await Receive_Request(client);
            var username = receive_data.data;
            if (username == null)// kiểm tra dữ liệu null 
            {
                Console.WriteLine("Server is down");//Server có lỗi (GUI => mess box lỗi)
                return new List<string>();// trả về list rỗng<null>
            }
            else
            {
                var array_User = username.Trim().Split(",");//tách các username (username có dạng : [username1],[username2],....) 
                return new List<string>(array_User);//trả về list username
            }
        }
        public static async Task Connect_To_Server(TcpClient client, IPEndPoint server)//kết nối đến server
        {
            try
            {
                client.Connect(server);//thử kết nối đến server
            }
            catch (SocketException se)//lỗi kết nối server
            {
                Console.WriteLine(se.Message);//in ra lỗi (GUI => mess box lỗi)
            }
        }
        public static async Task Show_Chat(TcpClient client, string des, string source)//hiển thị đoạn chat đã tồn tại của des với user hiện tại
        {
            await Send_Request(client, new Send_Form(Type_Packet.CHATHISTORY, des, source, "get chat history"));
            var receive_data = new Receive_Form();
            do
            {
                receive_data = await Receive_Request(client);
                Console.WriteLine(receive_data);//load tin nhắn lên giao diện;[datetime][username][:data];
            } while (receive_data.type == Type_Packet.CHATHISTORY);
            Console.WriteLine(receive_data.data);//đã load xong lịch sử chat
        }
        public static IPEndPoint Get_Server_IP()
        {
            var server_IP = new IPEndPoint(IPAddress.Any, 6000);
            var listenner = new UdpClient();
            byte[] buffer_Receive = new byte[1024];
            while (true)
            {
                buffer_Receive = listenner.Receive(ref server_IP);
                string string_Receive = Encoding.Unicode.GetString(buffer_Receive);
                if (string_Receive == "CorrectEndPoint")
                    break;
                Array.Clear(buffer_Receive, 0, 1024);
            }
            return server_IP;
        }
        static async Task Main(string[] args)
        {
            var server = new IPEndPoint(IPAddress.Loopback, 6000);//tạo điểm kết nối là server 
            //var server = new IPEndPoint(Get_Server_IP,6000); // server có ip bất kỳ
            var client = new TcpClient();// khai báo 1 socket với protocol là tcp (socket như là đường ống nối gữi 2 điểm trên mạng) 
            Connect_To_Server(client, server);//kết nối đến server
            //var list_User = await Get_All_User(client);//lấy về list các tên user đã được sử dụng
            //await Menu_Signup_Login_ShowChat(client, list_User);// lựa chọn đang nhập hoặc tạo tài khoản
            string username = await Sign_Up_Test(client);
            var cts = new CancellationTokenSource();//tạo dấu hiệu dừng hàm đồng bộ 
            var ct = cts.Token;//lấy token điều khiển huỷ đồng bộ
            Task writing = Task.Run(async () =>//tạo 1 task chuyên để nhận dữ liệu từ Server
            {
                while (!ct.IsCancellationRequested)//kiểm tra xem có dấu hiệu dừng hay không
                {
                    Console.Write("enter destination username : ");
                    string des = Console.ReadLine();//nhập người muốn gửi tin tới
                    Console.Write("enter your message : ");
                    string data = Console.ReadLine();//nhập tin nhắn
                    if (data == "close connection" && des == "cmd")//lệnh người dùng sử dụng để huỷ kết nối
                    {
                        cts.Cancel();//gửi yêu cầu huỷ đồng bộ 
                        Disconnect(client);//ngắt kết nối
                        break;
                    }
                    else if (data == "delete user" && des == "cmd")//lệnh người dùng xoá tài khoản
                    {
                        cts.Cancel();//gửi yêu cầu huỷ đồng bộ
                        await Delete_User(client, username);//xoá user
                        break;
                    }
                    else
                    {
                        await Send_Request(client, new Send_Form(Type_Packet.DATA, des, username, data));//gửi tin
                    }
                }
            });
            Task reading = Task.Run(async () =>//tạo task chuyên đọc dữ liệu từ server
            {
                while (!ct.IsCancellationRequested)//kiểm tra xem có dấu hiệu dừng hay không 
                {
                    await Receive_Request(client);//nhận dữ liệu
                }
            });
            await Task.WhenAll(reading, writing);// chờ cho cả 2 task thực hiện xong việc
            Disconnect(client);// ngắt kết nối
        }
        private static async Task Menu_Signup_Login_ShowChat(TcpClient client, List<string> list_User)//lựa chọn tạo tài khoản / đăng nhập / xem lịch sử chat
        {
            //Console.Write("bạn muốn đăng nhập(1) hay tạo tài khoản(2) : ");
            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    await Log_In(client);
                    break;
                case 2:
                    await Sign_Up(list_User, client);
                    break;
                default:
                    Console.WriteLine("enter 1 or 2");
                    break;
            }
        }
        public static async Task<string> Sign_Up_Test(TcpClient client)//Đăng ký tài khoản mới
        {
            Console.Write("Enter username : ");
            string username = Console.ReadLine();
            Console.Write("Enter password : ");
            string password = Console.ReadLine();
            await Send_Request(client, new Send_Form(Type_Packet.SIGNUP, "Server", username, username + "," + password));//gửi yêu cầu tạo tài khoản với username và password đã nhập
            var receive_data = await Receive_Request(client);//nhận trả lời từ server
            if (receive_data.type == Type_Packet.MESSAGE)
            {
                Console.WriteLine(receive_data.data);//in ra thông báo từ server (GUI => mess box)
            }
            return username;
        }
    }
}

