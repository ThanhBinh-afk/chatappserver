using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Client3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server_endpoint = new IPEndPoint(IPAddress.Loopback, 6000);
            var client_server_stream = new TcpClient();
            Console.WriteLine($"Client is connecting to {server_endpoint.ToString()}");
            client_server_stream.Connect(server_endpoint);
            Console.WriteLine("Connect completed!");
            var cancel_source_reading = new CancellationTokenSource();
            var cancel_token_reading = cancel_source_reading.Token;
            var cancel_source_writing = new CancellationTokenSource();
            var cancel_token_writing = cancel_source_writing.Token;
            var reader = new StreamReader(client_server_stream.GetStream(), Encoding.ASCII);
            var writer = new StreamWriter(client_server_stream.GetStream(), Encoding.ASCII);
            writer.AutoFlush = true;
            Task reading = Task.Run(async () =>
            {
                while (true)
                {
                    string data = await reader.ReadLineAsync();
                    if (data == "exit")
                    {
                        cancel_source_reading.Cancel();
                        Console.WriteLine("receive closing connection signal");
                    }
                    if (cancel_token_reading.IsCancellationRequested)
                        break;
                    Console.WriteLine($" : {data}");
                }
            });
            Task writing = Task.Run(async () =>
            {
                while (true)
                {
                    Console.Write("Enter client's name you want to sending data : ");
                    string connect_toClient = Console.ReadLine();
                    Console.Write("Enter data to send : ");
                    string Data = Console.ReadLine();
                    var data_object = new { type = "send data", client_des = connect_toClient, data = Data };
                    string data_json = JsonConvert.SerializeObject(data_object);
                    Console.WriteLine(data_json);
                    if (Data == "exit")
                    {
                        cancel_source_writing.Cancel();
                        Console.WriteLine("Sending closing connection signal ... ");
                    }
                    await writer.WriteLineAsync(data_json);
                    if (cancel_token_writing.IsCancellationRequested)
                        break;
                    Console.WriteLine($"Client : {Data}");
                }
            });
            await Task.WhenAll(reading, writing);
            Console.WriteLine("disconnect complete");
            client_server_stream.Close();
            reader.Close();
            writer.Close();
        }
        private static string[] getSource_realData(string data)
        {
            return new string[2] { data.Trim().Substring(data.Trim().LastIndexOf(' ') + 1), data.Trim().Substring(0, data.Trim().LastIndexOf(' ')) };
        }
    }
}
