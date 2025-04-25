using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Data;
using Nito.AsyncEx;
namespace Server
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
            public string source { get; set; } = "";
            public string des { get; set; } = "";
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
        public class Receive_Form : Data_Form { }
        public class Client
        {
            public string username { get; set; } = "";
            public string password { get; set; } = "";
            public IPAddress realtime_IP { get; set; } = IPAddress.Loopback;
            public bool state { get; set; } = true;

            public Client(string username, string password)
            {
                this.username = username;
                this.password = password;
            }
            public Client(string username, string password, IPAddress realtime_IP)
            {
                this.username = username;
                this.password = password;
                this.realtime_IP = realtime_IP;
            }
            public Client() { }
        }
        public class Active_Client
        {
            public string name { get; set; }

            public TcpClient tcp { get; set; }

            public Active_Client(string name, TcpClient tcp)
            {
                this.name = name;
                this.tcp = tcp;
            }
            public Active_Client() { }
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
                //await writer.DisposeAsync();//giải phóng và đóng luồng ghi
            }
        }
        public static async Task<Receive_Form> Receive_Request(TcpClient client)//nhận tin từ server, giá trị bool trả về biểu hiện request có thực hiện thành công hay không
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
                //reader.Dispose();// giải phóng và đóng luồng đọc
            }
        }
        public static async Task Sign_Up_Request(TcpClient server, Receive_Form receive)// Xử lý 
        {
            string[] username_password = new string[2];
            if (receive.type == Type_Packet.SIGNUP)
            {
                username_password = receive.data.Split(',');
                // ghi vào data base
                {
                    var connect_To_Database = "Server=VUONGQUAN;Database=serverdatabase;Integrated Security=True;TrustServerCertificate=True;";
                    var dataset = new DataSet();
                    var query_Command = "select * from clientdatabase";
                    var adapter = new SqlDataAdapter(query_Command, connect_To_Database);
                    var builder = new SqlCommandBuilder(adapter);
                    adapter.Fill(dataset, "clientdatabase");
                    DataRow newRow = dataset.Tables["clientdatabase"].NewRow();
                    newRow["username"] = username_password[0];//username
                    newRow["password"] = username_password[1];//password
                    newRow["state"] = 1;
                    dataset.Tables["clientdatabase"].Rows.Add(newRow);
                    adapter.Update(dataset, "clientdatabase");
                }
                await Send_Request(server, new Send_Form(Type_Packet.MESSAGE, receive.source, receive.des, "sign up success!"));
            }
        }
        public static async Task Data_Request(Receive_Form receive)// luu lại tin nhan
        {
            if (receive.type == Type_Packet.DATA)
            {
                {//lưu vào database
                    var data_receive = receive.data;
                    var connect_To_Database = "Server=VUONGQUAN;Database=serverdatabase;Integrated Security=True;TrustServerCertificate=True;";
                    //var dataset = new DataSet();
                    //var query_Command = "select * from messdatabase";
                    //var adapter = new SqlDataAdapter(query_Command, connect_To_Database);
                    //var builder = new SqlCommandBuilder(adapter); //tu dong tao lenh insert...
                    //adapter.Fill(dataset, "messdatabase");
                    //DataRow newRow = dataset.Tables["messdatabase"].NewRow();
                    //newRow["message"] = receive.data;
                    //newRow["mess_From"] = receive.source;
                    //newRow["mess_To"] = receive.des;
                    //newRow["time"] = DateTime.Now;
                    //newRow["state"] = 1;
                    //Console.WriteLine(newRow);
                    //dataset.Tables["messdatabase"].Rows.Add(newRow);
                    //adapter.Update(dataset, "clientdatabase"); //chay xuong database
                    var conn = new SqlConnection(connect_To_Database);
                    conn.Open();
                    var command = new SqlCommand("insert into messdatabase(mess_from, mess_to, mess,  state, time) values (@from,@to,@mess,1,@time)");
                    command.Parameters.Add(new SqlParameter("from", receive.source));
                    command.Parameters.Add(new SqlParameter("to", receive.des));
                    command.Parameters.Add(new SqlParameter("mess", receive.data));
                    command.Parameters.Add(new SqlParameter("time", DateTime.Now));
                    command.ExecuteNonQuery();

                    conn.Close();

                }

            }
        }

        static async Task Main()
        {
            var server = new IPEndPoint(IPAddress.Loopback, 6000);
            var list_Client = new List<Active_Client>();
            var listenner = new TcpListener(server);
            listenner.Start(5);
            var list_Task = new List<Task>();
            Task accept = Task.Run(() => accept_Client(listenner, list_Client,list_Task));
            await accept_Client(listenner,list_Client,list_Task);
        }
        private static readonly AsyncReaderWriterLock list_lock = new AsyncReaderWriterLock();
        public static async Task accept_Client(TcpListener listenner, List<Active_Client> list_Client,List<Task> list_Task)
        {
            while (list_Client.Count<=5)
            {
                var active_Client = await listenner.AcceptTcpClientAsync();
                var receive = await Receive_Request(active_Client);
                using (await list_lock.WriterLockAsync())
                {
                    Console.WriteLine(list_Client.Count);
                    list_Client.Add(new Active_Client(receive.source, active_Client));
                }
                //active_Client.Dispose();
                await Sign_Up_Request(list_Client.Last().tcp, receive);
                Task main_task = Task.Run(async()=>process_Request(list_Client.Last(),list_Client));
                list_Task.Add(main_task);
            }
            await Task.WhenAll(list_Task);
        }
        public static async Task process_Request(Active_Client client, List<Active_Client> list_Client)
        {
            var receive = await Receive_Request(client.tcp);
            await Data_Request(receive);
            var des_client_name = new Active_Client();
            using (await list_lock.ReaderLockAsync())
            {
                foreach (var i in list_Client)
                {
                    Console.WriteLine(i.name);
                    if (i.name == receive.des)
                    {
                        des_client_name = i;
                        Console.WriteLine(":))");
                        break;
                    }
                }
            }
            if (receive.data == "")
            {
                Console.WriteLine("Nothing");
            }
            if(receive.data == null)
            {
                Console.WriteLine("xD");
            }
            Console.WriteLine($"{Type_Packet.DATA.ToString()} + receive.des + receive.source + receive.data");
            await Send_Request(des_client_name.tcp, new Send_Form(Type_Packet.DATA, receive.des, receive.source, receive.data));
        }
    }
}
