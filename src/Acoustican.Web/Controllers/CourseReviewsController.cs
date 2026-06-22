using System.Security.Claims;
using System.Threading.Tasks;
using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/courses/{courseId}/reviews")]
public class CourseReviewsController(ICourseReviewService reviewService) : ControllerBase
{
    private readonly ICourseReviewService _reviewService = reviewService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(int courseId)
    {
        var reviews = await _reviewService.GetReviewsForCourseAsync(courseId);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitReview(int courseId, [FromBody] CreateReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "Invalid user session." });
        }

        var result = await _reviewService.SubmitReviewAsync(userId, courseId, dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result.Review);
    }

    [HttpGet("can-review")]
    [Authorize]
    public async Task<IActionResult> CanReview(int courseId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new { message = "Invalid user session." });
        }

        var canReview = await _reviewService.CanUserReviewCourseAsync(userId, courseId);
        return Ok(new { canReview });
    }
}
