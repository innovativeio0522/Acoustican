using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Lesson DTOs
public class LessonDto
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }      // Full lesson — enrolled users only
    public string? PreviewVideoId { get; set; } // Short teaser — anyone can watch
    public string? ThumbnailUrl { get; set; }
    public int DurationSeconds { get; set; }
    public int DisplayOrder { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public bool IsPreview { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLessonDto
{
    [Required]
    public int ModuleId { get; set; }

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }      // Full lesson — enrolled users only
    public string? PreviewVideoId { get; set; } // Short teaser — anyone can watch
    public string? ThumbnailUrl { get; set; }

    [Range(0, 1000000)]
    public int DurationSeconds { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    [StringLength(50000)]
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public bool IsPreview { get; set; }
}

public class UpdateLessonDto
{
    [Required]
    public int ModuleId { get; set; }

    [Required, StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }      // Full lesson — enrolled users only
    public string? PreviewVideoId { get; set; } // Short teaser — anyone can watch
    public string? ThumbnailUrl { get; set; }

    [Range(0, 1000000)]
    public int DurationSeconds { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    [StringLength(50000)]
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public bool IsPreview { get; set; }
}

