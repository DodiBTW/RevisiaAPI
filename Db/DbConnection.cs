using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Threading.Tasks;
namespace RevisiaAPI.Db
{
    public static class DbConnection
    {
        private static string? _connectionString;

        public static void Init(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                                ?? throw new Exception("Connection string missing");
        }

        public static MySqlConnection GetConnection()
        {
            if (_connectionString == null)
                throw new Exception("DB connection string not initialized. Call Init first.");

            return new MySqlConnection(_connectionString);
        }
    }

}
