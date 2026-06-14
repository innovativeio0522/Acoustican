using System.Security.Claims;
using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController(ICartService cartService) : ControllerBase
{
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var items = await cartService.GetCartItemsAsync(GetUserId());
        return Ok(new { success = true, items });
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetCartCount()
    {
        var count = await cartService.GetCartCountAsync(GetUserId());
        return Ok(new { success = true, count });
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var (success, message) = await cartService.AddToCartAsync(GetUserId(), request.CourseId);
        if (!success)
            return BadRequest(new { success, message });

        var count = await cartService.GetCartCountAsync(GetUserId());
        return Ok(new { success, message, cartCount = count });
    }

    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveFromCart(int courseId)
    {
        var (success, message) = await cartService.RemoveFromCartAsync(GetUserId(), courseId);
        if (!success)
            return BadRequest(new { success, message });

        var count = await cartService.GetCartCountAsync(GetUserId());
        return Ok(new { success, message, cartCount = count });
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncCart([FromBody] SyncCartRequest request)
    {
        var (added, skipped) = await cartService.SyncCartAsync(GetUserId(), request.CourseIds);
        var count = await cartService.GetCartCountAsync(GetUserId());
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
