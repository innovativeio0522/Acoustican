# Code Review - Issues to Fix

## Priority: HIGH

### 1. Public Register Endpoint (Security)
**File:** `Controllers/AuthController.cs` (lines 48-65)
**Problem:** `POST /api/auth/register` is `[AllowAnonymous]`. Anyone on the internet can create admin accounts.
**Fix:** Either remove the endpoint entirely, require a secret bootstrap key, or restrict it to authenticated Admin users only.

```csharp
// Option A: Remove it entirely after initial setup
// Option B: Protect it behind admin auth
[HttpPost("register")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> Register(...)
```

---

### 2. Password Reset Token Returned to Client (Security)
**File:** `Controllers/AuthController.cs` (line 79)
**Problem:** The reset token is returned in the API response (`resetToken = token`). This defeats the entire purpose of password reset — the token should only be delivered via email to the verified owner.
**Fix:** Remove the token from the response. Only send it through email.

```csharp
// REMOVE this:
return Ok(new { success, message, resetToken = token });

// REPLACE with:
return Ok(new { success, message });
```

---

### 3. Fallback JWT Key in Production (Security)
**File:** `Program.cs` (lines 17-21)
**Problem:** If `Jwt:Key` is missing from config, a hardcoded fallback key is used. In production, this means the app runs with a known, insecure key.
**Fix:** Throw an exception in non-development environments if the key is missing.

```csharp
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
        jwtKey = "TemporaryDesignTimeKeyForEFMigrationsToolOnly_MustBeChangedInProduction!";
    else
        throw new InvalidOperationException("JWT key must be configured in production.");
}
```

---

## Priority: MEDIUM

### 4. God Service - CourseService Handles Too Much
**File:** `Services/CourseService.cs` (228 lines)
**Problem:** `CourseService` implements methods for Courses, Modules, AND Lessons. The `ICourseService` interface is bloated with 18 methods.
**Fix:** Split into separate services:
- `ICourseService` / `CourseService` — course CRUD + publish/unpublish
- `IModuleService` / `ModuleService` — module CRUD
- `ILessonService` / `LessonService` — lesson CRUD

Update `Program.cs` registrations accordingly.

---

### 5. No Input Validation on DTOs
**Files:** All files in `DTOs/`
**Problem:** DTOs lack data annotations (`[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`). Invalid data can reach the service layer.
**Fix:** Add validation attributes. Example:

```csharp
public class CreateCourseDto
{
    [Required, StringLength(200)]
    public string Title { get; set; }

    [Required, StringLength(5000)]
    public string Description { get; set; }

    [Range(0, 99999.99)]
    public decimal Price { get; set; }
}
```

Since controllers use `[ApiController]`, model validation errors will automatically return 400 responses.

---

### 6. No Global Exception Handling
**Files:** All controllers
**Problem:** Every action method has its own try/catch with nearly identical error handling. This is repetitive and easy to forget.
**Fix:** Add a global exception handler middleware or use the built-in problem details:

```csharp
// In Program.cs, before app.UseAuthorization():
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(builder =>
    {
        builder.Run(async context =>
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
        });
    });
}
```

Then remove try/catch blocks from individual actions.

---

### 7. Auto-Migration at Startup (Production Risk)
**File:** `Program.cs` (lines 124-129)
**Problem:** `dbContext.Database.Migrate()` runs on every app start. In production with multiple instances, concurrent migrations can corrupt the database.
**Fix:**
- **Dev:** Keep as-is for convenience.
- **Production:** Remove auto-migration and run `dotnet ef database update` in CI/CD pipeline or a startup task.

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    DbInitializer.Initialize(db);
}
```

---

## Priority: LOW

### 8. Inconsistent Constructor Style
**Files:** `AuthController.cs` vs `CoursesController.cs`
**Problem:** `AuthController` uses C# primary constructors, while `CoursesController` uses traditional constructor injection. Mixing styles hurts readability.
**Fix:** Pick one style and apply it consistently. Primary constructors are cleaner:

```csharp
public class CoursesController(ICourseService courseService, IFileUploadService fileUploadService, ILogger<CoursesController> logger) : ControllerBase
{
    // Use courseService, fileUploadService, logger directly
}
```

---

### 9. Indentation Inconsistency
**File:** `Program.cs` (lines 75-76)
**Problem:** Lines 75-76 have extra indentation compared to surrounding lines.
**Fix:** Align the indentation:

```csharp
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContactService, ContactService>();
```

---

### 10. No Unit Tests
**Problem:** There is no test project. The service layer is well-structured with interfaces (ready for mocking), but no tests exist.
**Fix:** Create a test project:

```bash
dotnet new xunit -n AdminPanel.Tests
dotnet add AdminPanel.Tests reference AdminPanel/AdminPanel.csproj
dotnet add AdminPanel.Tests package Moq
```

Start with testing service methods (e.g., `CourseService.CreateCourseAsync`, `AuthService.AuthenticateAsync`).

---

## Checklist

- [ ] Lock down or remove public register endpoint
- [ ] Stop returning password reset token in API response
- [ ] Guard JWT key fallback — dev-only
- [ ] Split CourseService into Course/Module/Lesson services
- [ ] Add data annotation validation to all DTOs
- [ ] Add global exception handling middleware
- [ ] Restrict auto-migration to development only
- [ ] Unify constructor injection style across controllers
- [ ] Fix indentation in Program.cs lines 75-76
- [ ] Create test project and add unit tests
