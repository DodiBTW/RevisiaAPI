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
            string[] tags_to_string_array = reader.GetString("Tags").Split(',').ToArray();
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
                Tags = tags_to_string_array
            });
        }
        return cards;
    }

    public static async Task<int> CreateCardAsync(Card card, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"INSERT INTO Card (DeckId, Front, Back, Difficulty, `Interval`, NextReview, ReviewCount, Tags) VALUES (@deckId, @front, @back, @difficulty, @interval, @nextReview, @reviewCount, @tags);", conn);
        cmd.Parameters.AddWithValue("@deckId", card.DeckId);
        cmd.Parameters.AddWithValue("@front", card.Front);
        cmd.Parameters.AddWithValue("@back", card.Back);
        cmd.Parameters.AddWithValue("@difficulty", card.Difficulty);
        cmd.Parameters.AddWithValue("@interval", card.Interval);
        cmd.Parameters.AddWithValue("@nextReview", card.NextReview);
        cmd.Parameters.AddWithValue("@reviewCount", 0);
        cmd.Parameters.AddWithValue("@tags", string.Join(',', card.Tags));
        await cmd.ExecuteNonQueryAsync();
        return (int)cmd.LastInsertedId;
    }

    public static async Task UpdateCardAsync(Card card, MySqlConnection conn)
    {
        var cmd = new MySqlCommand(@"UPDATE Card SET DeckId = @deckId, Front = @front, Back = @back, Difficulty = @difficulty, `Interval` = @interval, NextReview = @nextReview, ReviewCount = @reviewCount, Tags = @tags WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", card.Id);
        cmd.Parameters.AddWithValue("@deckId", card.DeckId);
        cmd.Parameters.AddWithValue("@front", card.Front);
        cmd.Parameters.AddWithValue("@back", card.Back);
        cmd.Parameters.AddWithValue("@difficulty", card.Difficulty);
        cmd.Parameters.AddWithValue("@interval", card.Interval);
        cmd.Parameters.AddWithValue("@nextReview", card.NextReview);
        cmd.Parameters.AddWithValue("@reviewCount", card.ReviewCount ?? 0);
        cmd.Parameters.AddWithValue("@tags", string.Join(',', card.Tags));
        await cmd.ExecuteNonQueryAsync();
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
                Tags = reader.GetString("Tags").Split(',').ToArray()
            };
        }
        return null;
    }

    public static async Task DeleteCardAsync(int cardId, MySqlConnection conn)
    {
        var cmd = new MySqlCommand("DELETE FROM Card WHERE Id = @cardId", conn);
        cmd.Parameters.AddWithValue("@cardId", cardId);
        await cmd.ExecuteNonQueryAsync();
    }
}