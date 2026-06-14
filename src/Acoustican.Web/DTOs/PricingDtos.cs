using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Pricing DTOs
public class PricingTierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BillingPeriod { get; set; } = "monthly";
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsPopular { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public List<PricingFeatureDto> Features { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreatePricingTierDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999.99)]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string BillingPeriod { get; set; } = "monthly";

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Icon { get; set; }

    public bool IsPopular { get; set; } = false;

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; } = false;
    public List<string> Features { get; set; } = new();
}

public class UpdatePricingTierDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 99999.99)]
    public decimal Price { get; set; }

    [StringLength(50)]
    public string BillingPeriod { get; set; } = "monthly";

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Icon { get; set; }

    public bool IsPopular { get; set; }

    [Range(0, 10000)]
    public int DisplayOrder { get; set; }

    public bool IsPublished { get; set; }
    public List<string> Features { get; set; } = new();
}

public class PricingFeatureDto
{
    public int Id { get; set; }
    public int PricingTierId { get; set; }
    public string Feature { get; set; } = string.Empty;
    public bool IsIncluded { get; set; }
    public int DisplayOrder { get; set; }
}
