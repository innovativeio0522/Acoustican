# Acoustican Admin Panel - Setup & Installation Guide

## Quick Start

### Prerequisites
- .NET 8 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- SQL Server (LocalDB included with Visual Studio, or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads))
- Visual Studio Code or Visual Studio 2022

### Step 1: Navigate to the AdminPanel Directory
```bash
cd f:\Github\ Projects\Acoustican\AdminPanel
```

### Step 2: Restore NuGet Packages
```bash
dotnet restore
```

### Step 3: Update Connection String (Optional)
Edit `appsettings.json` if your SQL Server setup is different:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AcousticanAdminDB;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

### Step 4: Update JWT Secret (IMPORTANT!)
In `appsettings.json`, change the JWT key:
```json
"Jwt": {
  "Key": "your-production-key-minimum-32-characters-required-change-immediately"
}
```

### Step 5: Create Database & Run Migrations
```bash
dotnet ef database update
```

This will:
- Create the database
- Create all tables
- Seed initial data (admin user, sample pricing tiers, testimonial)

### Step 6: Run the Application
```bash
dotnet run
```

The API will start at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:7001
- **Swagger UI**: https://localhost:7001 (in development)

## Default Admin Credentials

**Email**: `admin@acoustican.com`  
**Password**: `Admin@123`

> ⚠️ **IMPORTANT**: Change these credentials immediately after first login!

## First Login

1. Go to `https://localhost:7001/swagger`
2. Click on the "Auth" section and expand "POST /api/auth/login"
3. Click "Try it out"
4. Enter credentials:
```json
{
  "email": "admin@acoustican.com",
  "password": "Admin@123"
}
```
5. Execute and copy the token from the response

## Authorize API Requests

1. Click the green "Authorize" button at the top of Swagger UI
2. Paste the token: `Bearer <your-token-here>`
3. Click "Authorize"
4. Now all endpoints are available for testing

## Project Structure

```
AdminPanel/
├── Models/                 # Data entities
├── DTOs/                   # Data transfer objects
├── Services/               # Business logic
├── Controllers/            # API endpoints
├── Data/                   # DbContext, seeding
├── Mappings/              # AutoMapper profiles
├── Migrations/            # EF Core migrations
├── Properties/            # Launch settings
├── appsettings.json       # Configuration
├── AdminPanel.csproj      # Project file
├── Program.cs             # Application startup
└── README.md              # API documentation
```

## Configuration

### File Uploads
Edit `appsettings.json`:
```json
"FileUpload": {
  "MaxFileSize": 536870912,              // 512MB in bytes
  "AllowedVideoExtensions": ".mp4,.webm,.mov",
  "AllowedImageExtensions": ".jpg,.jpeg,.png,.webp,.gif",
  "AllowedAudioExtensions": ".mp3,.wav,.flac,.aac",
  "UploadPath": "uploads"
}
```

Files will be uploaded to: `AdminPanel/uploads/{category}/{filename}`

### Database Configuration
Modify connection string in `appsettings.json`:

**SQL Server (Local Instance)**:
```
Server=.\SQLExpress;Database=AcousticanDB;Integrated Security=true;
```

**SQL Server (Named Instance)**:
```
Server=.\SQLEXPRESS;Database=AcousticanDB;Integrated Security=true;
```

**SQL Server (Remote)**:
```
Server=your-server.com;Database=AcousticanDB;User Id=sa;Password=your-password;
```

### JWT Configuration
In `appsettings.json`:
```json
"Jwt": {
  "Key": "your-secret-key-at-least-32-characters-long",
  "Issuer": "Acoustican",
  "Audience": "AcousticanUsers",
  "ExpirationMinutes": 60
}
```

## Common Tasks

### Create a New Admin User

**Via API** (requires admin token):
```bash
curl -X POST https://localhost:7001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "manager@acoustican.com",
    "password": "SecurePassword123",
    "fullName": "Content Manager",
    "role": "ContentManager"
  }'
```

### Add a New Course

```bash
curl -X POST https://localhost:7001/api/courses \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Advanced Fingerpicking",
    "description": "Master advanced fingerpicking techniques",
    "level": "Advanced",
    "price": 49.99,
    "durationMinutes": 480,
    "isPublished": false
  }'
```

### Upload a Course Thumbnail

```bash
curl -X POST https://localhost:7001/api/files/upload-image \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "file=@path/to/image.jpg"
```

### Publish a Course

```bash
curl -X POST https://localhost:7001/api/courses/1/publish \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Troubleshooting

### Database Connection Issues
```bash
# Test connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
```

### Migration Issues
```bash
# Remove last migration (if not applied to database yet)
dotnet ef migrations remove

# Create and apply migration
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

### Clear Database (Development Only)
```bash
# Drop and recreate database
dotnet ef database drop --force
dotnet ef database update
```

### Port Already in Use
Edit `Properties/launchSettings.json` and change the port numbers

### Swagger UI Not Loading
- Ensure you're in Development environment
- Check that Swashbuckle is properly configured in Program.cs
- Clear browser cache

## Deployment

### Production Checklist
- [ ] Change JWT secret to a secure value
- [ ] Change default admin credentials
- [ ] Use HTTPS only
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure proper CORS policy
- [ ] Set up database backups
- [ ] Enable CORS for frontend domain only
- [ ] Implement rate limiting
- [ ] Set up logging and monitoring
- [ ] Use environment variables for secrets

### Environment Variables
```powershell
# PowerShell
$env:ConnectionStrings__DefaultConnection = "your-prod-connection-string"
$env:Jwt__Key = "your-prod-jwt-key"
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

### IIS Deployment
1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Copy `publish` folder to IIS directory

3. Create IIS application pool (.NET 8)

4. Create IIS application pointing to `publish` folder

## Development Commands

```bash
# Build project
dotnet build

# Run tests
dotnet test

# Build for release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish

# Watch for file changes and rebuild
dotnet watch run

# Format code
dotnet format

# Run code analysis
dotnet publish --no-build /p:TreatWarningsAsErrors=true
```

## API Documentation

Once running, access:
- **Swagger UI**: https://localhost:7001 (Development)
- **Swagger JSON**: https://localhost:7001/swagger/v1/swagger.json
- **ReDoc**: Add to Swagger configuration if needed

## Frontend Integration

The API is designed to be consumed by the frontend. Add CORS headers to enable frontend communication:

```javascript
// Example fetch call from frontend
const token = localStorage.getItem('adminToken');

fetch('https://api.acoustican.com/api/courses', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(r => r.json())
.then(data => console.log(data));
```

## Support & Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://jwt.io/)
- [Swagger/OpenAPI](https://swagger.io/)

## License

© 2026 Acoustican. All rights reserved.
