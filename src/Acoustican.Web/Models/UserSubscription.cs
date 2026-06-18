namespace Acoustican.Models;

public class UserSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PricingTierId { get; set; }
    public string Status { get; set; } = "active"; // pending, active, cancelled
    public string? RazorpayOrderId { get; set; }
    public string? PaymentId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AdminUser User { get; set; } = null!;
    public PricingTier PricingTier { get; set; } = null!;
}
