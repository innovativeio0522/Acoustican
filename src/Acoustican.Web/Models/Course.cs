namespace Acoustican.Models;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DurationMinutes { get; set; }
    public int LectureCount { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int StudentCount { get; set; } = 0;
    public decimal Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public bool IsBestseller { get; set; } = false;
    public bool IsPublished { get; set; } = false;
    public string WhatYoullLearn { get; set; } = string.Empty; // Comma-separated list
    public string Requirements { get; set; } = string.Empty; // Comma-separated list
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<CourseReview> Reviews { get; set; } = [];
}