namespace RevisiaAPI.Models;

public class Review
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public int UserId { get; set; }
    public bool Remembered { get; set; }
    public DateTime ReviewedAt { get; set; }
    public double PreviousInterval { get; set; }
    public double NewInterval { get; set; }
}