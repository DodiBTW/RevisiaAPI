using MySqlConnector;
using RevisiaAPI.Models;
namespace RevisiaAPI.Db
{
    public class UserSql
    {
        public static async Task<bool> UserExistsAsync(string username, string email, MySqlConnection conn)
        {
            var cmd = new MySqlCommand("SELECT COUNT(*) FROM User WHERE Username = @username OR Email = @email", conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email", email);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public static async Task<bool> CreateUserAsync(string username, string email, string passwordHash, MySqlConnection conn)
        {
            if (await UserExistsAsync(username, email, conn)) return false;

            var cmd = new MySqlCommand("INSERT INTO User (Username, Email, PasswordHash) VALUES (@username, @email, @passwordHash)", conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@passwordHash", passwordHash);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        public static async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail, MySqlConnection conn)
        {
            var cmd = new MySqlCommand("SELECT id, username, email, PasswordHash FROM User WHERE Username = @usernameOrEmail OR Email = @usernameOrEmail", conn);
            cmd.Parameters.AddWithValue("@usernameOrEmail", usernameOrEmail);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Email = reader.GetString("email"),
                    PasswordHash = reader.GetString("password_hash")
                };
            }
            return null;
        }
    }
}
