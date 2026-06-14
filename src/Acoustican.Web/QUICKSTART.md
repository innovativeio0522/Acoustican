# Acoustican Admin Panel - Complete Setup Summary

## 🎯 What's Included

Complete ASP.NET Core 8 admin panel for managing all Acoustican content:

- ✅ **Database Models** - Courses, Modules, Lessons, Testimonials, Pricing
- ✅ **RESTful API** - Full CRUD operations with JWT authentication
- ✅ **File Upload** - Images, videos, audio files with validation
- ✅ **Admin Dashboard** - Web UI for content management
- ✅ **Publish/Unpublish** - Control what appears on the frontend
- ✅ **Public Endpoints** - Allow frontend to fetch published content
- ✅ **Swagger Documentation** - Interactive API explorer

## 📋 Project Structure

```
AdminPanel/
├── Models/                      # Database entities
│   ├── Course.cs
│   ├── CourseModule.cs
│   ├── Lesson.cs
│   ├── Testimonial.cs
│   ├── PricingTier.cs
│   ├── PricingFeature.cs
│   ├── HeroContent.cs
│   └── AdminUser.cs
├── DTOs/                        # Data transfer objects
├── Services/                    # Business logic
│   ├── AuthService.cs           # Authentication & JWT
│   ├── CourseService.cs         # Course management
│   ├── TestimonialService.cs   # Testimonial management
│   ├── PricingService.cs        # Pricing management
│   └── FileUploadService.cs    # File upload handling
├── Controllers/                 # API endpoints
│   ├── AuthController.cs
│   ├── CoursesController.cs
│   ├── TestimonialsController.cs
│   ├── PricingController.cs
│   └── FilesController.cs
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   ├── DatabaseSeeder.cs        # Initial data
├── Mappings/
│   └── MappingProfile.cs        # AutoMapper configuration
├── Migrations/                  # Database migrations
├── wwwroot/
│   ├── index.html              # Admin dashboard UI
│   └── uploads/                # Uploaded files
├── Properties/
│   └── launchSettings.json
├── appsettings.json            # Configuration
├── AdminPanel.csproj           # Project file
├── Program.cs                  # Application startup
├── SETUP.md                    # Detailed setup guide
├── FRONTEND_INTEGRATION.md     # Frontend integration guide
└── README.md                   # API documentation
```

## 🚀 Quick Start (5 minutes)

### 1. Prerequisites
```bash
# Install .NET 8 SDK
# Install SQL Server (or use LocalDB)
```

### 2. Navigate to Project
```bash
cd "f:\Github Projects\Acoustican\AdminPanel"
```

### 3. Restore & Setup Database
```bash
dotnet restore
dotnet ef database update
```

### 4. Run Application
```bash
dotnet run
```

### 5. Access Admin Panel
- **Dashboard**: https://localhost:7001
- **Swagger API**: https://localhost:7001/swagger
- **Default Login**: admin@acoustican.com / Admin@123

## 🔑 Default Credentials

| Field | Value |
|-------|-------|
| Email | admin@acoustican.com |
| Password | Admin@123 |

⚠️ **IMPORTANT**: Change immediately after first login!

## 📚 API Endpoints

### Authentication
```
POST   /api/auth/login          → Login and get JWT token
POST   /api/auth/register       → Register new admin user
```

### Courses (Admin only)
```
GET    /api/courses             → Get all courses
GET    /api/courses/{id}        → Get course details
POST   /api/courses             → Create course
PUT    /api/courses/{id}        → Update course
DELETE /api/courses/{id}        → Delete course
POST   /api/courses/{id}/publish     → Publish course
POST   /api/courses/{id}/unpublish   → Unpublish course
```

### Testimonials
```
GET    /api/testimonials                → Get all (admin)
GET    /api/testimonials/published      → Get published (public)
GET    /api/testimonials/{id}           → Get specific
POST   /api/testimonials                → Create
PUT    /api/testimonials/{id}           → Update
DELETE /api/testimonials/{id}           → Delete
POST   /api/testimonials/{id}/publish   → Publish
```

### Pricing
```
GET    /api/pricing              → Get all (admin)
GET    /api/pricing/published    → Get published (public)
GET    /api/pricing/{id}         → Get specific
POST   /api/pricing              → Create
PUT    /api/pricing/{id}         → Update
DELETE /api/pricing/{id}         → Delete
POST   /api/pricing/{id}/publish → Publish
```

### File Upload
```
POST   /api/files/upload-image   → Upload image
POST   /api/files/upload-video   → Upload video
POST   /api/files/upload-audio   → Upload audio
DELETE /api/files/delete         → Delete file
```

## 🔐 Authentication

### Get JWT Token
```javascript
const response = await fetch('https://localhost:7001/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'admin@acoustican.com',
    password: 'Admin@123'
  })
});

const { token } = await response.json();
localStorage.setItem('adminToken', token);
```

### Use Token for Requests
```javascript
fetch('https://localhost:7001/api/courses', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

## 📤 File Upload

### Configuration (appsettings.json)
```json
"FileUpload": {
  "MaxFileSize": 536870912,
  "AllowedImageExtensions": ".jpg,.jpeg,.png,.webp,.gif",
  "AllowedVideoExtensions": ".mp4,.webm,.mov",
  "AllowedAudioExtensions": ".mp3,.wav,.flac,.aac",
  "UploadPath": "uploads"
}
```

### Upload Files
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch('https://localhost:7001/api/files/upload-image', {
  method: 'POST',
  headers: { 'Authorization': `Bearer ${token}` },
  body: formData
});

const result = await response.json();
console.log(result.filePath); // /uploads/images/guid_timestamp.jpg
```

## 🌐 Frontend Integration

### Load Published Testimonials
```javascript
const testimonials = await fetch('https://localhost:7001/api/testimonials/published')
  .then(r => r.json());

testimonials.forEach(t => {
  console.log(`${t.studentName}: ${t.content} (${t.rating}/5 stars)`);
});
```

### Load Published Pricing
```javascript
const pricing = await fetch('https://localhost:7001/api/pricing/published')
  .then(r => r.json());

pricing.sort((a, b) => a.displayOrder - b.displayOrder);
console.log(pricing);
```

### Load Published Courses
```javascript
const courses = await fetch('https://localhost:7001/api/courses')
  .then(r => r.json())
  .then(courses => courses.filter(c => c.isPublished));
```

See [FRONTEND_INTEGRATION.md](./FRONTEND_INTEGRATION.md) for complete integration examples.

## 🔧 Configuration

### Connection String (appsettings.json)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AcousticanAdminDB;Trusted_Connection=true;"
}
```

### JWT Settings
```json
"Jwt": {
  "Key": "your-secret-key-minimum-32-characters",
  "Issuer": "Acoustican",
  "Audience": "AcousticanUsers",
  "ExpirationMinutes": 60
}
```

### CORS
By default, allows all origins. Change in `Program.cs` for production:
```csharp
options.WithOrigins("https://acoustican.com", "https://www.acoustican.com");
```

## 📊 Database Models

### Course
- Title, description, level (Beginner/Intermediate/Advanced)
- Price, duration, student count, rating
- Thumbnail URL, publish status
- One-to-many relationship with CourseModules

### CourseModule
- Part of a course with module number
- Contains lessons
- Title, description, duration, thumbnail

### Lesson
- Part of a module
- Video URL, thumbnail, duration
- Rich text content, display order

### Testimonial
- Student name, role, profile image
- Rating (1-5 stars), content
- Display order, publish status

### PricingTier
- Name, price, billing period
- Description, icon, popular flag
- One-to-many relationship with PricingFeatures

### PricingFeature
- Feature text, included flag
- Display order per tier

### AdminUser
- Email (unique), password hash, full name
- Role-based (Admin, ContentManager, Viewer)
- Last login tracking

## 🧪 Testing API

### Using Swagger UI
1. Go to https://localhost:7001/swagger
2. Click "Authorize" button
3. Paste JWT token: `Bearer your-token-here`
4. Use "Try it out" on any endpoint

### Using curl
```bash
# Login
curl -X POST https://localhost:7001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@acoustican.com","password":"Admin@123"}'

# Get courses (using token from login response)
curl https://localhost:7001/api/courses \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Using Postman
1. Create new POST request
2. URL: `https://localhost:7001/api/auth/login`
3. Body (JSON):
```json
{
  "email": "admin@acoustican.com",
  "password": "Admin@123"
}
```
4. Copy `token` from response
5. Set Authorization header for subsequent requests

## 📝 Common Tasks

### Add Course
```bash
curl -X POST https://localhost:7001/api/courses \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Guitar Basics",
    "description": "Learn fundamental techniques",
    "level": "Beginner",
    "price": 29.99,
    "durationMinutes": 120
  }'
```

### Publish Course
```bash
curl -X POST https://localhost:7001/api/courses/1/publish \
  -H "Authorization: Bearer TOKEN"
```

### Add Testimonial
```bash
curl -X POST https://localhost:7001/api/testimonials \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "studentName": "John Doe",
    "studentRole": "Musician",
    "content": "Great course!",
    "rating": 5,
    "isPublished": true
  }'
```

### Create Pricing Tier
```bash
curl -X POST https://localhost:7001/api/pricing \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Professional",
    "price": 24.99,
    "billingPeriod": "monthly",
    "isPopular": true,
    "features": ["All Courses", "Unlimited Access", "Certificate"]
  }'
```

## 🚨 Troubleshooting

| Issue | Solution |
|-------|----------|
| Database won't connect | Check connection string in appsettings.json |
| Port already in use | Change port in launchSettings.json |
| JWT token invalid | Regenerate token, check expiration |
| File upload fails | Check file size and allowed extensions |
| CORS error | Update CORS policy in Program.cs |
| Swagger not loading | Ensure running in Development |

See [SETUP.md](./SETUP.md#troubleshooting) for detailed troubleshooting.

## 📦 Deployment

### Before Deployment
- [ ] Change default admin credentials
- [ ] Update JWT secret key
- [ ] Configure production database
- [ ] Update CORS origins
- [ ] Enable HTTPS only
- [ ] Set environment to Production

### Deploy to IIS
```bash
dotnet publish -c Release -o ./publish
```
Then copy `publish` folder to IIS application directory.

### Deploy to Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish .
ENTRYPOINT ["dotnet", "AdminPanel.dll"]
```

## 📚 Documentation

- **[README.md](./README.md)** - Complete API documentation
- **[SETUP.md](./SETUP.md)** - Detailed installation & configuration
- **[FRONTEND_INTEGRATION.md](./FRONTEND_INTEGRATION.md)** - Frontend integration examples

## 🎓 Learning Resources

- [ASP.NET Core 8 Docs](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [JWT Authentication](https://tools.ietf.org/html/rfc7519)
- [Swagger/OpenAPI](https://swagger.io/resources/articles/best-practices-in-api-design/)

## 💡 Tips & Best Practices

1. **Always use HTTPS** in production
2. **Rotate JWT secrets** periodically
3. **Implement rate limiting** on production APIs
4. **Validate all file uploads** thoroughly
5. **Use environment variables** for secrets
6. **Monitor API usage** and errors
7. **Back up database** regularly
8. **Test API endpoints** before frontend integration
9. **Implement proper logging** and monitoring
10. **Use caching** for frequently accessed data

## 🤝 Support

For issues or questions:
1. Check the relevant documentation file
2. Review the Swagger API documentation
3. Check API response error messages
4. Enable logging for debugging

## ✅ Next Steps

1. ✓ Complete admin panel setup
2. ☐ Configure production environment
3. ☐ Integrate frontend with API
4. ☐ Add course content
5. ☐ Create testimonials
6. ☐ Set up pricing tiers
7. ☐ Deploy to production
8. ☐ Monitor and maintain

## 📄 License

© 2026 Acoustican. All rights reserved.

---

**Happy learning and content management! 🎸**
