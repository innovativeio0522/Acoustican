using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

public class CartItemDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnailUrl { get; set; }
    public decimal CoursePrice { get; set; }
    public decimal CourseOriginalPrice { get; set; }
    public string CourseLevel { get; set; } = string.Empty;
    public string InstructorName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}

public class AddToCartRequest
{
    [Required]
    public int CourseId { get; set; }
}

public class SyncCartRequest
{
    [Required]
    public List<int> CourseIds { get; set; } = new();
}
