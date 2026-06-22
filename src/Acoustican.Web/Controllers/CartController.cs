using Acoustican.DTOs;
using Acoustican.Extensions;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    private int? CurrentUserId => User.GetUserId();

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var items = await cartService.GetCartItemsAsync(userId);
        return Ok(new { success = true, items });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCartCount()
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var count = await cartService.GetCartCountAsync(userId);
        return Ok(new { success = true, count });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (success, message) = await cartService.AddToCartAsync(userId, request.CourseId);
        if (!success)
            return BadRequest(new { success, message });

        var count = await cartService.GetCartCountAsync(userId);
        return Ok(new { success, message, cartCount = count });
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (success, message) = await cartService.RemoveFromCartAsync(userId, courseId);
        if (!success)
            return BadRequest(new { success, message });

        var count = await cartService.GetCartCountAsync(userId);
        return Ok(new { success, message, cartCount = count });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncCart([FromBody] SyncCartRequest request)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (added, skipped) = await cartService.SyncCartAsync(userId, request.CourseIds);
        var count = await cartService.GetCartCountAsync(userId);
        return Ok(new
        {
            success = true,
            message = $"{added} item(s) synced to your cart",
            added,
            skipped,
            cartCount = count
        });
    }
}
