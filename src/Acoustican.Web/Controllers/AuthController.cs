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

    [HttpPost("register-user")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterUser([FromBody] CreateAdminUserDto request)
    {
        // Force role to "User" for public registrations
        var (success, message, user) = await authService.RegisterAsync(request.Email, request.Password, request.FullName);
        if (!success)
            return BadRequest(new { success, message });

        return Ok(new { success, message, userId = user?.Id });
    }


    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.RequestPasswordResetAsync(request.Email);

        // Always return 200 OK to prevent user enumeration
        return Ok(new { success = true, message = "If an account exists with this email, you will receive a password reset link" });
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
