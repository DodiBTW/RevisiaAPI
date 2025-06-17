using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class DeckSql
{
    public static async Task<List<Deck>> GetDecksByUserIdAsync(int userId, MySqlConnection conn)
    {
        var decks = new List<Deck>();
        var cmd = new MySqlCommand("SELECT * FROM Deck WHERE UserId = @userId", conn);
        cmd.Parameters.AddWithValue("@userId", userId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            decks.Add(new Deck
            {
                Id = reader.GetInt32("Id"),
                UserId = reader.GetInt32("UserId"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                Color = reader.GetString("Color"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                CardCount = reader.GetInt32("CardCount")
            });
        }
        return decks;
    }

    public static async Task<int> CreateDeckAsync(Deck deck, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Deck (UserId, Name, Description, Color, CreatedAt, UpdatedAt, CardCount)
            VALUES (@userId, @name, @description, @color, @createdAt, @updatedAt, @cardCount);
            SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@userId", deck.UserId);
        cmd.Parameters.AddWithValue("@name", deck.Name);
        cmd.Parameters.AddWithValue("@description", deck.Description);
        cmd.Parameters.AddWithValue("@color", deck.Color);
        cmd.Parameters.AddWithValue("@createdAt", deck.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", deck.UpdatedAt);
        cmd.Parameters.AddWithValue("@cardCount", deck.CardCount);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Ajoutez ici les méthodes Update, Delete, GetById si besoin
}