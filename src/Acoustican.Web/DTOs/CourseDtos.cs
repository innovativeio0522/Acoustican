using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Course DTOs
public class CourseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Level { get; set; } = "Beginner";
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DurationMinutes { get; set; }
    public int LectureCount { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsBestseller { get; set; }
    public bool IsPublished { get; set; }
    public string WhatYoullLearn { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CourseReviewDto> Reviews { get; set; } = new();
    public List<CourseModuleDto> Modules { get; set; } = new();
}

public class CreateCourseDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Level { get; set; } = "Beginner";

    [Range(0, 99999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999.99)]
    public decimal OriginalPrice { get; set; }

    [Range(0, 100000)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public int LectureCount { get; set; }

    [StringLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }
    public bool IsBestseller { get; set; } = false;
    public bool IsPublished { get; set; } = false;

    [StringLength(5000)]
    public string WhatYoullLearn { get; set; } = string.Empty;

    [StringLength(5000)]
    public string Requirements { get; set; } = string.Empty;
}

public class UpdateCourseDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(5000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string Level { get; set; } = "Beginner";

    [Range(0, 99999.99)]
    public decimal Price { get; set; }

    [Range(0, 99999.99)]
    public decimal OriginalPrice { get; set; }

    [Range(0, 100000)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public int LectureCount { get; set; }

    [StringLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    [Range(0, 1000000)]
    public int StudentCount { get; set; }

    [Range(0, 5)]
    public decimal Rating { get; set; }

    [Range(0, 1000000)]
    public int ReviewCount { get; set; }

    public bool IsBestseller { get; set; }
    public bool IsPublished { get; set; }
    public string? VideoUrl { get; set; }

    [StringLength(5000)]
    public string WhatYoullLearn { get; set; } = string.Empty;

    [StringLength(5000)]
    public string Requirements { get; set; } = string.Empty;
}

public class CourseReviewDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int UserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
