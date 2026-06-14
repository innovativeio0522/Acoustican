using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager,Viewer")]
public class CoursesController(ICourseService courseService, IFileUploadService fileUploadService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await courseService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourseById(int id)
    {
        var course = await courseService.GetCourseByIdAsync(id);
        if (course == null)
            return NotFound(new { message = "Course not found" });
        return Ok(course);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var course = await courseService.CreateCourseAsync(dto);
        return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await courseService.UpdateCourseAsync(id, dto);
        if (course == null)
            return NotFound(new { message = "Course not found" });
        return Ok(course);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var result = await courseService.DeleteCourseAsync(id);
        if (!result)
            return NotFound(new { message = "Course not found" });
        return Ok(new { message = "Course deleted successfully" });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> PublishCourse(int id)
    {
        var result = await courseService.PublishCourseAsync(id);
        if (!result)
            return NotFound(new { message = "Course not found" });
        return Ok(new { message = "Course published successfully" });
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UnpublishCourse(int id)
    {
        var result = await courseService.UnpublishCourseAsync(id);
        if (!result)
            return NotFound(new { message = "Course not found" });
        return Ok(new { message = "Course unpublished successfully" });
    }

    [HttpPost("{id}/upload-thumbnail")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UploadThumbnail(int id, IFormFile file)
    {
        var response = await fileUploadService.UploadImageAsync(file, "courses");
        if (!response.Success)
            return BadRequest(response);

        // Update course with thumbnail URL
        var course = await courseService.GetCourseByIdAsync(id);
        if (course != null)
        {
            var updateDto = new UpdateCourseDto
            {
                Title = course.Title,
                Description = course.Description,
                Level = course.Level,
                Price = course.Price,
                DurationMinutes = course.DurationMinutes,
                ThumbnailUrl = response.FilePath,
                StudentCount = course.StudentCount,
                Rating = course.Rating,
                ReviewCount = course.ReviewCount,
                IsPublished = course.IsPublished
            };
            await courseService.UpdateCourseAsync(id, updateDto);
        }

        return Ok(response);
    }
}
