using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface IOrderService
{
    Task<(bool Success, string Message, OrderDto? Order)> CreateOrderFromCartAsync(int userId);
    Task<List<OrderDto>> GetOrdersAsync(int userId);
    Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
    Task<(bool Success, string Message)> CancelOrderAsync(int userId, int orderId);
}

public class OrderService(ApplicationDbContext context, ICartService cartService, ILogger<OrderService> logger) : IOrderService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICartService _cartService = cartService;
    private readonly ILogger<OrderService> _logger = logger;

    public async Task<(bool Success, string Message, OrderDto? Order)> CreateOrderFromCartAsync(int userId)
    {
        // Get cart items with course data
        var cartItems = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Course)
            .ToListAsync();

        if (cartItems.Count == 0)
            return (false, "Your cart is empty", null);

        // Create order with snapshot of current prices
        var now = DateTime.UtcNow;
        var order = new Order
        {
            UserId = userId,
            TotalAmount = cartItems.Sum(ci => ci.Course.Price),
            Status = "Pending",
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // Save to get order ID

        // Create order items with price/title snapshots
        var orderItems = cartItems.Select(ci => new OrderItem
        {
            OrderId = order.Id,
            CourseId = ci.CourseId,
            CourseTitle = ci.Course.Title,
            Price = ci.Course.Price
        }).ToList();

        _context.OrderItems.AddRange(orderItems);

        // Clear the cart
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Order {OrderId} created for user {UserId} with {ItemCount} items, total ₹{Total}",
            order.Id, userId, orderItems.Count, order.TotalAmount);

        // Build response DTO
        var orderDto = new OrderDto
        {
            Id = order.Id,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            Items = orderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                CourseId = oi.CourseId,
                CourseTitle = oi.CourseTitle,
                Price = oi.Price,
                CourseThumbnailUrl = cartItems
                    .FirstOrDefault(ci => ci.CourseId == oi.CourseId)?.Course.ThumbnailUrl
            }).ToList()
        };

        return (true, "Order placed successfully", orderDto);
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Course)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentId = o.PaymentId,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    CourseId = oi.CourseId,
                    CourseTitle = oi.CourseTitle,
                    Price = oi.Price,
                    CourseThumbnailUrl = oi.Course.ThumbnailUrl
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        return await _context.Orders
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Course)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                PaymentId = o.PaymentId,
                CreatedAt = o.CreatedAt,
                Items = o.Items.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    CourseId = oi.CourseId,
                    CourseTitle = oi.CourseTitle,
                    Price = oi.Price,
                    CourseThumbnailUrl = oi.Course.ThumbnailUrl
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string Message)> CancelOrderAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        if (order == null)
            return (false, "Order not found");

        if (order.Status != "Pending")
            return (false, $"Cannot cancel order with status '{order.Status}'");

        order.Status = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} cancelled by user {UserId}", orderId, userId);
        return (true, "Order cancelled successfully");
    }
}
