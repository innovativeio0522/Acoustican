namespace Acoustican.Models;

public class CourseModule
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int ModuleNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Course? Course { get; set; }
    public ICollection<Lesson> Lessons { get; set; } = [];
}
