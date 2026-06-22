using System;

namespace Acoustican.Models;

public class CourseReview
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; } // Range: 1 to 5
    public string Comment { get; set; } = string.Empty; // Max 1000 characters
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Course Course { get; set; } = null!;
    public AdminUser User { get; set; } = null!;
}
