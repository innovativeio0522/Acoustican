using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Acoustican.Controllers;

[Route("api/subscriptions")]
[ApiController]
public class SubscriptionController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { success = false, message = "Invalid token" });

        var subscription = await subscriptionService.SubscribeAsync(userId, request.PricingTierId);
        if (subscription == null)
            return BadRequest(new { success = false, message = "Plan not found or not available" });

        return Ok(new { success = true, subscription });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMySubscription()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var subscription = await subscriptionService.GetUserSubscriptionAsync(userId);
        return Ok(new { success = true, subscription });
    }

    [HttpDelete("me")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var cancelled = await subscriptionService.CancelSubscriptionAsync(userId);
        return Ok(new { success = cancelled });
    }
}
