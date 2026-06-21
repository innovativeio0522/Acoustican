using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Acoustican.Services;

public class SubscriptionService(
    ApplicationDbContext context,
    Microsoft.Extensions.Configuration.IConfiguration configuration,
    Microsoft.AspNetCore.Hosting.IWebHostEnvironment environment,
    ILogger<SubscriptionService> logger) : ISubscriptionService
{
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration = configuration;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _environment = environment;
    private readonly ILogger<SubscriptionService> _logger = logger;

    public async Task<SubscriptionDto?> SubscribeAsync(int userId, int pricingTierId)
    {
        var tier = await context.PricingTiers
            .FirstOrDefaultAsync(p => p.Id == pricingTierId && p.IsPublished);

        if (tier == null) return null;

        // Cancel/update any existing active/pending subscription first
        var existing = await context.UserSubscriptions
            .Where(s => s.UserId == userId && (s.Status == "active" || s.Status == "pending"))
            .ToListAsync();

        foreach (var sub in existing)
        {
            // If already subscribed to the same plan and it is active, return it as-is
            if (sub.PricingTierId == pricingTierId && sub.Status == "active")
            {
                sub.PricingTier = tier;
                return MapToDto(sub);
            }

            sub.Status = "cancelled";
            sub.EndDate = DateTime.UtcNow;
            sub.UpdatedAt = DateTime.UtcNow;
        }

        string status = "active";
        string? razorpayOrderId = null;
        bool requiresPayment = tier.Price > 0;
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];

        if (requiresPayment)
        {
            status = "pending";

            if (!string.IsNullOrWhiteSpace(keyId) && !keyId.Contains("placeholder") &&
                !string.IsNullOrWhiteSpace(keySecret) && !keySecret.Contains("placeholder"))
            {
                try
                {
                    var client = new Razorpay.Api.RazorpayClient(keyId, keySecret);
                    var options = new Dictionary<string, object>
                    {
                        { "amount", (int)(tier.Price * 100) }, // in paise
                        { "currency", "INR" },
                        { "receipt", $"gva_sub_rcpt_{userId}_{pricingTierId}" }
                    };
                    var razorpayOrder = client.Order.Create(options);
                    razorpayOrderId = razorpayOrder["id"]?.ToString();
                }
                catch
                {
                    // Fall back to mock below
                }
            }

            if (string.IsNullOrEmpty(razorpayOrderId))
            {
                razorpayOrderId = "order_mock_" + Guid.NewGuid().ToString("N")[..14];
            }
        }

        var subscription = new UserSubscription
        {
            UserId = userId,
            PricingTierId = pricingTierId,
            Status = status,
            RazorpayOrderId = razorpayOrderId,
            StartDate = DateTime.UtcNow,
            EndDate = !requiresPayment
                ? (tier.BillingPeriod.Equals("yearly", StringComparison.OrdinalIgnoreCase) || tier.BillingPeriod.Equals("annually", StringComparison.OrdinalIgnoreCase)
                    ? DateTime.UtcNow.AddYears(1)
                    : DateTime.UtcNow.AddMonths(1))
                : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.UserSubscriptions.Add(subscription);
        await context.SaveChangesAsync();

        subscription.PricingTier = tier;
        var dto = MapToDto(subscription);
        dto.RazorpayKey = keyId;
        return dto;
    }

    public async Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId)
    {
        var subscription = await context.UserSubscriptions
            .AsNoTracking()
            .Include(s => s.PricingTier)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");

        return subscription == null ? null : MapToDto(subscription);
    }

    public async Task<bool> CancelSubscriptionAsync(int userId)
    {
        var subscription = await context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");

        if (subscription == null) return false;

        subscription.Status = "cancelled";
        subscription.EndDate = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    private static SubscriptionDto MapToDto(UserSubscription s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        PricingTierId = s.PricingTierId,
        PlanName = s.PricingTier?.Name ?? string.Empty,
        PlanPrice = s.PricingTier?.Price ?? 0,
        BillingPeriod = s.PricingTier?.BillingPeriod ?? string.Empty,
        Status = s.Status,
        RazorpayOrderId = s.RazorpayOrderId,
        RequiresPayment = s.PricingTier != null && s.PricingTier.Price > 0,
        StartDate = s.StartDate,
        EndDate = s.EndDate
    };

    public async Task<(bool Success, string Message)> VerifySubscriptionPaymentAsync(int userId, VerifySubscriptionPaymentDto dto)
    {
        var subscription = await context.UserSubscriptions
            .Include(s => s.PricingTier)
            .FirstOrDefaultAsync(s => s.RazorpayOrderId == dto.RazorpayOrderId && s.UserId == userId);

        if (subscription == null)
            return (false, "Subscription not found");

        if (subscription.Status != "pending")
            return (false, $"Subscription is already in '{subscription.Status}' state");

        var keySecret = _configuration["Razorpay:KeySecret"];
        bool isValid = false;

        if (dto.RazorpayOrderId.StartsWith("order_mock_"))
        {
            if (_environment.IsDevelopment())
            {
                isValid = true;
                _logger.LogInformation("Verifying mock subscription order {OrderId} as successful in Development environment.", dto.RazorpayOrderId);
            }
            else
            {
                _logger.LogWarning("Attempted to bypass subscription payment verification using mock order {OrderId} in production!", dto.RazorpayOrderId);
            }
        }
        else if (!string.IsNullOrWhiteSpace(keySecret))
        {
            try
            {
                var payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
                var computedSignature = HmacSha256(payload, keySecret);
                isValid = computedSignature.Equals(dto.RazorpaySignature, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // Verification exception
            }
        }

        if (!isValid)
        {
            return (false, "Signature verification failed");
        }

        subscription.Status = "active";
        subscription.PaymentId = dto.RazorpayPaymentId;
        subscription.StartDate = DateTime.UtcNow;
        subscription.EndDate = subscription.PricingTier != null && (subscription.PricingTier.BillingPeriod.Equals("yearly", StringComparison.OrdinalIgnoreCase) || subscription.PricingTier.BillingPeriod.Equals("annually", StringComparison.OrdinalIgnoreCase))
            ? subscription.StartDate.AddYears(1)
            : subscription.StartDate.AddMonths(1);
        subscription.UpdatedAt = DateTime.UtcNow;

        context.UserSubscriptions.Update(subscription);
        await context.SaveChangesAsync();

        return (true, "Subscription payment verified successfully");
    }

    private static string HmacSha256(string payload, string secret)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(secret);
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
