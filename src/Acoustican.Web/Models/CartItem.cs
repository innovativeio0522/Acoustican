namespace Acoustican.Models;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AdminUser User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
