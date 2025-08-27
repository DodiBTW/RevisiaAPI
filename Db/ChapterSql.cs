using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class ChapterSql
{
    public static async Task<List<Chapter>> GetChaptersByCourseIdAsync(int courseId, MySqlConnection conn)
    {
        var chapters = new List<Chapter>();
        var cmd = new MySqlCommand("SELECT * FROM Chapter WHERE CourseId = @courseId AND IsActive = 1 ORDER BY OrderIndex ASC, CreatedAt ASC", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            chapters.Add(new Chapter
            {
                Id = reader.GetInt32("Id"),
                CourseId = reader.GetInt32("CourseId"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString("Notes"),
                OrderIndex = reader.GetInt32("OrderIndex"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive")
            });
        }
        return chapters;
    }

    public static async Task<int> CreateChapterAsync(Chapter chapter, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Chapter (CourseId, Name, Description, Notes, OrderIndex, CreatedAt, UpdatedAt, IsActive) 
                                    VALUES (@courseId, @name, @description, @notes, @orderIndex, @createdAt, @updatedAt, @isActive);", conn);
        cmd.Parameters.AddWithValue("@courseId", chapter.CourseId);
        cmd.Parameters.AddWithValue("@name", chapter.Name);
        cmd.Parameters.AddWithValue("@description", chapter.Description);
        cmd.Parameters.AddWithValue("@notes", chapter.Notes);
        cmd.Parameters.AddWithValue("@orderIndex", chapter.OrderIndex);
        cmd.Parameters.AddWithValue("@createdAt", chapter.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", chapter.UpdatedAt);
        cmd.Parameters.AddWithValue("@isActive", chapter.IsActive);
        await cmd.ExecuteNonQueryAsync();
        return (int)cmd.LastInsertedId;
    }

    public static async Task UpdateChapterAsync(Chapter chapter, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"UPDATE Chapter SET Name = @name, Description = @description, Notes = @notes, OrderIndex = @orderIndex, 
                                    UpdatedAt = @updatedAt, IsActive = @isActive WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", chapter.Id);
        cmd.Parameters.AddWithValue("@name", chapter.Name);
        cmd.Parameters.AddWithValue("@description", chapter.Description);
        cmd.Parameters.AddWithValue("@notes", chapter.Notes);
        cmd.Parameters.AddWithValue("@orderIndex", chapter.OrderIndex);
        cmd.Parameters.AddWithValue("@updatedAt", chapter.UpdatedAt);
        cmd.Parameters.AddWithValue("@isActive", chapter.IsActive);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<Chapter?> GetChapterByIdAsync(int chapterId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT * FROM Chapter WHERE Id = @chapterId", conn);
        cmd.Parameters.AddWithValue("@chapterId", chapterId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Chapter
            {
                Id = reader.GetInt32("Id"),
                CourseId = reader.GetInt32("CourseId"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString("Notes"),
                OrderIndex = reader.GetInt32("OrderIndex"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
        return null;
    }

    public static async Task DeleteChapterAsync(int chapterId, MySqlConnection conn)
    {
        // Soft delete - set IsActive to false
        var cmd = new MySqlCommand("UPDATE Chapter SET IsActive = 0, UpdatedAt = @updatedAt WHERE Id = @chapterId", conn);
        cmd.Parameters.AddWithValue("@chapterId", chapterId);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task DeleteAllChaptersByCourseIdAsync(int courseId, MySqlConnection conn)
    {
        // Soft delete all chapters for a course
        var cmd = new MySqlCommand("UPDATE Chapter SET IsActive = 0, UpdatedAt = @updatedAt WHERE CourseId = @courseId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<int> GetNextOrderIndexAsync(int courseId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT COALESCE(MAX(OrderIndex), 0) + 1 FROM Chapter WHERE CourseId = @courseId AND IsActive = 1", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
