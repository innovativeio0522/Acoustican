using Acoustican.DTOs;
using Acoustican.Models;
using Acoustican.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Acoustican.Data;

namespace Acoustican.Web.Tests;

public class CourseServiceTests
{
    private ApplicationDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<Acoustican.Mappings.MappingProfile>());
        return config.CreateMapper();
    }

    [Fact]
    public async Task CreateCourseAsync_ShouldAddCourseToDatabase()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var logger = new Mock<ILogger<CourseService>>();
        var service = new CourseService(db, mapper);

        var dto = new CreateCourseDto
        {
            Title = "Test Course",
            Description = "A test course",
            Level = "Beginner",
            Price = 49.99m,
            DurationMinutes = 120,
            InstructorName = "Test Instructor"
        };

        // Act
        var result = await service.CreateCourseAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Course", result.Title);
        Assert.Equal(1, await db.Courses.CountAsync());
    }

    [Fact]
    public async Task GetCourseByIdAsync_ShouldReturnNull_WhenCourseNotFound()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseService(db, mapper);

        // Act
        var result = await service.GetCourseByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteCourseAsync_ShouldReturnFalse_WhenCourseNotFound()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseService(db, mapper);

        // Act
        var result = await service.DeleteCourseAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task PublishCourseAsync_ShouldSetIsPublishedTrue()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseService(db, mapper);

        db.Courses.Add(new Course
        {
            Title = "Unpublished",
            Description = "Test",
            Level = "Beginner",
            Price = 0,
            DurationMinutes = 60,
            InstructorName = "Test",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.PublishCourseAsync(1);

        // Assert
        Assert.True(result);
        var course = await db.Courses.FindAsync(1);
        Assert.True(course!.IsPublished);
    }
}
