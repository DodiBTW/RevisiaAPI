using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class CourseDeckSql
{
    public static async Task<int> AddDeckToCourseAsync(int courseId, int deckId, MySqlConnection conn)
    {
        // Check if relationship already exists
        var existingCmd = new MySqlCommand("SELECT COUNT(*) FROM CourseDeck WHERE CourseId = @courseId AND DeckId = @deckId", conn);
        existingCmd.Parameters.AddWithValue("@courseId", courseId);
        existingCmd.Parameters.AddWithValue("@deckId", deckId);
        var exists = Convert.ToInt32(await existingCmd.ExecuteScalarAsync()) > 0;
        
        if (exists)
        {
            return 0; // Already exists, return 0
        }

        var cmd = new MySqlCommand(@"INSERT INTO CourseDeck (CourseId, DeckId, CreatedAt) 
                                    VALUES (@courseId, @deckId, @createdAt);", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        cmd.Parameters.AddWithValue("@deckId", deckId);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
        return (int)cmd.LastInsertedId;
    }

    public static async Task RemoveDeckFromCourseAsync(int courseId, int deckId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("DELETE FROM CourseDeck WHERE CourseId = @courseId AND DeckId = @deckId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        cmd.Parameters.AddWithValue("@deckId", deckId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<List<int>> GetDeckIdsByCourseIdAsync(int courseId, MySqlConnection conn)
    {
        var deckIds = new List<int>();
        var cmd = new MySqlCommand("SELECT DeckId FROM CourseDeck WHERE CourseId = @courseId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            deckIds.Add(reader.GetInt32("DeckId"));
        }
        return deckIds;
    }

    public static async Task<List<int>> getCourseIdsByDeckIdAsync(int deckId, MySqlConnection conn)
    {
        var courseIds = new List<int>();
        var cmd = new MySqlCommand("SELECT CourseId FROM CourseDeck WHERE DeckId = @deckId", conn);
        cmd.Parameters.AddWithValue("@deckId", deckId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            courseIds.Add(reader.GetInt32("CourseId"));
        }
        return courseIds;
    }

    public static async Task RemoveAllDeckRelationshipsAsync(int deckId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("DELETE FROM CourseDeck WHERE DeckId = @deckId", conn);
        cmd.Parameters.AddWithValue("@deckId", deckId);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task RemoveAllCourseRelationshipsAsync(int courseId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("DELETE FROM CourseDeck WHERE CourseId = @courseId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        await cmd.ExecuteNonQueryAsync();
    }
}
