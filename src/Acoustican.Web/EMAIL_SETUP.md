# Email Configuration for Password Reset

## Overview
The password reset functionality now sends emails to users. EmailService is integrated into the authentication workflow.

## How It Works

1. **User Request**: User enters email on `/forgot-password` page
2. **Token Generation**: AuthService generates a secure 24-hour reset token
3. **Email Send**: EmailService sends reset link via SMTP
4. **Password Reset**: User clicks link in email, goes to `/reset-password?token=...`
5. **Validation**: Token is validated before allowing password change

## Configuration

### Step 1: Configure SMTP (do NOT store passwords in repo)

This project reads SMTP settings from configuration.
In ASP.NET Core, **environment variables override `appsettings.json`**, so store the Gmail app password only as an environment variable.

Set these environment variables:

- `EmailSettings__Smtp__Host` = `smtp.gmail.com`
- `EmailSettings__Smtp__Port` = `587`
- `EmailSettings__Smtp__SenderEmail` = `your-email@gmail.com`
- `EmailSettings__Smtp__SenderPassword` = `your-gmail-app-password`

Notes:
- Keep `SenderPassword` out of `appsettings.json` / source control.
- `EmailService` will skip sending emails (and return the generic password-reset message) if SMTP values are not configured.
- `App:BaseUrl` should be set correctly for your environment (localhost vs production).

### Step 2: Gmail Setup (if using Gmail)

1. Enable 2-Step Verification on your Google Account
2. Generate App Password: https://myaccount.google.com/apppasswords
3. Use generated 16-character password as `SenderPassword`

### Step 3: Testing

#### Test 1: Manual Email Submission
```bash
POST http://localhost:5000/api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}
```

Expected Response:
```json
{
  "success": true,
  "message": "If an account exists with this email, you will receive a password reset link",
  "resetToken": null
}
```

#### Test 2: Check Email
- Check inbox for "Reset Your Acoustican Password" email
- Click link in email to go to reset-password page

#### Test 3: Password Reset
1. Open reset link from email or navigate to: `/reset-password?token={token}`
2. Enter new password
3. Confirm password
4. Click "Reset Password"

## Without Email Configuration

If SMTP is not configured:
- API endpoint still returns success (prevents email enumeration attacks)
- Users see generic message: "If an account exists with this email, you will receive a password reset link"
- Tokens are generated and saved but not emailed

## Important Security Notes

1. **Token Expiry**: Tokens expire after 24 hours
2. **One-Time Use**: Tokens are cleared after successful password reset
3. **Email Privacy**: System doesn't reveal if email exists (prevents account enumeration)
4. **App Password**: Use Gmail app-specific password, not account password
5. **SSL/TLS**: SMTP uses SSL/TLS encryption by default (Port 587 or 465)

## Files Modified

- `Services/AuthService.cs` - Integrated EmailService into RequestPasswordResetAsync
- `Services/EmailService.cs` - SMTP implementation with HTML email templates
- `Program.cs` - Registered IEmailService in dependency injection
- `appsettings.json` - Added EmailSettings and App:BaseUrl configuration
- `Views/Home/ForgotPassword.cshtml` - Updated to not display tokens on page
- `Controllers/AuthService.cs` - Updated constructor to inject IEmailService

## Troubleshooting

### Emails Not Sending
1. Check SMTP credentials in appsettings.json
2. Check application logs for email send errors
3. Verify firewall allows outbound SMTP (ports 587 or 465)
4. Test with a test email service first (MailKit, SendGrid, etc.)

### Gmail Errors
- Error: "Less secure apps": Generate App Password instead
- Error: "Invalid credentials": Double-check app password (16 chars with spaces)
- Error: "Quota exceeded": Check Gmail sending limits

### Production Considerations
1. Use environment variables for sensitive credentials
2. Change `App:BaseUrl` to production domain
3. Consider using managed email service (SendGrid, AWS SES)
4. Implement email verification queue for reliability
