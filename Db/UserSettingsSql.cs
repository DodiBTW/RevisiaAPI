using MySqlConnector;
using RevisiaAPI.Models;
using System.Threading.Tasks;

namespace RevisiaAPI.Db
{
    public static class UserSettingsSql
    {
        public static async Task<UserSettings?> GetByUserIdAsync(int userId, MySqlConnection conn)
        {
            var cmd = new MySqlCommand("SELECT * FROM UserSettings WHERE UserId = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new UserSettings
                {
                    userId = reader.GetInt32("userId"),
                    rememberMultiplier = reader.GetDouble("rememberMultiplier"),
                    forgotMultiplier = reader.GetDouble("forgotMultiplier"),
                    maxInterval = reader.GetInt32("maxInterval"),
                    dailyGoal = reader.GetInt32("dailyGoal"),
                    language = reader.GetString("language"),
                };
            }
            return null;
        }

        public static async Task<bool> InsertSettings(UserSettings settings, MySqlConnection conn)
        {
            var cmd = new MySqlCommand(@"
                INSERT INTO UserSettings (UserId, RememberMultiplier, ForgotMultiplier, MaxInterval, DailyGoal, Language)
                VALUES (@userId, @rememberMultiplier, @forgotMultiplier, @maxInterval, @dailyGoal, @language)
                ON DUPLICATE KEY UPDATE
                    RememberMultiplier = @rememberMultiplier,
                    ForgotMultiplier = @forgotMultiplier,
                    MaxInterval = @maxInterval,
                    DailyGoal = @dailyGoal;", conn);

            cmd.Parameters.AddWithValue("@userId", settings.userId);
            cmd.Parameters.AddWithValue("@rememberMultiplier", settings.rememberMultiplier);
            cmd.Parameters.AddWithValue("@forgotMultiplier", settings.forgotMultiplier);
            cmd.Parameters.AddWithValue("@maxInterval", settings.maxInterval);
            cmd.Parameters.AddWithValue("@dailyGoal", settings.dailyGoal);
            cmd.Parameters.AddWithValue("@language", settings.language ?? "en");

            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        public static async Task<bool> UpdateSettings(UserSettings settings, MySqlConnection conn)
        {
            var cmd = new MySqlCommand(@"
                UPDATE UserSettings
                SET RememberMultiplier = @rememberMultiplier,
                    ForgotMultiplier = @forgotMultiplier,
                    MaxInterval = @maxInterval,
                    DailyGoal = @dailyGoal,
                    Language = @language
                WHERE UserId = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", settings.userId);
            cmd.Parameters.AddWithValue("@rememberMultiplier", settings.rememberMultiplier);
            cmd.Parameters.AddWithValue("@forgotMultiplier", settings.forgotMultiplier);
            cmd.Parameters.AddWithValue("@maxInterval", settings.maxInterval);
            cmd.Parameters.AddWithValue("@dailyGoal", settings.dailyGoal);
            cmd.Parameters.AddWithValue("@language", settings.language ?? "en");
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
