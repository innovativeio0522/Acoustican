namespace Acoustican.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled
    public string? PaymentId { get; set; }           // Future: Razorpay/Stripe ID
    public string? RazorpayOrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AdminUser User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
}
