namespace Acoustican.Models;

public class Lesson
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int DurationSeconds { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public string Content { get; set; } = string.Empty; // Rich text lesson content
    public bool IsPublished { get; set; } = false;
    public bool IsPreview { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public CourseModule? Module { get; set; }
}
