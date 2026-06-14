using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, message, user) = await authService.AuthenticateAsync(request.Email, request.Password);
        if (!success || user == null)
            return Unauthorized(new LoginResponse { Success = false, Message = message });

        var token = authService.GenerateJwtToken(user);
        var userDto = new AdminUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        };

        return Ok(new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = userDto
        });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] CreateAdminUserDto request)
    {
        var (success, message, user) = await authService.RegisterAsync(request.Email, request.Password, request.FullName);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message, userId = user?.Id });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var (success, message, token) = await authService.RequestPasswordResetAsync(request.Email);
        if (!success)
            return BadRequest(new { success, message });

        // Token is sent via email only — never return it in the response
        return Ok(new { success, message });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var (success, message) = await authService.ResetPasswordAsync(request.Token, request.NewPassword);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message });
    }
}
