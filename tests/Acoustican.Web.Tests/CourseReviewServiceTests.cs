using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using Acoustican.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Acoustican.Web.Tests;

public class CourseReviewServiceTests
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
    public async Task CanUserReviewCourseAsync_ShouldReturnTrue_WhenUserIsAdmin()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseReviewService(db, mapper);

        var adminUser = new AdminUser { Id = 1, Email = "admin@test.com", Role = "Admin" };
        db.AdminUsers.Add(adminUser);
        await db.SaveChangesAsync();

        // Act
        var result = await service.CanUserReviewCourseAsync(1, 101);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserReviewCourseAsync_ShouldReturnTrue_WhenUserHasActiveSubscription()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseReviewService(db, mapper);

        var user = new AdminUser { Id = 2, Email = "user@test.com", Role = "User" };
        var tier = new PricingTier { Id = 10, Name = "Gold", Price = 99m, BillingPeriod = "monthly" };
        var sub = new UserSubscription { UserId = 2, PricingTierId = 10, Status = "active" };

        db.AdminUsers.Add(user);
        db.PricingTiers.Add(tier);
        db.UserSubscriptions.Add(sub);
        await db.SaveChangesAsync();

        // Act
        var result = await service.CanUserReviewCourseAsync(2, 101);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserReviewCourseAsync_ShouldReturnTrue_WhenUserPurchasedCourse()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseReviewService(db, mapper);

        var user = new AdminUser { Id = 3, Email = "buyer@test.com", Role = "User" };
        var course = new Course { Id = 101, Title = "Guitar Chords", Description = "Learn Chords", Level = "Beginner" };
        var order = new Order { Id = 50, UserId = 3, Status = "Confirmed" };
        var orderItem = new OrderItem { OrderId = 50, CourseId = 101, CourseTitle = "Guitar Chords", Price = 49.99m };

        db.AdminUsers.Add(user);
        db.Courses.Add(course);
        db.Orders.Add(order);
        db.OrderItems.Add(orderItem);
        await db.SaveChangesAsync();

        // Act
        var result = await service.CanUserReviewCourseAsync(3, 101);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanUserReviewCourseAsync_ShouldReturnFalse_WhenUserNotEligible()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseReviewService(db, mapper);

        var user = new AdminUser { Id = 4, Email = "guest@test.com", Role = "User" };
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();

        // Act
        var result = await service.CanUserReviewCourseAsync(4, 101);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SubmitReviewAsync_ShouldAddReviewAndRecalculateStats()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var mapper = CreateMapper();
        var service = new CourseReviewService(db, mapper);

        var user1 = new AdminUser { Id = 10, Email = "u1@test.com", Role = "User", FullName = "User One" };
        var user2 = new AdminUser { Id = 11, Email = "u2@test.com", Role = "User", FullName = "User Two" };
        var course = new Course { Id = 200, Title = "Test Recalc", Description = "Test", Level = "Beginner", Rating = 0, ReviewCount = 0 };
        
        // Grant access via confirmed order
        var order1 = new Order { Id = 1, UserId = 10, Status = "Confirmed" };
        var orderItem1 = new OrderItem { OrderId = 1, CourseId = 200, CourseTitle = "Test Recalc", Price = 10m };
        var order2 = new Order { Id = 2, UserId = 11, Status = "Confirmed" };
        var orderItem2 = new OrderItem { OrderId = 2, CourseId = 200, CourseTitle = "Test Recalc", Price = 10m };

        db.AdminUsers.AddRange(user1, user2);
        db.Courses.Add(course);
        db.Orders.AddRange(order1, order2);
        db.OrderItems.AddRange(orderItem1, orderItem2);
        await db.SaveChangesAsync();

        // Act - Submit review 1 (rating = 4)
        var result1 = await service.SubmitReviewAsync(10, 200, new CreateReviewDto { Rating = 4, Comment = "Good" });
        
        // Assert review 1
        Assert.True(result1.Success);
        Assert.NotNull(result1.Review);
        Assert.Equal("User One", result1.Review.ReviewerName);

        var courseDbAfter1 = await db.Courses.FindAsync(200);
        Assert.Equal(4m, courseDbAfter1!.Rating);
        Assert.Equal(1, courseDbAfter1.ReviewCount);

        // Act - Submit review 2 (rating = 5)
        var result2 = await service.SubmitReviewAsync(11, 200, new CreateReviewDto { Rating = 5, Comment = "Awesome" });

        // Assert review 2
        Assert.True(result2.Success);
        var courseDbAfter2 = await db.Courses.FindAsync(200);
        Assert.Equal(4.5m, courseDbAfter2!.Rating); // (4+5)/2 = 4.5
        Assert.Equal(2, courseDbAfter2.ReviewCount);

        // Act - Update review 1 (rating = 5)
        var result3 = await service.SubmitReviewAsync(10, 200, new CreateReviewDto { Rating = 5, Comment = "Updated Good" });

        // Assert update
        Assert.True(result3.Success);
        var courseDbAfter3 = await db.Courses.FindAsync(200);
        Assert.Equal(5.0m, courseDbAfter3!.Rating); // (5+5)/2 = 5
        Assert.Equal(2, courseDbAfter3.ReviewCount);
    }
}
