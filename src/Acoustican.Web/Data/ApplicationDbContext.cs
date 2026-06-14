using Microsoft.EntityFrameworkCore;
using Acoustican.Models;

namespace Acoustican.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AdminUser> AdminUsers { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<CourseModule> CourseModules { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Testimonial> Testimonials { get; set; } = null!;
    public DbSet<PricingTier> PricingTiers { get; set; } = null!;
    public DbSet<PricingFeature> PricingFeatures { get; set; } = null!;
    public DbSet<HeroContent> HeroContents { get; set; } = null!;
    public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Course relationships
        modelBuilder.Entity<Course>()
            .HasMany(c => c.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // CourseModule relationships
        modelBuilder.Entity<CourseModule>()
            .HasMany(m => m.Lessons)
            .WithOne(l => l.Module)
            .HasForeignKey(l => l.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);

        // PricingTier relationships
        modelBuilder.Entity<PricingTier>()
            .HasMany(p => p.Features)
            .WithOne(f => f.PricingTier)
            .HasForeignKey(f => f.PricingTierId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserSubscription relationships
        modelBuilder.Entity<UserSubscription>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSubscription>()
            .HasOne(s => s.PricingTier)
            .WithMany()
            .HasForeignKey(s => s.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserSubscription>()
            .HasIndex(s => new { s.UserId, s.Status });

        // CartItem relationships
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany()
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Course)
            .WithMany()
            .HasForeignKey(ci => ci.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasIndex(ci => new { ci.UserId, ci.CourseId })
            .IsUnique();

        // Order relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderItem relationships
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Course)
            .WithMany()
            .HasForeignKey(oi => oi.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

        // Decimal precision
        modelBuilder.Entity<Course>()
            .Property(c => c.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Course>()
            .Property(c => c.Rating)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PricingTier>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Price)
            .HasPrecision(18, 2);

        // Map AdminUser entity to AdminUsers table (matches existing migrations)
        modelBuilder.Entity<AdminUser>()
            .ToTable("AdminUsers");

        // Indexes
        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Title);

        modelBuilder.Entity<Testimonial>()
            .HasIndex(t => t.DisplayOrder);

        modelBuilder.Entity<PricingTier>()
            .HasIndex(p => p.DisplayOrder);

        // NOTE: Seed data is applied at runtime via DbInitializer.Initialize() in Program.cs.
        // HasData()-based seeding was removed because BCrypt.HashPassword() generates a
        // different hash on every run, causing EF model-snapshot mismatches.
    }
}
