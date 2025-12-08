using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ElderProjectjr.Models
{
    public class DBConnect
    {
        private readonly string _connectionString;

        public DBConnect(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
