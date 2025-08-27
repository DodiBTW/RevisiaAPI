using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class CourseSql
{
    public static async Task<List<Course>> GetCoursesByUserIdAsync(int userId, MySqlConnection conn)
    {
        var courses = new List<Course>();
        var cmd = new MySqlCommand("SELECT * FROM Course WHERE UserId = @userId AND IsActive = 1 ORDER BY CreatedAt DESC", conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            courses.Add(new Course
            {
                Id = reader.GetInt32("Id"),
                UserId = reader.GetInt32("UserId"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString("Color"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive")
            });
        }
        return courses;
    }

    public static async Task<int> CreateCourseAsync(Course course, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Course (UserId, Name, Description, Color, CreatedAt, UpdatedAt, IsActive) 
                                    VALUES (@userId, @name, @description, @color, @createdAt, @updatedAt, @isActive);", conn);
        cmd.Parameters.AddWithValue("@userId", course.UserId);
        cmd.Parameters.AddWithValue("@name", course.Name);
        cmd.Parameters.AddWithValue("@description", course.Description);
        cmd.Parameters.AddWithValue("@color", course.Color);
        cmd.Parameters.AddWithValue("@createdAt", course.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", course.UpdatedAt);
        cmd.Parameters.AddWithValue("@isActive", course.IsActive);
        await cmd.ExecuteNonQueryAsync();
        return (int)cmd.LastInsertedId;
    }

    public static async Task UpdateCourseAsync(Course course, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"UPDATE Course SET Name = @name, Description = @description, 
                                    Color = @color, UpdatedAt = @updatedAt, IsActive = @isActive WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", course.Id);
        cmd.Parameters.AddWithValue("@name", course.Name);
        cmd.Parameters.AddWithValue("@description", course.Description);
        cmd.Parameters.AddWithValue("@color", course.Color);
        cmd.Parameters.AddWithValue("@updatedAt", course.UpdatedAt);
        cmd.Parameters.AddWithValue("@isActive", course.IsActive);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task<Course?> GetCourseByIdAsync(int courseId, int userId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT * FROM Course WHERE Id = @courseId AND UserId = @userId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        cmd.Parameters.AddWithValue("@userId", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Course
            {
                Id = reader.GetInt32("Id"),
                UserId = reader.GetInt32("UserId"),
                Name = reader.GetString("Name"),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString("Description"),
                Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString("Color"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                IsActive = reader.GetBoolean("IsActive")
            };
        }
        return null;
    }

    public static async Task DeleteCourseAsync(int courseId, MySqlConnection conn)
    {
        // Soft delete - set IsActive to false
        var cmd = new MySqlCommand("UPDATE Course SET IsActive = 0, UpdatedAt = @updatedAt WHERE Id = @courseId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task HardDeleteCourseAsync(int courseId, MySqlConnection conn)
    {
        // Hard delete - completely remove from database
        var cmd = new MySqlCommand("DELETE FROM Course WHERE Id = @courseId", conn);
        cmd.Parameters.AddWithValue("@courseId", courseId);
        await cmd.ExecuteNonQueryAsync();
    }
}
