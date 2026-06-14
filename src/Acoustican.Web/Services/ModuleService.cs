using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface IModuleService
{
    Task<List<CourseModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<List<CourseModuleDto>> GetAllPublishedModulesAsync();
    Task<CourseModuleDto?> GetModuleByIdAsync(int id);
    Task<CourseModuleDto> CreateModuleAsync(CreateCourseModuleDto dto);
    Task<CourseModuleDto?> UpdateModuleAsync(int id, UpdateCourseModuleDto dto);
    Task<bool> DeleteModuleAsync(int id);
}

public class ModuleService(ApplicationDbContext context, IMapper mapper) : IModuleService
{
    public async Task<List<CourseModuleDto>> GetModulesByCourseIdAsync(int courseId)
    {
        var modules = await context.CourseModules
            .Include(m => m.Lessons.OrderBy(l => l.DisplayOrder))
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.ModuleNumber)
            .ToListAsync();
        return mapper.Map<List<CourseModuleDto>>(modules);
    }

    public async Task<List<CourseModuleDto>> GetAllPublishedModulesAsync()
    {
        var modules = await context.CourseModules
            .Include(m => m.Lessons.OrderBy(l => l.DisplayOrder))
            .Where(m => m.IsPublished)
            .OrderBy(m => m.DisplayOrder)
            .ThenBy(m => m.ModuleNumber)
            .ToListAsync();
        return mapper.Map<List<CourseModuleDto>>(modules);
    }

    public async Task<CourseModuleDto?> GetModuleByIdAsync(int id)
    {
        var module = await context.CourseModules
            .Include(m => m.Lessons.OrderBy(l => l.DisplayOrder))
            .FirstOrDefaultAsync(m => m.Id == id);
        return mapper.Map<CourseModuleDto>(module);
    }

    public async Task<CourseModuleDto> CreateModuleAsync(CreateCourseModuleDto dto)
    {
        var module = mapper.Map<CourseModule>(dto);
        context.CourseModules.Add(module);
        await context.SaveChangesAsync();
        return mapper.Map<CourseModuleDto>(module);
    }

    public async Task<CourseModuleDto?> UpdateModuleAsync(int id, UpdateCourseModuleDto dto)
    {
        var module = await context.CourseModules.FindAsync(id);
        if (module == null) return null;

        mapper.Map(dto, module);
        module.UpdatedAt = DateTime.UtcNow;
        context.CourseModules.Update(module);
        await context.SaveChangesAsync();
        return mapper.Map<CourseModuleDto>(module);
    }

    public async Task<bool> DeleteModuleAsync(int id)
    {
        var module = await context.CourseModules.FindAsync(id);
        if (module == null) return false;

        context.CourseModules.Remove(module);
        await context.SaveChangesAsync();
        return true;
    }
}
