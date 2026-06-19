using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers.Mvc;

[Route("admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        ViewData["Title"] = "Admin Login";
        return View();
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        ViewData["ActivePage"] = "dashboard";
        return View();
    }

    [HttpGet("courses")]
    public IActionResult Courses()
    {
        ViewData["ActivePage"] = "courses";
        return View();
    }

    [HttpGet("modules")]
    public IActionResult Modules()
    {
        ViewData["ActivePage"] = "modules";
        return View();
    }

    [HttpGet("testimonials")]
    public IActionResult Testimonials()
    {
        ViewData["ActivePage"] = "testimonials";
        return View();
    }

    [HttpGet("pricing")]
    public IActionResult Pricing()
    {
        return RedirectToAction("Dashboard");
    }

    [HttpGet("hero")]
    public IActionResult Hero()
    {
        ViewData["ActivePage"] = "hero";
        return View();
    }

    [HttpGet("files")]
    public IActionResult Files()
    {
        ViewData["ActivePage"] = "files";
        return View();
    }

    [HttpGet("contact")]
    public IActionResult Contact()
    {
        ViewData["ActivePage"] = "contact";
        return View();
    }
}

