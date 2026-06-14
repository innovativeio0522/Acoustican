namespace Acoustican.Models;

public class PricingFeature
{
    public int Id { get; set; }
    public int PricingTierId { get; set; }
    public string Feature { get; set; } = string.Empty;
    public bool IsIncluded { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;

    // Navigation properties
    public PricingTier? PricingTier { get; set; }
}
