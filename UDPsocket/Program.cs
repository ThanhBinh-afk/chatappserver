using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Nito.AsyncEx;
using System.Data;
namespace Server
{
    internal class Program
    {

        private class User_Infor
        {
            public string user_Name { get; set; }
            public TcpClient tcp_client { get; set; }
            public bool State { get; set; } = true;
            public User_Infor(string user_Name, TcpClient tcp_client)
            {
                user_Name = user_Name;
                this.tcp_client = tcp_client;
            }
            ~User_Infor()
            {
                this.tcp_client.Dispose();
            }
            public IPAddress getIPv4()
            {
                var client_ipendpoint = this.tcp_client.Client.LocalEndPoint as IPEndPoint;
                if (client_ipendpoint == null)
                    return null;
                else
                    return client_ipendpoint.Address;
            }
            public int getPort()
            {
                var client_ipendpoint = this.tcp_client.Client.LocalEndPoint as IPEndPoint;
                if (client_ipendpoint == null)
                    return -1;
                else
                    return client_ipendpoint.Port;
            }
        }
        static async Task Main(string[] args)
        {
            var server_endpoint = new IPEndPoint(IPAddress.Loopback, 6000);
            var listenner = new TcpListener(server_endpoint);
            listenner.Start(5);
            var list_Client = new List<User_Infor>();
            var list_Task = new List<Task>();
            while (list_Client.Count <= 5)
            {
                var tcp_client = await listenner.AcceptTcpClientAsync();
                foreach (var i in list_Client)
                {
                    Console.WriteLine(i.user_Name);
                }
                Console.Write($"New Connection is completed from {tcp_client.Client.RemoteEndPoint}!\nEnter client's name : ");
                string Name = Console.ReadLine();
                list_Client.Add(new User_Infor(Name, tcp_client));
                // lay user tu db len
                // lay toan bo tin nhan cua moi nguoi gui den user nay va hien thi nó len
                list_Task.Add(Task.Run(async () =>
                {
                    await Handle_Client_Request(list_Client.Last(), list_Client);
                }));
            }
            await Task.WhenAll(list_Task);
            listenner.Stop();
        }
        private static async Task Handle_Client_Request(User_Infor client, List<User_Infor> list_Client)
        {
            var reader = new StreamReader(client.tcp_client.GetStream(), Encoding.ASCII);
            var writer = new StreamWriter(client.tcp_client.GetStream(), Encoding.ASCII);
            writer.AutoFlush = true;
            Task reading = Task.Run(async () =>
            {
                while (true)
                {
                    var data_string = await reader.ReadLineAsync();
                    Console.WriteLine(data_string);
                    if (data_string == "off")
                    {
                        list_Client.Remove(client);
                    }
                    else
                    {
                        var data_json = JsonConvert.DeserializeObject<Data_json>(data_string);
                        var des_client = find_ClientAddress(data_json.client_des, list_Client);
                        // luu tin nhan vao db
                        if (data_json.client_des == null)
                        {
                            Console.WriteLine($"Destination client({data_json.client_des}) don't exsit or client isn't online");
                        }
                        else
                        {
                            var write_to_des = new StreamWriter(des_client.GetStream(), Encoding.ASCII);
                            write_to_des.AutoFlush = true;
                            await write_to_des.WriteLineAsync(data_json.data);
                            Console.WriteLine($"Sending data complete to from {client.tcp_client.Client.RemoteEndPoint} to {des_client.Client.RemoteEndPoint}");
                            Console.WriteLine("Information : " + data_json.data);
                        }
                    }
                }
            });
            await reading;
            reader.Dispose();
            writer.Dispose();
        }
        private static string[] getDes_realData(string data)
        {
            return new string[2] { data.Trim().Substring(0, data.Trim().IndexOf(" ")), data.Trim().Substring(data.Trim().IndexOf(" ") + 1) };
        }
        private static TcpClient find_ClientAddress(string Name, List<User_Infor> list_client)
        {
            foreach (var i in list_client)
            {
                if (i.user_Name.Equals(Name))
                    return i.tcp_client;
            }
            return null;
        }
    }
}
