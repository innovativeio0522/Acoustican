using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideosController(IConfiguration configuration, ILogger<VideosController> logger) : ControllerBase
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

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://dev.vdocipher.com/api/videos/{videoId}/otp");
            request.Headers.Add("Authorization", $"Apisecret {apiKey}");

            object payload;
            if (User.Identity?.IsAuthenticated == true)
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? "student@guitarverse.com";
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
