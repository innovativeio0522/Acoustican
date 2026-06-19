using System;
using System.Collections.Generic;
using System.Linq;
using Acoustican.Models;

namespace Acoustican.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        try
        {
            // Check if database has been seeded already
            var existingCourses = context.Courses.ToList();
            var updateSeedDateTime = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

            // Update existing courses with missing fields
            if (existingCourses.Any())
            {
                var needsUpdate = false;

                foreach (var course in existingCourses)
                {
                    var courseNeedsUpdate = false;
                    if (course.OriginalPrice == 0)
                    {
                        if (course.Title == "Guitar Basics Masterclass")
                        {
                            course.OriginalPrice = 149.99m;
                            course.LectureCount = 40;
                            course.InstructorName = "Erich Andreas";
                            course.StudentCount = 1200;
                            course.IsBestseller = true;
                        }
                        else if (course.Title == "Fingerstyle Essentials")
                        {
                            course.OriginalPrice = 179.99m;
                            course.LectureCount = 35;
                            course.InstructorName = "Henry Olsen";
                            course.StudentCount = 800;
                            course.IsBestseller = true;
                        }
                        courseNeedsUpdate = true;
                    }
                    
                    // Add new fields if they're empty
                    if (string.IsNullOrEmpty(course.WhatYoullLearn))
                    {
                        if (course.Title == "Guitar Basics Masterclass")
                        {
                            course.WhatYoullLearn = "Master fundamental guitar techniques,Learn chord progressions and strumming patterns,Understand music theory for guitar,Develop your own playing style";
                        }
                        else if (course.Title == "Fingerstyle Essentials")
                        {
                            course.WhatYoullLearn = "Master PIMA technique,Learn Travis picking,Understand classical fingerstyle patterns,Develop finger independence";
                        }
                        else if (course.Title == "Blues Soloing Mastery")
                        {
                            course.WhatYoullLearn = "Learn blues scales,Master bending techniques,Improvisation secrets,Develop vibrato control";
                        }
                        courseNeedsUpdate = true;
                    }
                    
                    if (string.IsNullOrEmpty(course.Requirements))
                    {
                        course.Requirements = "A guitar (acoustic or electric),Basic understanding of music (not required),Passion for learning!";
                        courseNeedsUpdate = true;
                    }

                    if (courseNeedsUpdate)
                    {
                        needsUpdate = true;
                    }
                }

                if (needsUpdate)
                {
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Updated existing courses with missing fields.");
                }

                // Add course 3 if it doesn't exist
                var newCourse3 = existingCourses.FirstOrDefault(c => c.Title == "Blues Soloing Mastery");
                if (newCourse3 == null)
                {
                    newCourse3 = new Course
                    {
                        Title = "Blues Soloing Mastery",
                        Description = "Learn blues scales, bending techniques, and improvisation secrets.",
                        Level = "Advanced",
                        Price = 79.99m,
                        OriginalPrice = 229.99m,
                        DurationMinutes = 680,
                        LectureCount = 45,
                        InstructorName = "Marco Cirillo",
                        StudentCount = 600,
                        ThumbnailUrl = "https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=600&h=340&fit=crop&q=80",
                        IsPublished = true,
                        IsBestseller = false,
                        Rating = 4.7m,
                        ReviewCount = 12,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };
                    context.Courses.Add(newCourse3);
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Added Blues Soloing Mastery course.");
                }

                // Check if modules exist for course 1
                var targetCourse1 = existingCourses.FirstOrDefault(c => c.Title == "Guitar Basics Masterclass");
                if (targetCourse1 != null && !context.CourseModules.Any(m => m.CourseId == targetCourse1.Id))
                {
                    var newModule1 = new CourseModule
                    {
                        CourseId = targetCourse1.Id,
                        ModuleNumber = 1,
                        Title = "Getting Started",
                        Description = "Learn the basics of holding the guitar and tuning",
                        DurationMinutes = 60,
                        DisplayOrder = 1,
                        IsPublished = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };

                    var newModule2 = new CourseModule
                    {
                        CourseId = targetCourse1.Id,
                        ModuleNumber = 2,
                        Title = "Open Chords",
                        Description = "Master the essential open chords for beginners",
                        DurationMinutes = 120,
                        DisplayOrder = 2,
                        IsPublished = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };
                    context.CourseModules.AddRange(newModule1, newModule2);
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Added modules to Guitar Basics Masterclass.");

                    var newLesson1 = new Lesson
                    {
                        ModuleId = newModule1.Id,
                        Title = "How to Hold Your Guitar",
                        Description = "Proper posture and holding technique",
                        VideoUrl = null, // Admin must set a VdoCipher Video ID via admin panel
                        ThumbnailUrl = null,
                        DurationSeconds = 600,
                        DisplayOrder = 1,
                        Content = "In this lesson, we'll cover...",
                        IsPublished = true,
                        IsPreview = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };

                    var newLesson2 = new Lesson
                    {
                        ModuleId = newModule1.Id,
                        Title = "Tuning Your Guitar",
                        Description = "Standard tuning and how to tune your instrument",
                        VideoUrl = null, // Admin must set a VdoCipher Video ID via admin panel
                        ThumbnailUrl = null,
                        DurationSeconds = 480,
                        DisplayOrder = 2,
                        Content = "Let's learn how to tune...",
                        IsPublished = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };

                    var newLesson3 = new Lesson
                    {
                        ModuleId = newModule2.Id,
                        Title = "G, C, and D Chords",
                        Description = "Learn your first three open chords",
                        VideoUrl = null, // Admin must set a VdoCipher Video ID via admin panel
                        ThumbnailUrl = null,
                        DurationSeconds = 1200,
                        DisplayOrder = 1,
                        Content = "Now let's tackle...",
                        IsPublished = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };

                    var newLesson4 = new Lesson
                    {
                        ModuleId = newModule2.Id,
                        Title = "Basic Strumming Patterns",
                        Description = "Get started with rhythmic playing",
                        VideoUrl = null, // Admin must set a VdoCipher Video ID via admin panel
                        ThumbnailUrl = null,
                        DurationSeconds = 900,
                        DisplayOrder = 2,
                        Content = "Time to add some rhythm...",
                        IsPublished = true,
                        CreatedAt = updateSeedDateTime,
                        UpdatedAt = updateSeedDateTime
                    };
                    context.Lessons.AddRange(newLesson1, newLesson2, newLesson3, newLesson4);
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Added lessons to modules.");
                }
            }

            if (context.AdminUsers.Any())
            {
                var existingHero = context.HeroContents.FirstOrDefault();
                if (existingHero != null && string.IsNullOrEmpty(existingHero.PreviewVideoId))
                {
                    existingHero.PreviewVideoId = "c2d8e1d68ba5449343adb7a66f4c6ed3";
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Updated existing HeroContent with default PreviewVideoId.");
                }

                // Clean up any lessons that still have the legacy local video path.
                // Admins must set a proper VdoCipher Video ID via the admin panel.
                var lessonsWithLocalPath = context.Lessons.Where(l => l.VideoUrl != null && l.VideoUrl.StartsWith("/videos/")).ToList();
                if (lessonsWithLocalPath.Any())
                {
                    foreach (var lesson in lessonsWithLocalPath)
                    {
                        lesson.VideoUrl = null;
                    }
                    context.SaveChanges();
                    Console.WriteLine("[DbInitializer] Cleared legacy local video paths from lessons. Admin must set VdoCipher IDs.");
                }

                Console.WriteLine("[DbInitializer] Database already seeded — skipping.");
                return; // DB has been seeded
            }

            Console.WriteLine("[DbInitializer] No users found — seeding database...");

        var seedDateTime = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1. Seed default user (password: Admin@123)
        var adminUser = new AdminUser
        {

            Email = "admin@acoustican.com",
            FullName = "Admin User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsActive = true,
            Role = "Admin",
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime,
            LastLoginAt = seedDateTime
        };
        context.AdminUsers.Add(adminUser);

        // 2. Seed Pricing Tiers with Features
        var starterTier = new PricingTier
        {
            Name = "Starter",
            Price = 19.99m,
            BillingPeriod = "monthly",
            Description = "Perfect for beginners",
            IsPopular = false,
            DisplayOrder = 1,
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };
        context.PricingTiers.Add(starterTier);
        context.SaveChanges(); // Save to generate starterTier ID

        var starterFeatures = new[]
        {
            new PricingFeature { PricingTierId = starterTier.Id, Feature = "Unlimited Access to 100+ Modules", IsIncluded = true, DisplayOrder = 1 },
            new PricingFeature { PricingTierId = starterTier.Id, Feature = "Interactive Practice Waveforms", IsIncluded = true, DisplayOrder = 2 },
            new PricingFeature { PricingTierId = starterTier.Id, Feature = "4K Cinematic Video Quality", IsIncluded = true, DisplayOrder = 3 }
        };
        context.PricingFeatures.AddRange(starterFeatures);

        var proPricingTier = new PricingTier
        {
            Name = "Annual Pass",
            Price = 7999.00m,
            BillingPeriod = "annually",
            Description = "Most popular choice",
            IsPopular = true,
            DisplayOrder = 2,
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };
        context.PricingTiers.Add(proPricingTier);
        context.SaveChanges(); // Save to generate proPricingTier ID

        var proFeatures = new[]
        {
            new PricingFeature { PricingTierId = proPricingTier.Id, Feature = "All Monthly Pass Features", IsIncluded = true, DisplayOrder = 1 },
            new PricingFeature { PricingTierId = proPricingTier.Id, Feature = "Access to Live Monthly Q&As", IsIncluded = true, DisplayOrder = 2 },
            new PricingFeature { PricingTierId = proPricingTier.Id, Feature = "Exclusive VIP Masterclasses", IsIncluded = true, DisplayOrder = 3 },
            new PricingFeature { PricingTierId = proPricingTier.Id, Feature = "Personalized 1-on-1 Feedback", IsIncluded = true, DisplayOrder = 4 }
        };
        context.PricingFeatures.AddRange(proFeatures);

        // 3. Seed Testimonials
        var testimonials = new List<Testimonial>
        {
            new Testimonial
            {
                StudentName = "John Smith",
                StudentRole = "Professional Musician",
                StudentImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop&crop=face&q=80",
                Content = "Acoustican has transformed my playing. The structured curriculum and high-quality videos make learning enjoyable and effective!",
                Rating = 5,
                IsPublished = true,
                DisplayOrder = 1,
                CreatedAt = seedDateTime,
                UpdatedAt = seedDateTime
            },
            new Testimonial
            {
                StudentName = "Sarah Jenkins",
                StudentRole = "Hobbyist Guitarist",
                StudentImageUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&h=100&fit=crop&crop=face&q=80",
                Content = "The cinematic quality of the lessons makes it feel like the instructor is right in the room. I've progressed more in 3 months here than in 2 years of YouTube tutorials.",
                Rating = 5,
                IsPublished = true,
                DisplayOrder = 2,
                CreatedAt = seedDateTime,
                UpdatedAt = seedDateTime
            },
            new Testimonial
            {
                StudentName = "Michael Chen",
                StudentRole = "Beginner",
                StudentImageUrl = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=100&h=100&fit=crop&crop=face&q=80",
                Content = "I always wanted to play guitar but found it overwhelming. Acoustican's beginner track broke everything down into manageable steps. Highly recommend!",
                Rating = 5,
                IsPublished = true,
                DisplayOrder = 3,
                CreatedAt = seedDateTime,
                UpdatedAt = seedDateTime
            },
            new Testimonial
            {
                StudentName = "Emily Rodriguez",
                StudentRole = "Classical Student",
                StudentImageUrl = "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=100&h=100&fit=crop&crop=face&q=80",
                Content = "The attention to detail in the fingerstyle modules is incredible. It's rare to find an online platform that respects classical technique while keeping it modern.",
                Rating = 5,
                IsPublished = true,
                DisplayOrder = 4,
                CreatedAt = seedDateTime,
                UpdatedAt = seedDateTime
            },
            new Testimonial
            {
                StudentName = "David Wilson",
                StudentRole = "Blues Enthusiast",
                StudentImageUrl = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=100&h=100&fit=crop&crop=face&q=80",
                Content = "I've been playing for years but was stuck in a rut. The blues soloing course gave me the fresh perspective I needed to start improvising with confidence.",
                Rating = 5,
                IsPublished = true,
                DisplayOrder = 5,
                CreatedAt = seedDateTime,
                UpdatedAt = seedDateTime
            }
        };
        context.Testimonials.AddRange(testimonials);

        // 4. Seed Hero Content
        var heroContent = new HeroContent
        {
            Title = "Learn Guitar From",
            Subtitle = "Beginner to Pro",
            Description = "Unlock your musical potential with high-fidelity lessons, cinematic video quality, and an interactive curriculum designed for the modern guitarist.",
            BackgroundVideoUrl = "/videos/5467-184226823_medium.mp4",
            PreviewVideoId = "c2d8e1d68ba5449343adb7a66f4c6ed3",
            PrimaryButtonText = "Start Learning",
            SecondaryButtonText = "Watch Preview",
            IsActive = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };
        context.HeroContents.Add(heroContent);

        // 5. Seed Courses
        var course1 = new Course
        {
            Title = "Guitar Basics Masterclass",
            Description = "Learn open chords, strumming patterns, and your first full song from scratch.",
            Level = "Beginner",
            Price = 49.99m,
            OriginalPrice = 149.99m,
            DurationMinutes = 765,
            LectureCount = 40,
            InstructorName = "Erich Andreas",
            StudentCount = 1200,
            ThumbnailUrl = "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&h=340&fit=crop&q=80",
            IsPublished = true,
            IsBestseller = true,
            Rating = 4.9m,
            ReviewCount = 24,
            WhatYoullLearn = "Master fundamental guitar techniques,Learn chord progressions and strumming patterns,Understand music theory for guitar,Develop your own playing style",
            Requirements = "A guitar (acoustic or electric),Basic understanding of music (not required),Passion for learning!",
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        var course2 = new Course
        {
            Title = "Fingerstyle Essentials",
            Description = "Master PIMA technique, Travis picking, and classical fingerstyle patterns.",
            Level = "Intermediate",
            Price = 59.99m,
            OriginalPrice = 179.99m,
            DurationMinutes = 560,
            LectureCount = 35,
            InstructorName = "Henry Olsen",
            StudentCount = 800,
            ThumbnailUrl = "https://images.unsplash.com/photo-1525201548942-d8732f6617a0?w=600&h=340&fit=crop&q=80",
            IsPublished = true,
            IsBestseller = true,
            Rating = 4.8m,
            ReviewCount = 18,
            WhatYoullLearn = "Master PIMA technique,Learn Travis picking,Understand classical fingerstyle patterns,Develop finger independence",
            Requirements = "A guitar (acoustic or electric),Basic understanding of music (not required),Passion for learning!",
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        var course3 = new Course
        {
            Title = "Blues Soloing Mastery",
            Description = "Learn blues scales, bending techniques, and improvisation secrets.",
            Level = "Advanced",
            Price = 79.99m,
            OriginalPrice = 229.99m,
            DurationMinutes = 680,
            LectureCount = 45,
            InstructorName = "Marco Cirillo",
            StudentCount = 600,
            ThumbnailUrl = "https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=600&h=340&fit=crop&q=80",
            IsPublished = true,
            IsBestseller = false,
            Rating = 4.7m,
            ReviewCount = 12,
            WhatYoullLearn = "Learn blues scales,Master bending techniques,Improvisation secrets,Develop vibrato control",
            Requirements = "A guitar (acoustic or electric),Basic understanding of music (not required),Passion for learning!",
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };
        context.Courses.AddRange(course1, course2, course3);
        context.SaveChanges(); // To generate course IDs

        // Seed modules for course 1
        var module1 = new CourseModule
        {
            CourseId = course1.Id,
            ModuleNumber = 1,
            Title = "Getting Started",
            Description = "Learn the basics of holding the guitar and tuning",
            DurationMinutes = 60,
            DisplayOrder = 1,
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        var module2 = new CourseModule
        {
            CourseId = course1.Id,
            ModuleNumber = 2,
            Title = "Open Chords",
            Description = "Master the essential open chords for beginners",
            DurationMinutes = 120,
            DisplayOrder = 2,
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };
        context.CourseModules.AddRange(module1, module2);
        context.SaveChanges(); // To generate module IDs

        // Seed lessons for module 1
        var lesson1 = new Lesson
        {
            ModuleId = module1.Id,
            Title = "How to Hold Your Guitar",
            Description = "Proper posture and holding technique",
            VideoUrl = "/videos/5467-184226823_medium.mp4",
            ThumbnailUrl = null,
            DurationSeconds = 600,
            DisplayOrder = 1,
            Content = "In this lesson, we'll cover...",
            IsPublished = true,
            IsPreview = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        var lesson2 = new Lesson
        {
            ModuleId = module1.Id,
            Title = "Tuning Your Guitar",
            Description = "Standard tuning and how to tune your instrument",
            VideoUrl = "/videos/5467-184226823_medium.mp4",
            ThumbnailUrl = null,
            DurationSeconds = 480,
            DisplayOrder = 2,
            Content = "Let's learn how to tune...",
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        // Seed lessons for module 2
        var lesson3 = new Lesson
        {
            ModuleId = module2.Id,
            Title = "G, C, and D Chords",
            Description = "Learn your first three open chords",
            VideoUrl = "/videos/5467-184226823_medium.mp4",
            ThumbnailUrl = null,
            DurationSeconds = 1200,
            DisplayOrder = 1,
            Content = "Now let's tackle...",
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        var lesson4 = new Lesson
        {
            ModuleId = module2.Id,
            Title = "Basic Strumming Patterns",
            Description = "Get started with rhythmic playing",
            VideoUrl = "/videos/5467-184226823_medium.mp4",
            ThumbnailUrl = null,
            DurationSeconds = 900,
            DisplayOrder = 2,
            Content = "Time to add some rhythm...",
            IsPublished = true,
            CreatedAt = seedDateTime,
            UpdatedAt = seedDateTime
        };

        context.Lessons.AddRange(lesson1, lesson2, lesson3, lesson4);
        context.SaveChanges();
            Console.WriteLine("[DbInitializer] Database seeded successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DbInitializer] ERROR seeding database: {ex.Message}");
            Console.WriteLine(ex.ToString());
            throw; // Re-throw so the app fails fast on seed errors
        }
    }
}
