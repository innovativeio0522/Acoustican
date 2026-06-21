using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Acoustican.Data;
using Acoustican.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Acoustican.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, AdminUser? User)> AuthenticateAsync(string email, string password);
    Task<(bool Success, string Message, AdminUser? User)> RegisterAsync(string email, string password, string fullName);
    string GenerateJwtToken(AdminUser user);
    Task<(bool Success, string Message, string? Token)> RequestPasswordResetAsync(string email);
    Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
}

public class AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger, IEmailService emailService) : IAuthService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailService _emailService = emailService;

    public async Task<(bool Success, string Message, AdminUser? User)> AuthenticateAsync(string email, string password)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return (false, "Invalid email or password", null);

        if (!user.IsActive)
            return (false, "User account is inactive", null);

        user.LastLoginAt = DateTime.UtcNow;
        _context.AdminUsers.Update(user);
        await _context.SaveChangesAsync();

        return (true, "Authentication successful", user);
    }

    public async Task<(bool Success, string Message, AdminUser? User)> RegisterAsync(string email, string password, string fullName)
    {
        if (await _context.AdminUsers.AnyAsync(u => u.Email == email))
            return (false, "Email already registered", null);

        var user = new AdminUser
        {
            Email = email,
            FullName = fullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsActive = true,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        try
        {
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email} during registration.", user.Email);
        }

        return (true, "User registered successfully", user);
    }

    public string GenerateJwtToken(AdminUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("IsActive", user.IsActive.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<(bool Success, string Message, string? Token)> RequestPasswordResetAsync(string email)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return (false, "If an account exists with this email, you will receive a password reset link", null);

        // Generate a unique password reset token
        var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24); // Token valid for 24 hours

        _context.AdminUsers.Update(user);
        await _context.SaveChangesAsync();

        // Send reset email
        var (emailSuccess, emailMessage) = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FullName);
        
        if (emailSuccess)
        {
            _logger.LogInformation("Password reset email sent to {Email}", email);
            return (true, "If an account exists with this email, you will receive a password reset link", null);
        }
        else
        {
            _logger.LogError("Failed to send password reset email to {Email}", email);
            return (true, "If an account exists with this email, you will receive a password reset link", null);
        }
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
        if (user == null)
            return (false, "Invalid reset token");

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return (false, "Reset token has expired");

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        _context.AdminUsers.Update(user);
        await _context.SaveChangesAsync();

        return (true, "Password reset successfully");
    }
}
