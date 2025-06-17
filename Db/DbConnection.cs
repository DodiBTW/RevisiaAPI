using Microsoft.Extensions.Configuration;
using MySqlConnector;

public static class DbConnection
{
    private static string? _connectionString;

    public static void Init(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Connection string 'DefaultConnection' is missing.");
    }

    public static MySqlConnection GetConnection()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException("Database connection is not initialized. Call Init() first.");
        }

        return new MySqlConnection(_connectionString);
    }
}
