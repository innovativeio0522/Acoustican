using Acoustican.DTOs;

namespace Acoustican.Services;

public interface ISubscriptionService
{
    Task<SubscriptionDto?> SubscribeAsync(int userId, int pricingTierId);
    Task<SubscriptionDto?> GetUserSubscriptionAsync(int userId);
    Task<bool> CancelSubscriptionAsync(int userId);
    Task<(bool Success, string Message)> VerifySubscriptionPaymentAsync(int userId, VerifySubscriptionPaymentDto dto);
}
