using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Acoustican.Data;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController(ApplicationDbContext context, IConfiguration configuration, ILogger<VideosController> logger, IHttpClientFactory httpClientFactory) : ControllerBase
{

    [HttpPost("otp/{videoId}")]
    [AllowAnonymous] // Allow guests for previews, but overlay credentials if logged in
    public async Task<IActionResult> GetVideoOtp(string videoId)
    {
        try
        {
            var apiKey = configuration["VdoCipher:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_VDOCIPHER_API_SECRET_KEY")
            {
                return BadRequest(new { message = "Video provider API key is not configured." });
            }

            // ─── Secure Server-Side Authorization Checks ─────────────────────
            bool isAllowed = false;

            // 1. Allow if it's the Hero preview video
            var isHeroPreview = await context.HeroContents.AnyAsync(h => h.PreviewVideoId == videoId && h.PreviewVideoId != null && h.PreviewVideoId != "");
            if (isHeroPreview)
            {
                isAllowed = true;
            }
            else
            {
                // 2. Allow if it's a teaser/preview video configured for any lesson
                var isLessonPreview = await context.Lessons.AnyAsync(l => l.PreviewVideoId == videoId && l.PreviewVideoId != null && l.PreviewVideoId != "");
                if (isLessonPreview)
                {
                    isAllowed = true;
                }
            }

            if (!isAllowed)
            {
                // 3. Check if it matches a full lesson video URL
                var lesson = await context.Lessons
                    .Include(l => l.Module)
                    .FirstOrDefaultAsync(l => l.VideoUrl == videoId && l.VideoUrl != null && l.VideoUrl != "");

                if (lesson == null)
                {
                    // Video is not registered in our system
                    return NotFound(new { message = "Requested video is not found or not configured." });
                }

                // User must be authenticated to watch full lessons
                if (User.Identity?.IsAuthenticated != true)
                {
                    return Unauthorized(new { message = "You must be logged in to view this lesson." });
                }

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user session." });
                }

                // Admins/ContentManagers have full access
                var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("ContentManager");
                if (!isAdminOrManager)
                {
                    // Check active subscription
                    var hasActiveSub = await context.UserSubscriptions
                        .AnyAsync(s => s.UserId == userId && s.Status == "active");

                    if (!hasActiveSub)
                    {
                        // Check if they purchased the specific course
                        var courseId = lesson.Module!.CourseId;
                        var isPurchased = await context.Orders
                            .AnyAsync(o => o.UserId == userId && o.Status == "Confirmed" && o.Items.Any(oi => oi.CourseId == courseId));

                        if (!isPurchased)
                        {
                            return StatusCode(403, new { message = "You must enroll in this course to watch this lesson." });
                        }
                    }
                }
            }


            using var httpClient = httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://dev.vdocipher.com/api/videos/{videoId}/otp");
            request.Headers.Add("Authorization", $"Apisecret {apiKey}");

            object payload;
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? "student@theacoustican.com";
                var name = User.Identity.Name ?? "Student";

                // Dynamic Moving Watermark config: showing Name (Email) floating on screen
                var watermarkConfig = new[]
                {
                    new
                    {
                        type = "rtext",
                        text = $"{name} ({email})",
                        alpha = "0.25", // 25% opacity
                        color = "0xFFFFFF", // white
                        size = "13",
                        interval = "5000" // change position every 5s
                    }
                };

                payload = new
                {
                    ttl = 300, // token valid for 5 mins
                    annotate = JsonSerializer.Serialize(watermarkConfig)
                };
            }
            else
            {
                payload = new
                {
                    ttl = 300 // token valid for 5 mins
                };
            }

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                logger.LogError("VdoCipher OTP request failed: {Error}", errorContent);
                return StatusCode((int)response.StatusCode, new { message = "Failed to request secure video token from provider." });
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonString);

            var otp = result.GetProperty("otp").GetString();
            var playbackInfo = result.GetProperty("playbackInfo").GetString();

            return Ok(new
            {
                otp,
                playbackInfo
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating VdoCipher OTP");
            return StatusCode(500, new { message = "An internal error occurred." });
        }
    }
}
