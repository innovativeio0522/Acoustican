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
