using System.Text;
using Acoustican.Data;
using Acoustican.Mappings;
using Acoustican.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsDevelopment())
    {
        jwtKey = "TemporaryDesignTimeKeyForEFMigrationsToolOnly_MustBeChangedInProduction!";
        builder.Configuration["Jwt:Key"] = jwtKey;
    }
    else
        throw new InvalidOperationException("Jwt:Key must be configured in non-development environments.");
}


var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(options =>
    {
        // Use JWT for API authorization by default.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = "Cookies";
    })
    .AddCookie("Cookies")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";

        // Route must match the one in appsettings.json.
        options.CallbackPath = builder.Configuration["Google:CallbackPath"] ?? "/api/auth/google/callback";

        options.SaveTokens = true;

        // Fix for HTTP localhost: the correlation cookie defaults to SameSite=None,
        // which Chrome drops on HTTP (requires Secure). Override it explicitly so
        // the cookie is sent back when Google redirects to the callback.
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.CorrelationCookie.HttpOnly = true;
    });



// Add CORS. Prefer Cors:AllowedOrigins, but keep App:AllowedOrigins as a
// backwards-compatible fallback for the existing local configuration.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins = builder.Configuration
        .GetSection("App:AllowedOrigins")
        .Get<string[]>() ?? [];
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add HttpClient
builder.Services.AddHttpClient();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITestimonialService, TestimonialService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();


// Add Controllers
builder.Services.AddControllersWithViews();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Acoustican Admin API",
        Version = "v1",
        Description = "API for managing Acoustican content"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

// Needed for Google OAuth state/correlation cookie to persist on localhost HTTP.
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();


// Apply migrations and seed database on startup
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
        DbInitializer.Initialize(dbContext);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Acoustican Admin API v1");
    });
}

// Global exception handling
if (!app.Environment.IsDevelopment())
{
    if (app.Configuration.GetValue<bool>("Logging:ShowDetailedErrors"))
    {
        app.UseDeveloperExceptionPage();
    }
    else
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
}

// Enable CORS
app.UseCors("AllowConfiguredOrigins");

// Serve static files (uploaded files and admin dashboard)
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
