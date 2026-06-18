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
    Task<(bool Success, string Message)> VerifyOrderPaymentAsync(int userId, VerifyPaymentDto dto);
}

public class OrderService(
    ApplicationDbContext context,
    ICartService cartService,
    ILogger<OrderService> logger,
    Microsoft.Extensions.Configuration.IConfiguration configuration) : IOrderService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICartService _cartService = cartService;
    private readonly ILogger<OrderService> _logger = logger;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration = configuration;

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

        // Generate Razorpay Order
        var keyId = _configuration["Razorpay:KeyId"];
        var keySecret = _configuration["Razorpay:KeySecret"];
        string? razorpayOrderId = null;

        if (!string.IsNullOrWhiteSpace(keyId) && !keyId.Contains("placeholder") &&
            !string.IsNullOrWhiteSpace(keySecret) && !keySecret.Contains("placeholder"))
        {
            try
            {
                var client = new Razorpay.Api.RazorpayClient(keyId, keySecret);
                var options = new Dictionary<string, object>
                {
                    { "amount", (int)(order.TotalAmount * 100) }, // amount in paise
                    { "currency", "INR" },
                    { "receipt", $"gva_rcpt_{order.Id}" }
                };
                var razorpayOrder = client.Order.Create(options);
                razorpayOrderId = razorpayOrder["id"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Razorpay order for local order {OrderId}", order.Id);
            }
        }

        if (string.IsNullOrEmpty(razorpayOrderId))
        {
            razorpayOrderId = "order_mock_" + Guid.NewGuid().ToString("N")[..14];
            _logger.LogWarning("Using mock Razorpay Order ID '{OrderId}' because Razorpay credentials are not configured.", razorpayOrderId);
        }

        order.RazorpayOrderId = razorpayOrderId;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

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
            RazorpayOrderId = order.RazorpayOrderId,
            RazorpayKey = keyId,
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
                RazorpayOrderId = o.RazorpayOrderId,
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
                RazorpayOrderId = o.RazorpayOrderId,
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

    public async Task<(bool Success, string Message)> VerifyOrderPaymentAsync(int userId, VerifyPaymentDto dto)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.RazorpayOrderId == dto.RazorpayOrderId && o.UserId == userId);

        if (order == null)
            return (false, "Order not found");

        if (order.Status != "Pending")
            return (false, $"Order is already in '{order.Status}' state");

        var keySecret = _configuration["Razorpay:KeySecret"];
        bool isValid = false;

        if (dto.RazorpayOrderId.StartsWith("order_mock_"))
        {
            isValid = true;
            _logger.LogInformation("Verifying mock order {OrderId} as successful.", dto.RazorpayOrderId);
        }
        else if (!string.IsNullOrWhiteSpace(keySecret))
        {
            try
            {
                var payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
                var computedSignature = HmacSha256(payload, keySecret);
                isValid = computedSignature.Equals(dto.RazorpaySignature, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Razorpay signature for order {OrderId}", order.Id);
            }
        }

        if (!isValid)
        {
            return (false, "Signature verification failed");
        }

        order.Status = "Confirmed";
        order.PaymentId = dto.RazorpayPaymentId;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Order {OrderId} successfully paid and verified with PaymentId {PaymentId}", order.Id, dto.RazorpayPaymentId);
        return (true, "Payment verified successfully");
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
