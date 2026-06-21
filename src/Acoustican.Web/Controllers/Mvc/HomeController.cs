using Acoustican.Data;
using Acoustican.Models;
using Acoustican.Services;
using Acoustican.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Controllers.Mvc;

public class HomeController(
    ICourseService courseService,
    IPricingService pricingService,
    ITestimonialService testimonialService,
    ApplicationDbContext context,
    ILogger<HomeController> logger) : Controller
{
    private readonly ICourseService _courseService = courseService;
    private readonly IPricingService _pricingService = pricingService;
    private readonly ITestimonialService _testimonialService = testimonialService;
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<HomeController> _logger = logger;



    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var hero = await _context.HeroContents
                .Where(h => h.IsActive)
                .OrderByDescending(h => h.UpdatedAt)
                .FirstOrDefaultAsync();

            var courses = await _courseService.GetAllCoursesAsync();
            var publishedCourses = courses.Where(c => c.IsPublished).ToList();

            var testimonials = await _testimonialService.GetPublishedTestimonialsAsync();
            var pricingTiers = await _pricingService.GetPublishedTiersAsync();

            var viewModel = new HomeViewModel
            {
                Hero = hero,
                Courses = publishedCourses,
                Testimonials = testimonials,
                PricingTiers = pricingTiers
            };

            ViewData["ActivePage"] = "Home";
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering home page");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("courses")]
    public async Task<IActionResult> Courses()
    {
        try
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var publishedCourses = courses.Where(c => c.IsPublished).ToList();

            ViewData["ActivePage"] = "Courses";
            return View(publishedCourses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering courses page");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword()
    {
        ViewData["ActivePage"] = "ForgotPassword";
        return View();
    }

    [HttpGet("reset-password")]
    public IActionResult ResetPassword(string token)
    {
        ViewData["ActivePage"] = "ResetPassword";
        ViewData["Token"] = token;
        return View();
    }

    [HttpGet("courses/{id}")]
    public async Task<IActionResult> CourseDetail(int id)
    {
        try
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewData["ActivePage"] = "Courses";
            return View(course);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering course detail page");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("cart")]
    public IActionResult Cart()
    {
        ViewData["ActivePage"] = "Cart";
        return View();
    }

    [HttpGet("orders")]
    public IActionResult Orders()
    {
        ViewData["ActivePage"] = "Orders";
        return View();
    }

    [HttpGet("debug-refresh-modules")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DebugRefreshModules()
    {
        try
        {
            var course1 = await _context.Courses.FirstOrDefaultAsync(c => c.Title == "Guitar Basics Masterclass");
            if (course1 == null) return NotFound("Course 1 not found");
            
            // Delete existing modules and lessons for course 1
            var existingModules = await _context.CourseModules.Where(m => m.CourseId == course1.Id).ToListAsync();
            foreach (var module in existingModules)
            {
                var moduleLessons = await _context.Lessons.Where(l => l.ModuleId == module.Id).ToListAsync();
                _context.Lessons.RemoveRange(moduleLessons);
            }
            _context.CourseModules.RemoveRange(existingModules);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleared old modules/lessons");
            
            var updateSeedDateTime = DateTime.UtcNow;
            
            // Add fresh modules
            var newModule1 = new CourseModule
            {
                CourseId = course1.Id,
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
                CourseId = course1.Id,
                ModuleNumber = 2,
                Title = "Open Chords",
                Description = "Master the essential open chords for beginners",
                DurationMinutes = 120,
                DisplayOrder = 2,
                IsPublished = true,
                CreatedAt = updateSeedDateTime,
                UpdatedAt = updateSeedDateTime
            };
            _context.CourseModules.AddRange(newModule1, newModule2);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added fresh modules");
            
            // Add fresh lessons
            var newLesson1 = new Lesson { ModuleId = newModule1.Id, Title = "How to Hold Your Guitar", Description = "Proper posture and holding technique", VideoUrl = null, ThumbnailUrl = null, DurationSeconds = 600, DisplayOrder = 1, Content = "In this lesson, we'll cover...", IsPublished = true, CreatedAt = updateSeedDateTime, UpdatedAt = updateSeedDateTime };
            var newLesson2 = new Lesson { ModuleId = newModule1.Id, Title = "Tuning Your Guitar", Description = "Standard tuning and how to tune your instrument", VideoUrl = null, ThumbnailUrl = null, DurationSeconds = 480, DisplayOrder = 2, Content = "Let's learn how to tune...", IsPublished = true, CreatedAt = updateSeedDateTime, UpdatedAt = updateSeedDateTime };
            var newLesson3 = new Lesson { ModuleId = newModule2.Id, Title = "G, C, and D Chords", Description = "Learn your first three open chords", VideoUrl = null, ThumbnailUrl = null, DurationSeconds = 1200, DisplayOrder = 1, Content = "Now let's tackle...", IsPublished = true, CreatedAt = updateSeedDateTime, UpdatedAt = updateSeedDateTime };
            var newLesson4 = new Lesson { ModuleId = newModule2.Id, Title = "Basic Strumming Patterns", Description = "Get started with rhythmic playing", VideoUrl = null, ThumbnailUrl = null, DurationSeconds = 900, DisplayOrder = 2, Content = "Time to add some rhythm...", IsPublished = true, CreatedAt = updateSeedDateTime, UpdatedAt = updateSeedDateTime };
            _context.Lessons.AddRange(newLesson1, newLesson2, newLesson3, newLesson4);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Added fresh lessons");
            
            return Redirect("/courses/1");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DebugRefreshModules");
            return StatusCode(500, ex.Message);
        }
    }
}
