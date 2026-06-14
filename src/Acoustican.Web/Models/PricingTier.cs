namespace Acoustican.Models;

public class PricingTier
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Starter", "Professional", "Enterprise"
    public decimal Price { get; set; }
    public string BillingPeriod { get; set; } = "monthly"; // monthly, yearly
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsPopular { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<PricingFeature> Features { get; set; } = [];
}
