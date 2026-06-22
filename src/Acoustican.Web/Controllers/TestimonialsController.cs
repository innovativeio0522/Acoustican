using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager,Viewer,User")]
public class TestimonialsController(ITestimonialService testimonialService, IFileUploadService fileUploadService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTestimonials()
    {
        var testimonials = await testimonialService.GetAllTestimonialsAsync();
        return Ok(testimonials);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublishedTestimonials()
    {
        var testimonials = await testimonialService.GetPublishedTestimonialsAsync();
        return Ok(testimonials);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTestimonialById(int id)
    {
        var testimonial = await testimonialService.GetTestimonialByIdAsync(id);
        if (testimonial == null)
            return NotFound(new { message = "Testimonial not found" });
        return Ok(testimonial);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ContentManager,User")]
    public async Task<IActionResult> CreateTestimonial([FromBody] CreateTestimonialDto dto)
    {
        if (User.IsInRole("User"))
        {
            dto.IsPublished = false;
        }
        var testimonial = await testimonialService.CreateTestimonialAsync(dto);
        return CreatedAtAction(nameof(GetTestimonialById), new { id = testimonial.Id }, testimonial);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateTestimonial(int id, [FromBody] UpdateTestimonialDto dto)
    {
        var testimonial = await testimonialService.UpdateTestimonialAsync(id, dto);
        if (testimonial == null)
            return NotFound(new { message = "Testimonial not found" });
        return Ok(testimonial);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteTestimonial(int id)
    {
        var result = await testimonialService.DeleteTestimonialAsync(id);
        if (!result)
            return NotFound(new { message = "Testimonial not found" });
        return Ok(new { message = "Testimonial deleted successfully" });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> PublishTestimonial(int id)
    {
        var result = await testimonialService.PublishTestimonialAsync(id);
        if (!result)
            return NotFound(new { message = "Testimonial not found" });
        return Ok(new { message = "Testimonial published successfully" });
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UnpublishTestimonial(int id)
    {
        var result = await testimonialService.UnpublishTestimonialAsync(id);
        if (!result)
            return NotFound(new { message = "Testimonial not found" });
        return Ok(new { message = "Testimonial unpublished successfully" });
    }

    [HttpPost("{id}/upload-image")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UploadImage(int id, IFormFile file)
    {
        var response = await fileUploadService.UploadImageAsync(file, "testimonials");
        if (!response.Success)
            return BadRequest(response);
        return Ok(response);
    }
}
