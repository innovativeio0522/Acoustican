namespace Acoustican.DTOs;

public class SubscribeRequest
{
    public int PricingTierId { get; set; }
}

public class SubscriptionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PricingTierId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public decimal PlanPrice { get; set; }
    public string BillingPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayKey { get; set; }
    public bool RequiresPayment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class VerifySubscriptionPaymentDto
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}
