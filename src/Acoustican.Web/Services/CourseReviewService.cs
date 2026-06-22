using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface ICourseReviewService
{
    Task<List<CourseReviewDto>> GetReviewsForCourseAsync(int courseId);
    Task<(bool Success, string Message, CourseReviewDto? Review)> SubmitReviewAsync(int userId, int courseId, CreateReviewDto dto);
    Task<bool> CanUserReviewCourseAsync(int userId, int courseId);
}

public class CourseReviewService(ApplicationDbContext context, IMapper mapper) : ICourseReviewService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<List<CourseReviewDto>> GetReviewsForCourseAsync(int courseId)
    {
        var reviews = await _context.CourseReviews
            .AsNoTracking()
            .Include(r => r.User)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<CourseReviewDto>>(reviews);
    }

    public async Task<(bool Success, string Message, CourseReviewDto? Review)> SubmitReviewAsync(int userId, int courseId, CreateReviewDto dto)
    {
        // 1. Verify the course exists
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
        {
            return (false, "Course not found", null);
        }

        // 2. Verify eligibility
        var isEligible = await CanUserReviewCourseAsync(userId, courseId);
        if (!isEligible)
        {
            return (false, "You must enroll in this course or have an active subscription to leave a review.", null);
        }

        // 3. Find if user already reviewed the course
        var review = await _context.CourseReviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId);

        if (review == null)
        {
            // Insert new review
            review = new CourseReview
            {
                CourseId = courseId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.CourseReviews.Add(review);
        }
        else
        {
            // Update existing review
            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;
            _context.CourseReviews.Update(review);
        }

        await _context.SaveChangesAsync();

        // 4. Recalculate average rating & review count for the Course
        await RecalculateCourseStatsAsync(courseId);

        // Fetch User details for the returned DTO mapping
        if (review.User == null)
        {
            await _context.Entry(review).Reference(r => r.User).LoadAsync();
        }

        return (true, "Review submitted successfully", _mapper.Map<CourseReviewDto>(review));
    }

    public async Task<bool> CanUserReviewCourseAsync(int userId, int courseId)
    {
        // Check if user is Admin or ContentManager (full access)
        var user = await _context.AdminUsers.FindAsync(userId);
        if (user == null) return false;

        if (user.Role == "Admin" || user.Role == "ContentManager")
        {
            return true;
        }

        // Check if user has an active subscription
        var hasActiveSub = await _context.UserSubscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == "active");

        if (hasActiveSub) return true;

        // Check if they purchased the specific course
        var isPurchased = await _context.Orders
            .AnyAsync(o => o.UserId == userId && o.Status == "Confirmed" && o.Items.Any(oi => oi.CourseId == courseId));

        return isPurchased;
    }

    private async Task RecalculateCourseStatsAsync(int courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return;

        var stats = await _context.CourseReviews
            .Where(r => r.CourseId == courseId)
            .GroupBy(r => r.CourseId)
            .Select(g => new
            {
                AverageRating = (decimal?)g.Average(r => r.Rating) ?? 0,
                Count = g.Count()
            })
            .FirstOrDefaultAsync();

        course.Rating = stats?.AverageRating ?? 0;
        course.ReviewCount = stats?.Count ?? 0;

        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }
}
