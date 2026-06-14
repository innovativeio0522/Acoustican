using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Testimonial DTOs
public class TestimonialDto
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentRole { get; set; } = string.Empty;
    public string? StudentImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTestimonialDto
{
    [Required, StringLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [StringLength(200)]
    public string StudentRole { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    public bool IsPublished { get; set; } = false;

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }
}

public class UpdateTestimonialDto
{
    [Required, StringLength(200)]
    public string StudentName { get; set; } = string.Empty;

    [StringLength(200)]
    public string StudentRole { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Range(1, 5)]
    public int Rating { get; set; }

    public bool IsPublished { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }
}
