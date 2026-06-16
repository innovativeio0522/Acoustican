using System.Security.Claims;
using System.Text.Json;
using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/auth/google")]
public class GoogleAuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public GoogleAuthController(ApplicationDbContext db, IAuthService authService, IConfiguration configuration)
    {
        _db = db;
        _authService = authService;
        _configuration = configuration;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        // Initiates Google OAuth challenge.
        // IMPORTANT: build callback URL deterministically from config to avoid redirect_uri_mismatch.
        var baseUrl = _configuration["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
        baseUrl = baseUrl.TrimEnd('/');
        // RedirectUri must point to a DIFFERENT path than CallbackPath.
        // The middleware owns /callback; it signs the user into Cookies and then
        // redirects here to /token, where we read the cookie and issue a JWT.
        var redirectUrl = $"{baseUrl}/api/auth/google/token";

        return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, GoogleDefaults.AuthenticationScheme);
    }

    // This endpoint is reached after the Google middleware has handled /callback,
    // validated the OAuth state, signed the user into the Cookies scheme, and
    // redirected here. We just read the cookie principal and issue a JWT.
    [HttpGet("token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token()
    {
        // Read the Google principal.
        var result = await HttpContext.AuthenticateAsync("Cookies");
        if (result?.Principal == null)
        {
            return BadRequest(new { success = false, message = "Google authentication failed" });
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? result.Principal.FindFirstValue("name");

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { success = false, message = "Google account email not available" });

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new Acoustican.Models.AdminUser
            {
                Email = email,
                FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                IsActive = true,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _db.AdminUsers.Add(user);
        }
        else
        {
            user.FullName = string.IsNullOrWhiteSpace(fullName) ? user.FullName : fullName;
            user.IsActive = true;
            user.Role = "User"; // normal users only
            user.LastLoginAt = DateTime.UtcNow;

            _db.AdminUsers.Update(user);
        }

        await _db.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);

        var userDto = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        };

        // Redirect to the landing page, passing the JWT and user data as query params.
        // The frontend script picks these up on DOMContentLoaded, saves them to
        // localStorage, and then cleans the URL via history.replaceState.
        var baseUrl = _configuration["App:BaseUrl"]?.TrimEnd('/') ?? "";
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var userJson = Uri.EscapeDataString(JsonSerializer.Serialize(userDto, options));
        var encodedToken = Uri.EscapeDataString(token);

        return Redirect($"{baseUrl}/?token={encodedToken}&user={userJson}");
    }
}

