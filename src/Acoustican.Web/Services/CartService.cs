using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface ICartService
{
    Task<List<CartItemDto>> GetCartItemsAsync(int userId);
    Task<(bool Success, string Message)> AddToCartAsync(int userId, int courseId);
    Task<(bool Success, string Message)> RemoveFromCartAsync(int userId, int courseId);
    Task ClearCartAsync(int userId);
    Task<int> GetCartCountAsync(int userId);
    Task<(int Added, int Skipped)> SyncCartAsync(int userId, List<int> courseIds);
}

public class CartService(ApplicationDbContext context, ILogger<CartService> logger) : ICartService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<CartService> _logger = logger;

    public async Task<List<CartItemDto>> GetCartItemsAsync(int userId)
    {
        return await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.Course)
            .OrderByDescending(ci => ci.AddedAt)
            .Select(ci => new CartItemDto
            {
                Id = ci.Id,
                CourseId = ci.CourseId,
                CourseTitle = ci.Course.Title,
                CourseThumbnailUrl = ci.Course.ThumbnailUrl,
                CoursePrice = ci.Course.Price,
                CourseOriginalPrice = ci.Course.OriginalPrice,
                CourseLevel = ci.Course.Level,
                InstructorName = ci.Course.InstructorName,
                AddedAt = ci.AddedAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> AddToCartAsync(int userId, int courseId)
    {
        // Check if course exists and is published
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.IsPublished);
        if (course == null)
            return (false, "Course not found or not available");

        // Check if user already has an active subscription
        var hasActiveSub = await _context.UserSubscriptions.AnyAsync(s => s.UserId == userId && s.Status == "active");
        if (hasActiveSub)
            return (false, "You already have access to all courses via your active subscription.");

        // Check if user already purchased the course
        var isPurchased = await _context.Orders.AnyAsync(o => o.UserId == userId && o.Status == "Confirmed" && o.Items.Any(oi => oi.CourseId == courseId));
        if (isPurchased)
            return (false, "You have already purchased this course.");

        // Check for duplicate
        var exists = await _context.CartItems
            .AnyAsync(ci => ci.UserId == userId && ci.CourseId == courseId);
        if (exists)
            return (false, "Course is already in your cart");

        var cartItem = new CartItem
        {
            UserId = userId,
            CourseId = courseId,
            AddedAt = DateTime.UtcNow
        };

        _context.CartItems.Add(cartItem);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} added course {CourseId} to cart", userId, courseId);
        return (true, "Added to cart");
    }

    public async Task<(bool Success, string Message)> RemoveFromCartAsync(int userId, int courseId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.CourseId == courseId);

        if (cartItem == null)
            return (false, "Item not found in cart");

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} removed course {CourseId} from cart", userId, courseId);
        return (true, "Removed from cart");
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        if (items.Count > 0)
        {
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleared {Count} items from cart for user {UserId}", items.Count, userId);
        }
    }

    public async Task<int> GetCartCountAsync(int userId)
    {
        return await _context.CartItems
            .CountAsync(ci => ci.UserId == userId);
    }

    public async Task<(int Added, int Skipped)> SyncCartAsync(int userId, List<int> courseIds)
    {
        if (courseIds.Count == 0)
            return (0, 0);

        // If they have an active subscription, don't sync any items
        var hasActiveSub = await _context.UserSubscriptions.AnyAsync(s => s.UserId == userId && s.Status == "active");
        if (hasActiveSub)
            return (0, courseIds.Count);

        // Get already purchased course IDs
        var purchasedCourseIds = await _context.Orders
            .Where(o => o.UserId == userId && o.Status == "Confirmed")
            .SelectMany(o => o.Items)
            .Select(oi => oi.CourseId)
            .ToListAsync();

        // Get published course IDs that actually exist and are not already purchased
        var validCourseIds = await _context.Courses
            .Where(c => courseIds.Contains(c.Id) && c.IsPublished && !purchasedCourseIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

        // Get existing cart course IDs
        var existingCourseIds = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.CourseId)
            .ToListAsync();

        var newCourseIds = validCourseIds.Except(existingCourseIds).ToList();

        if (newCourseIds.Count > 0)
        {
            var newItems = newCourseIds.Select(courseId => new CartItem
            {
                UserId = userId,
                CourseId = courseId,
                AddedAt = DateTime.UtcNow
            });

            _context.CartItems.AddRange(newItems);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Synced cart for user {UserId}: {Added} added, {Skipped} skipped",
            userId, newCourseIds.Count, courseIds.Count - newCourseIds.Count);

        return (newCourseIds.Count, courseIds.Count - newCourseIds.Count);
    }
}
