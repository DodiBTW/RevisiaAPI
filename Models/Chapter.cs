namespace RevisiaAPI.Models;

public class Chapter
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; } // For ordering chapters within a course
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; } // Markdown notes, max 10k characters validated in code
}
