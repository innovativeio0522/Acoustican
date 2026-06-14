namespace Acoustican.Models;

public class Testimonial
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentRole { get; set; } = string.Empty; // e.g., "Hobby Guitarist", "Professional Musician"
    public string? StudentImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; } = 5; // 1-5 stars
    public bool IsPublished { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
