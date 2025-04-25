using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAT_Server
{
    public class DAL_Connect
    {
        protected SqlConnection connect = new SqlConnection("Server=VUONGQUAN;Database=serverdatabase;Trusted_Connection=True;TrustServerCertificate=True;");
        //kết nối đến serverdatabase
    }
}
