using Acoustican.DTOs;
using Acoustican.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Acoustican.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,ContentManager,Viewer")]
public class CourseModulesController(IModuleService moduleService) : ControllerBase
{
    [HttpGet("course/{courseId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModulesByCourse(int courseId)
    {
        var modules = await moduleService.GetModulesByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPublishedModules()
    {
        var modules = await moduleService.GetAllPublishedModulesAsync();
        return Ok(modules);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModuleById(int id)
    {
        var module = await moduleService.GetModuleByIdAsync(id);
        if (module == null)
            return NotFound(new { message = "Module not found" });
        return Ok(module);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> CreateModule([FromBody] CreateCourseModuleDto dto)
    {
        var module = await moduleService.CreateModuleAsync(dto);
        return CreatedAtAction(nameof(GetModuleById), new { id = module.Id }, module);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UpdateModule(int id, [FromBody] UpdateCourseModuleDto dto)
    {
        var module = await moduleService.UpdateModuleAsync(id, dto);
        if (module == null)
            return NotFound(new { message = "Module not found" });
        return Ok(module);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var result = await moduleService.DeleteModuleAsync(id);
        if (!result)
            return NotFound(new { message = "Module not found" });
        return Ok(new { message = "Module deleted successfully" });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> PublishModule(int id)
    {
        var module = await moduleService.GetModuleByIdAsync(id);
        if (module == null)
            return NotFound(new { message = "Module not found" });

        var updateDto = new UpdateCourseModuleDto
        {
            ModuleNumber = module.ModuleNumber,
            Title = module.Title,
            Description = module.Description,
            DurationMinutes = module.DurationMinutes,
            DisplayOrder = module.DisplayOrder,
            IsPublished = true
        };

        await moduleService.UpdateModuleAsync(id, updateDto);
        return Ok(new { message = "Module published successfully" });
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,ContentManager")]
    public async Task<IActionResult> UnpublishModule(int id)
    {
        var module = await moduleService.GetModuleByIdAsync(id);
        if (module == null)
            return NotFound(new { message = "Module not found" });

        var updateDto = new UpdateCourseModuleDto
        {
            ModuleNumber = module.ModuleNumber,
            Title = module.Title,
            Description = module.Description,
            DurationMinutes = module.DurationMinutes,
            DisplayOrder = module.DisplayOrder,
            IsPublished = false
        };

        await moduleService.UpdateModuleAsync(id, updateDto);
        return Ok(new { message = "Module unpublished successfully" });
    }
}
