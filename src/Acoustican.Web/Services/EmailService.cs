using System.Net;
using System.Net.Mail;

namespace Acoustican.Services;

public interface IEmailService
{
    Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email, string resetToken, string userName);
    Task<(bool Success, string Message)> SendWelcomeEmailAsync(string email, string userName);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> SendPasswordResetEmailAsync(string email, string resetToken, string userName)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings:Smtp");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var sender = smtpSettings["SenderEmail"];
            var password = smtpSettings["SenderPassword"];

            // Validate SMTP settings (avoid logging secrets)
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning(
                    "SMTP settings not configured. hostConfigured={HostConfigured}, senderConfigured={SenderConfigured}. Skipping email send.",
                    !string.IsNullOrEmpty(host),
                    !string.IsNullOrEmpty(sender)
                );
                return (true, "If an account exists with this email, you will receive a password reset link");
            }

            // Create reset link
            var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:5000";
            var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(resetToken)}";

            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.Credentials = new NetworkCredential(sender, password);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(sender, "Acoustican Support"),
                    Subject = "Reset Your Acoustican Password",
                    Body = GeneratePasswordResetEmailBody(userName, resetLink),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Password reset email sent to {email}");
                return (true, "Password reset email sent successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending password reset email to {email}");
            return (true, "If an account exists with this email, you will receive a password reset link");
        }
    }

    public async Task<(bool Success, string Message)> SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("EmailSettings:Smtp");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var sender = smtpSettings["SenderEmail"];
            var password = smtpSettings["SenderPassword"];

            // Validate SMTP settings (avoid logging secrets)
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning(
                    "SMTP settings not configured. hostConfigured={HostConfigured}, senderConfigured={SenderConfigured}. Skipping email send.",
                    !string.IsNullOrEmpty(host),
                    !string.IsNullOrEmpty(sender)
                );
                return (true, "Welcome notification (email not configured)");
            }

            using (var smtpClient = new SmtpClient(host, port))
            {
                smtpClient.Credentials = new NetworkCredential(sender, password);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(sender, "Acoustican"),
                    Subject = "Welcome to Acoustican!",
                    Body = GenerateWelcomeEmailBody(userName),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Welcome email sent to {email}");
                return (true, "Welcome email sent successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending welcome email to {email}");
            return (false, "Failed to send welcome email");
        }
    }

    private string GeneratePasswordResetEmailBody(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #f5a623; font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
        .content {{ color: #333333; line-height: 1.6; }}
        .button {{ display: inline-block; background-color: #f5a623; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ color: #999999; font-size: 12px; margin-top: 30px; border-top: 1px solid #f0f0f0; padding-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>🎸 Acoustican</div>
        <div class='content'>
            <p>Hi {userName},</p>
            <p>We received a request to reset your password. Click the button below to create a new password:</p>
            <a href='{resetLink}' class='button'>Reset Password</a>
            <p style='margin-top: 20px; color: #666666; font-size: 14px;'>This link will expire in 24 hours.</p>
            <p style='color: #666666; font-size: 14px;'>If you didn't request a password reset, please ignore this email. Your account remains secure.</p>
        </div>
        <div class='footer'>
            <p>© 2026 Acoustican Academy. All rights reserved.</p>
            <p>Questions? Contact support@acoustican.com</p>
        </div>
    </div>
</body>
</html>
";
    }

    private string GenerateWelcomeEmailBody(string userName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 20px; border-radius: 8px; }}
        .header {{ color: #f5a623; font-size: 24px; font-weight: bold; margin-bottom: 20px; }}
        .content {{ color: #333333; line-height: 1.6; }}
        .button {{ display: inline-block; background-color: #f5a623; color: white; padding: 12px 30px; text-decoration: none; border-radius: 4px; margin-top: 20px; }}
        .footer {{ color: #999999; font-size: 12px; margin-top: 30px; border-top: 1px solid #f0f0f0; padding-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>🎸 Welcome to Acoustican!</div>
        <div class='content'>
            <p>Hi {userName},</p>
            <p>Welcome to Acoustican! We're excited to have you join our community of guitarists.</p>
            <p>Get started learning with our comprehensive courses designed for beginners to advanced players.</p>
            <a href='http://localhost:5000/courses' class='button'>Explore Courses</a>
        </div>
        <div class='footer'>
            <p>© 2026 Acoustican Academy. All rights reserved.</p>
            <p>Questions? Contact support@acoustican.com</p>
        </div>
    </div>
</body>
</html>
";
    }
}
