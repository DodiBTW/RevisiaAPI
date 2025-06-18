using System;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db
{
    public class RefreshTokenSql
    {
        public static async Task<bool> CreateRefreshTokenAsync(RefreshToken token, MySqlConnection conn)
        {
            var query = @"
                INSERT INTO RefreshTokens 
                (Id, UserId, TokenHash, LastTokenHash, IsInvalidated, InvalidatedAt, CreatedAt, ExpiresAt) 
                VALUES 
                (@Id, @UserId, @TokenHash, @LastTokenHash, 0, NULL, NOW(), @ExpiresAt)";

            await using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Id", token.Id);
            cmd.Parameters.AddWithValue("@UserId", token.UserId);
            cmd.Parameters.AddWithValue("@TokenHash", token.HashedValue);
            cmd.Parameters.AddWithValue("@LastTokenHash", (object?)token.LastTokenHash ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpiresAt", token.ExpiresAt);

            int affected = await cmd.ExecuteNonQueryAsync();
            return affected == 1;
        }

        public static async Task<bool> InvalidateRefreshTokenAsync(string tokenId, MySqlConnection conn)
        {
            var query = @"
                UPDATE RefreshTokens 
                SET IsInvalidated = 1, InvalidatedAt = NOW() 
                WHERE Id = @TokenId";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TokenId", tokenId);

            int affected = await cmd.ExecuteNonQueryAsync();
            return affected == 1;
        }

        public static async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, MySqlConnection conn)
        {
            var query = @"
                SELECT Id, UserId, TokenHash, LastTokenHash, IsInvalidated, InvalidatedAt, CreatedAt, ExpiresAt 
                FROM RefreshTokens 
                WHERE TokenHash = @TokenHash 
                  AND IsInvalidated = 0 
                  AND ExpiresAt > NOW()";

            await using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@TokenHash", tokenHash);

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                object idObj = reader["Id"];
                return new RefreshToken
                {
                    Id = idObj.ToString() ?? Guid.NewGuid().ToString(),
                    UserId = reader.GetInt32("UserId"),
                    HashedValue = reader.GetString("TokenHash"),
                    LastTokenHash = reader.IsDBNull("LastTokenHash") ? null : reader.GetString("LastTokenHash"),
                    IsInvalidated = reader.GetBoolean("IsInvalidated"),
                    InvalidatedAt = reader.IsDBNull("InvalidatedAt") ? null : reader.GetDateTime("InvalidatedAt"),
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    ExpiresAt = reader.GetDateTime("ExpiresAt")
                };
            }

            return null;
        }
    }
}
