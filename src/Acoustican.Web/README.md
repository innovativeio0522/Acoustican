# Acoustican Admin Panel

A comprehensive ASP.NET Core 8 admin panel for managing Acoustican course content, testimonials, and pricing.

## Features

- **Course Management**: Create, edit, publish/unpublish courses with modules and lessons
- **File Uploads**: Support for images, videos, and audio files with automatic validation
- **Testimonials**: Manage student testimonials with ratings and display order
- **Pricing Tiers**: Create and manage subscription pricing plans with features
- **JWT Authentication**: Secure API endpoints with JWT token-based authentication
- **Admin Users**: Role-based access control (Admin, ContentManager, Viewer)
- **RESTful API**: Complete API documentation with Swagger/OpenAPI
- **Entity Framework Core**: Database-first approach with SQL Server
- **AutoMapper**: Automatic DTO mapping

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full installation)
- Visual Studio Code or Visual Studio 2022

## Installation

1. **Clone the repository**
   ```bash
   cd AdminPanel
   ```

2. **Configure Database Connection**
   Update `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AcousticanAdminDB;Trusted_Connection=true;"
   }
   ```

3. **Change JWT Secret**
   In `appsettings.json`, replace the JWT key with a secure value:
   ```json
   "Jwt": {
     "Key": "your-super-secret-key-minimum-32-characters-required"
   }
   ```

4. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

5. **Apply Database Migrations**
   ```bash
   dotnet ef database update
   ```

6. **Run the Application**
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7001` (or the configured port).

## Project Structure

```
AdminPanel/
├── Models/              # Data models
├── DTOs/               # Data transfer objects
├── Services/           # Business logic
├── Controllers/        # API endpoints
├── Data/              # DbContext and configurations
├── Mappings/          # AutoMapper profiles
├── appsettings.json   # Configuration
└── Program.cs         # Application startup
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with email and password
- `POST /api/auth/register` - Register new admin user

### Courses
- `GET /api/courses` - Get all courses
- `GET /api/courses/{id}` - Get course by ID
- `POST /api/courses` - Create new course
- `PUT /api/courses/{id}` - Update course
- `DELETE /api/courses/{id}` - Delete course
- `POST /api/courses/{id}/publish` - Publish course
- `POST /api/courses/{id}/unpublish` - Unpublish course

### Testimonials
- `GET /api/testimonials` - Get all testimonials (admin only)
- `GET /api/testimonials/published` - Get published testimonials (public)
- `GET /api/testimonials/{id}` - Get testimonial by ID
- `POST /api/testimonials` - Create testimonial
- `PUT /api/testimonials/{id}` - Update testimonial
- `DELETE /api/testimonials/{id}` - Delete testimonial
- `POST /api/testimonials/{id}/publish` - Publish testimonial
- `POST /api/testimonials/{id}/unpublish` - Unpublish testimonial
- `POST /api/testimonials/{id}/upload-image` - Upload student image

### Pricing
- `GET /api/pricing` - Get all pricing tiers (admin only)
- `GET /api/pricing/published` - Get published tiers (public)
- `GET /api/pricing/{id}` - Get pricing tier by ID
- `POST /api/pricing` - Create pricing tier
- `PUT /api/pricing/{id}` - Update pricing tier
- `DELETE /api/pricing/{id}` - Delete pricing tier
- `POST /api/pricing/{id}/publish` - Publish tier
- `POST /api/pricing/{id}/unpublish` - Unpublish tier

### File Upload
- `POST /api/files/upload-image` - Upload image
- `POST /api/files/upload-video` - Upload video
- `POST /api/files/upload-audio` - Upload audio
- `DELETE /api/files/delete` - Delete file

## Database Models

### Course
- Courses have multiple modules
- Each module contains multiple lessons
- Tracks metadata like duration, rating, student count

### Testimonial
- Student name and role
- Rating (1-5 stars)
- Optional student image
- Display order and publish status

### PricingTier
- Price and billing period
- Multiple features per tier
- Popular/featured flag
- Display order

### AdminUser
- Role-based access (Admin, ContentManager, Viewer)
- Secure password hashing with BCrypt
- Login tracking

## Authentication

The API uses JWT (JSON Web Token) for authentication. To access protected endpoints:

1. Call `/api/auth/login` with email and password
2. Receive JWT token in response
3. Include token in Authorization header for subsequent requests:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

## File Upload Configuration

Configure file upload settings in `appsettings.json`:

```json
"FileUpload": {
  "MaxFileSize": 536870912,        // 512MB
  "AllowedVideoExtensions": ".mp4,.webm,.mov",
  "AllowedImageExtensions": ".jpg,.jpeg,.png,.webp,.gif",
  "AllowedAudioExtensions": ".mp3,.wav,.flac,.aac",
  "UploadPath": "uploads"
}
```

## Development

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName
```

Update database:
```bash
dotnet ef database update
```

### Swagger Documentation

Access API documentation at `/swagger` when running in development mode.

## Security Notes

- Always use HTTPS in production
- Change the JWT key to a secure value
- Use environment variables for sensitive data
- Implement rate limiting
- Validate all file uploads
- Use CORS policies appropriate for your frontend domain

## Frontend Integration

### Getting Published Content

Testimonials and pricing tiers have public endpoints that don't require authentication:

```javascript
// Get published testimonials
fetch('https://api.example.com/api/testimonials/published')
  .then(r => r.json())
  .then(data => console.log(data));

// Get published pricing
fetch('https://api.example.com/api/pricing/published')
  .then(r => r.json())
  .then(data => console.log(data));
```

### Admin Operations

For admin operations, include JWT token:

```javascript
const token = 'your-jwt-token';

fetch('https://api.example.com/api/courses', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify({
    title: 'Guitar Basics',
    description: 'Learn fundamental guitar techniques',
    level: 'Beginner',
    price: 29.99,
    durationMinutes: 120,
    isPublished: false
  })
})
.then(r => r.json())
.then(data => console.log(data));
```

## License

© 2026 Acoustican. All rights reserved.
