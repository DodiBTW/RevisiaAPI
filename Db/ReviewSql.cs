using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class ReviewSql
{
    public static async Task<List<Review>> GetReviewsByUserIdAsync(int userId, MySqlConnection conn)
    {
        var reviews = new List<Review>();
        var cmd = new MySqlCommand("SELECT * FROM Review WHERE UserId = @userId", conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            reviews.Add(new Review
            {
                Id = reader.GetInt32("Id"),
                CardId = reader.GetInt32("CardId"),
                UserId = reader.GetInt32("UserId"),
                Remembered = reader.GetBoolean("Remembered"),
                ReviewedAt = reader.GetDateTime("ReviewedAt"),
                PreviousInterval = reader.GetDouble("PreviousInterval"),
                NewInterval = reader.GetDouble("NewInterval")
            });
        }
        return reviews;
    }

    public static async Task<int> CreateReviewAsync(Review review, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Review (CardId, UserId, Remembered, ReviewedAt, PreviousInterval, NewInterval)
            VALUES (@cardId, @userId, @remembered, @reviewedAt, @previousInterval, @newInterval);
            SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@cardId", review.CardId);
        cmd.Parameters.AddWithValue("@userId", review.UserId);
        cmd.Parameters.AddWithValue("@remembered", review.Remembered);
        cmd.Parameters.AddWithValue("@reviewedAt", review.ReviewedAt);
        cmd.Parameters.AddWithValue("@previousInterval", review.PreviousInterval);
        cmd.Parameters.AddWithValue("@newInterval", review.NewInterval);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Ajoutez ici les méthodes Update, Delete, GetById si besoin
}