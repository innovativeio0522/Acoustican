# Acoustican

ASP.NET Core 8 web application for managing the Acoustican course platform — includes a RESTful API, admin dashboard, and MVC frontend.

## Tech Stack

- **Runtime:** .NET 8
- **Framework:** ASP.NET Core (API + MVC)
- **Database:** SQL Server with Entity Framework Core
- **Auth:** JWT Bearer tokens with role-based access (Admin, ContentManager, Viewer)
- **Mapping:** AutoMapper
- **Passwords:** BCrypt
- **API Docs:** Swagger / OpenAPI
- **Tests:** xUnit + Moq

## Project Structure

```
Acoustican/
├── Acoustican.sln
├── src/
│   └── Acoustican.Web/            # Main web application
│       ├── Controllers/            # API + MVC controllers
│       │   └── Mvc/               # View-serving controllers
│       ├── Data/                  # DbContext, seeders, migrations
│       ├── DTOs/                  # Request/response data transfer objects
│       ├── Mappings/              # AutoMapper profiles
│       ├── Models/                # Entity models
│       ├── Services/              # Business logic (interface-based)
│       ├── ViewModels/            # MVC view models
│       ├── Views/                 # Razor views
│       ├── wwwroot/               # Static assets
│       ├── Program.cs             # App startup & middleware pipeline
│       └── Acoustican.Web.csproj
└── tests/
    └── Acoustican.Web.Tests/      # xUnit test project
        └── Acoustican.Web.Tests.csproj
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (Express, LocalDB, or full installation)

## Setup

### 1. Clone and restore

```bash
git clone <repo-url>
cd Acoustican
dotnet restore Acoustican.sln
```

### 2. Configure the database

Edit `src/Acoustican.Web/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=AcousticanAdminDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Configure secrets (development)

Use [user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for sensitive values:

```bash
cd src/Acoustican.Web

dotnet user-secrets set "Jwt:Key" "your-secure-key-minimum-32-characters"
dotnet user-secrets set "EmailSettings:Smtp:SenderPassword" "your-app-password"
dotnet user-secrets set "VdoCipher:ApiKey" "your-vdocipher-key"
```

### 4. Apply migrations

```bash
cd src/Acoustican.Web
dotnet ef database update
```

In development, migrations also run automatically on startup.

### 5. Run

```bash
dotnet run --project src/Acoustican.Web
```

- **API:** `http://localhost:5000`
- **Swagger (dev):** `http://localhost:5000/swagger`
- **Admin dashboard:** `http://localhost:5000/admin`

## Build & Test

```bash
# Build entire solution
dotnet build Acoustican.sln

# Run tests
dotnet test Acoustican.sln
```

## API Endpoints

### Authentication

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | Public | Login with email & password |
| POST | `/api/auth/register` | Admin | Create new admin user |
| POST | `/api/auth/forgot-password` | Public | Request password reset |
| POST | `/api/auth/reset-password` | Public | Reset password with token |

### Courses

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/courses` | Public | List all courses |
| GET | `/api/courses/{id}` | Public | Get course by ID |
| POST | `/api/courses` | Admin/CM | Create course |
| PUT | `/api/courses/{id}` | Admin/CM | Update course |
| DELETE | `/api/courses/{id}` | Admin/CM | Delete course |
| POST | `/api/courses/{id}/publish` | Admin/CM | Publish course |
| POST | `/api/courses/{id}/unpublish` | Admin/CM | Unpublish course |
| POST | `/api/courses/{id}/upload-thumbnail` | Admin/CM | Upload thumbnail |

### Modules

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/coursemodules/course/{courseId}` | Public | Get modules by course |
| GET | `/api/coursemodules/published` | Public | Get published modules |
| GET | `/api/coursemodules/{id}` | Public | Get module by ID |
| POST | `/api/coursemodules` | Admin/CM | Create module |
| PUT | `/api/coursemodules/{id}` | Admin/CM | Update module |
| DELETE | `/api/coursemodules/{id}` | Admin/CM | Delete module |

### Lessons

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/lessons/module/{moduleId}` | Public | Get lessons by module |
| GET | `/api/lessons/{id}` | Public | Get lesson by ID |
| POST | `/api/lessons` | Admin/CM | Create lesson |
| PUT | `/api/lessons/{id}` | Admin/CM | Update lesson |
| DELETE | `/api/lessons/{id}` | Admin/CM | Delete lesson |

### Testimonials

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/testimonials` | Any role | List all testimonials |
| GET | `/api/testimonials/published` | Public | Get published testimonials |
| POST | `/api/testimonials` | Admin/CM | Create testimonial |
| PUT | `/api/testimonials/{id}` | Admin/CM | Update testimonial |
| DELETE | `/api/testimonials/{id}` | Admin/CM | Delete testimonial |

### Pricing

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/pricing` | Any role | List all tiers |
| GET | `/api/pricing/published` | Public | Get published tiers |
| POST | `/api/pricing` | Admin/CM | Create tier |
| PUT | `/api/pricing/{id}` | Admin/CM | Update tier |
| DELETE | `/api/pricing/{id}` | Admin/CM | Delete tier |

### Files

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/files/upload-image` | Admin/CM | Upload image |
| POST | `/api/files/upload-video` | Admin/CM | Upload video |
| POST | `/api/files/upload-audio` | Admin/CM | Upload audio |
| DELETE | `/api/files/delete` | Admin/CM | Delete file |

### Contact

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/contact` | Public | Submit contact message |
| GET | `/api/contact` | Admin | List all messages |
| GET | `/api/contact/{id}` | Admin | Get message by ID |
| PUT | `/api/contact/{id}/mark-read` | Admin | Mark as read |
| DELETE | `/api/contact/{id}` | Admin | Delete message |

> **Admin/CM** = Admin or ContentManager role required.  
> **Any role** = Any authenticated user (Admin, ContentManager, or Viewer).

## Authentication

1. Call `POST /api/auth/login` with email and password
2. Use the returned token in subsequent requests:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

## Development

### Add a migration

```bash
cd src/Acoustican.Web
dotnet ef migrations add MigrationName
```

### Update database

```bash
cd src/Acoustican.Web
dotnet ef database update
```

### CORS

Configure allowed origins in `appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": ["http://localhost:3000", "https://yourdomain.com"]
}
```

## Security Checklist

- JWT key stored in user-secrets, never in `appsettings.json`
- SMTP and API credentials in user-secrets
- Register endpoint restricted to Admin role
- Password reset tokens delivered via email only
- Global exception handler in production
- Input validation on all DTOs
- File upload size and extension restrictions
- Auto-migrations limited to development environment

## License

&copy; 2026 Acoustican. All rights reserved.
