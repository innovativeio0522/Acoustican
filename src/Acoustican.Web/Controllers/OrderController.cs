using Acoustican.DTOs;
using Acoustican.Extensions;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController(IOrderService orderService) : ControllerBase
{
    private int? CurrentUserId => User.GetUserId();

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (success, message, order) = await orderService.CreateOrderFromCartAsync(userId);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message, order });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyPaymentDto dto)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (success, message) = await orderService.VerifyOrderPaymentAsync(userId, dto);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var orders = await orderService.GetOrdersAsync(userId);
        return Ok(new { success = true, orders });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var order = await orderService.GetOrderByIdAsync(userId, id);
        if (order == null)
            return NotFound(new { success = false, message = "Order not found" });

        return Ok(new { success = true, order });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        if (CurrentUserId is not { } userId)
            return Unauthorized(new { success = false, message = "Invalid or expired token" });

        var (success, message) = await orderService.CancelOrderAsync(userId, id);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message });
    }
}
