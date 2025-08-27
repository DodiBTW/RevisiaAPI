namespace RevisiaAPI.Models;

public class CourseDeck
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int DeckId { get; set; }
    public DateTime? CreatedAt { get; set; }
}
