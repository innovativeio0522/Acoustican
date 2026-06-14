namespace Acoustican.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty; // Snapshot at purchase time
    public decimal Price { get; set; }                       // Snapshot at purchase time

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
