using MySqlConnector;
using RevisiaAPI.Models;

namespace RevisiaAPI.Db;

public static class CardSql
{
    public static async Task<List<Card>> GetCardsByDeckIdAsync(int deckId, MySqlConnection conn)
    {
        var cards = new List<Card>();
        var cmd = new MySqlCommand("SELECT * FROM Card WHERE DeckId = @deckId", conn);
        cmd.Parameters.AddWithValue("@deckId", deckId);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cards.Add(new Card
            {
                Id = reader.GetInt32("Id"),
                DeckId = reader.GetInt32("DeckId"),
                Front = reader.GetString("Front"),
                Back = reader.GetString("Back"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                Difficulty = reader.GetDouble("Difficulty"),
                Interval = reader.GetDouble("Interval"),
                NextReview = reader.GetDateTime("NextReview"),
                ReviewCount = reader.GetInt32("ReviewCount"),
                Tags = reader.GetString("Tags").Split(',', StringSplitOptions.RemoveEmptyEntries)
            });
        }
        return cards;
    }

    public static async Task<int> CreateCardAsync(Card card, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Card (DeckId, Front, Back, CreatedAt, UpdatedAt, Difficulty, Interval, NextReview, ReviewCount, Tags)
            VALUES (@deckId, @front, @back, @createdAt, @updatedAt, @difficulty, @interval, @nextReview, @reviewCount, @tags);
            SELECT LAST_INSERT_ID();", conn);
        cmd.Parameters.AddWithValue("@deckId", card.DeckId);
        cmd.Parameters.AddWithValue("@front", card.Front);
        cmd.Parameters.AddWithValue("@back", card.Back);
        cmd.Parameters.AddWithValue("@createdAt", card.CreatedAt);
        cmd.Parameters.AddWithValue("@updatedAt", card.UpdatedAt);
        cmd.Parameters.AddWithValue("@difficulty", card.Difficulty);
        cmd.Parameters.AddWithValue("@interval", card.Interval);
        cmd.Parameters.AddWithValue("@nextReview", card.NextReview);
        cmd.Parameters.AddWithValue("@reviewCount", card.ReviewCount);
        cmd.Parameters.AddWithValue("@tags", string.Join(',', card.Tags));
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public static async Task<Card?> GetCardByIdAsync(int cardId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("SELECT * FROM Card WHERE Id = @cardId", conn);
        cmd.Parameters.AddWithValue("@cardId", cardId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Card
            {
                Id = reader.GetInt32("Id"),
                DeckId = reader.GetInt32("DeckId"),
                Front = reader.GetString("Front"),
                Back = reader.GetString("Back"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                Difficulty = reader.GetDouble("Difficulty"),
                Interval = reader.GetDouble("Interval"),
                NextReview = reader.GetDateTime("NextReview"),
                ReviewCount = reader.GetInt32("ReviewCount"),
                Tags = reader.GetString("Tags").Split(',', StringSplitOptions.RemoveEmptyEntries)
            };
        }
        return null;
    }
}