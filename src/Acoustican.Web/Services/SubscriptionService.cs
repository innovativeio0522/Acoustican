using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public class SubscriptionService(ApplicationDbContext context) : ISubscriptionService
{
    public async Task<SubscriptionDto?> SubscribeAsync(int userId, int pricingTierId)
    {
        var tier = await context.PricingTiers
            .FirstOrDefaultAsync(p => p.Id == pricingTierId && p.IsPublished);

        if (tier == null) return null;

        // Cancel any existing active subscription first
        var existing = await context.UserSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "active");

        if (existing != null)
        {
            // Same plan — no-op, just return current
            if (existing.PricingTierId == pricingTierId)
                return MapToDto(existing);

            existing.Status = "cancelled";
            existing.EndDate = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        var subscription = new UserSubscription
        {
            UserId = userId,
            PricingTierId = pricingTierId,
            Status = "active",
            StartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.UserSubscriptions.Add(subscription);
        await context.SaveChangesAsync();

        subscription.PricingTier = tier;
        return MapToDto(subscription);
    }

    public async Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId)
    {
        var subscription = await context.UserSubscriptions
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
        StartDate = s.StartDate,
        EndDate = s.EndDate
    };
}
