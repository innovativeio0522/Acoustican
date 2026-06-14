using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager,Viewer")]
public class LessonsController(ILessonService lessonService) : ControllerBase
{
    [HttpGet("module/{moduleId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonsByModule(int moduleId)
    {
        var lessons = await lessonService.GetLessonsByModuleIdAsync(moduleId);
        return Ok(lessons);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessonById(int id)
    {
        var lesson = await lessonService.GetLessonByIdAsync(id);
        if (lesson == null) return NotFound(new { message = "Lesson not found" });
        return Ok(lesson);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonDto dto)
    {
        var lesson = await lessonService.CreateLessonAsync(dto);
        return CreatedAtAction(nameof(GetLessonById), new { id = lesson.Id }, lesson);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateLesson(int id, [FromBody] UpdateLessonDto dto)
    {
        var lesson = await lessonService.UpdateLessonAsync(id, dto);
        if (lesson == null) return NotFound(new { message = "Lesson not found" });
        return Ok(lesson);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteLesson(int id)
    {
        var result = await lessonService.DeleteLessonAsync(id);
        if (!result) return NotFound(new { message = "Lesson not found" });
        return Ok(new { message = "Lesson deleted successfully" });
    }
}
