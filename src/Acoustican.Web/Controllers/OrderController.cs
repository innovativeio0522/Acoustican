using System.Security.Claims;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrderController(IOrderService orderService) : ControllerBase
{
    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var (success, message, order) = await orderService.CreateOrderFromCartAsync(GetUserId());
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message, order });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await orderService.GetOrdersAsync(GetUserId());
        return Ok(new { success = true, orders });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await orderService.GetOrderByIdAsync(GetUserId(), id);
        if (order == null)
            return NotFound(new { success = false, message = "Order not found" });

        return Ok(new { success = true, order });
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        var (success, message) = await orderService.CancelOrderAsync(GetUserId(), id);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message });
    }
}
