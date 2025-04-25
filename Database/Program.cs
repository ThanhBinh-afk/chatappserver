using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
namespace Database
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connection_database = "Server=VUONGQUAN;Database=serverdatabase;Trusted_Connection=True;TrustServerCertificate=True;";
            //using (var new_connect = new SqlConnection(connection_database))
            //{
            //    try
            //    {
            //        new_connect.Open();
            //        Console.WriteLine("Connection complete");
            //    }
            //    catch(Exception ex)
            //    {
            //        Console.WriteLine($"Exception : {ex.Message}");
            //    }

            //string them_tk = "insert into DANHSACHCLIENT (TenTaiKhoan,MatKhau,TrangThai) values (@tk,@mk,@tt)";
            //using (var them = new SqlCommand(them_tk, new_connect))
            //{
            //    them.Parameters.AddWithValue("@tk", "LinhKhanh");
            //    them.Parameters.AddWithValue("@mk", "123456");
            //    them.Parameters.AddWithValue("@tt", false);
            //    them.ExecuteNonQuery();
            //}

            //string xoa_tk = "delete from DANHSACHCLIENT where TenTaiKhoan = @tk ";
            //using (var xoa = new SqlCommand(xoa_tk, new_connect))
            //{
            //    xoa.Parameters.AddWithValue("@tk", "VuongQuan");
            //    xoa.ExecuteNonQuery();
            //}

            //string truyvan_tk = "select * from DANHSACHCLIENT where TrangThai = true ";
            //using (var cmd = new SqlCommand(truyvan_tk, new_connect))
            //{
            //    using (var reader = cmd.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            Console.WriteLine($"{reader["TenTaiKhoan"]},{reader["MatKhau"]},{reader["TrangThai"]}");
            //        }
            //    }
            //}
            string query_command = "select * from DANHSACHCLIENT";
            var data_set = new DataSet();
            var adapter = new SqlDataAdapter(query_command,connection_database);
            var builder_command = new SqlCommandBuilder(adapter);
            adapter.Fill(data_set, "DANHSACHCLIENT");

            foreach(var row in data_set.Tables["DANHSACHCLIENT"].Rows)
            {
                var r = row as DataRow;
                foreach(var col in data_set.Tables["DANHSACHCLIENT"].Columns)
                {
                    var c = col as DataColumn;
                    Console.Write($"{r[c.ColumnName]} ");
                }
                Console.WriteLine();
            }
            adapter.Update(data_set, "DANHSACHCLIENT");
        }
    }

}
