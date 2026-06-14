using System.ComponentModel.DataAnnotations;

namespace Acoustican.DTOs;

// Auth DTOs
public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public AdminUserDto? User { get; set; }
}

public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; }
    public DateTime LastLoginAt { get; set; }
}

public class CreateAdminUserDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}

public class FileUploadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
}
