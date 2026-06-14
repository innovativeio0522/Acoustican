using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Module DTOs
public class CourseModuleDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int ModuleNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LessonDto> Lessons { get; set; } = new();
}

public class CreateCourseModuleDto
{
    [Required]
    public int CourseId { get; set; }

    [Range(0, 1000)]
    public int ModuleNumber { get; set; }

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; } = false;
}

public class UpdateCourseModuleDto
{
    [Range(0, 1000)]
    public int ModuleNumber { get; set; }

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int DurationMinutes { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
}
