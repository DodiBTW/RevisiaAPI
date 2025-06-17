namespace RevisiaAPI.Models;

public class Card
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public double Difficulty { get; set; }
    public double Interval { get; set; }
    public DateTime NextReview { get; set; }
    public int ReviewCount { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}   