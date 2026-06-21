using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface ILessonService
{
    Task<List<LessonDto>> GetLessonsByModuleIdAsync(int moduleId);
    Task<LessonDto?> GetLessonByIdAsync(int id);
    Task<LessonDto> CreateLessonAsync(CreateLessonDto dto);
    Task<LessonDto?> UpdateLessonAsync(int id, UpdateLessonDto dto);
    Task<bool> DeleteLessonAsync(int id);
}

public class LessonService(ApplicationDbContext context, IMapper mapper) : ILessonService
{
    public async Task<List<LessonDto>> GetLessonsByModuleIdAsync(int moduleId)
    {
        var lessons = await context.Lessons
            .AsNoTracking()
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.DisplayOrder)
            .ToListAsync();
        return mapper.Map<List<LessonDto>>(lessons);
    }

    public async Task<LessonDto?> GetLessonByIdAsync(int id)
    {
        var lesson = await context.Lessons.FindAsync(id);
        return mapper.Map<LessonDto>(lesson);
    }

    public async Task<LessonDto> CreateLessonAsync(CreateLessonDto dto)
    {
        var lesson = mapper.Map<Lesson>(dto);
        context.Lessons.Add(lesson);
        await context.SaveChangesAsync();
        return mapper.Map<LessonDto>(lesson);
    }

    public async Task<LessonDto?> UpdateLessonAsync(int id, UpdateLessonDto dto)
    {
        var lesson = await context.Lessons.FindAsync(id);
        if (lesson == null) return null;

        mapper.Map(dto, lesson);
        lesson.UpdatedAt = DateTime.UtcNow;
        context.Lessons.Update(lesson);
        await context.SaveChangesAsync();
        return mapper.Map<LessonDto>(lesson);
    }

    public async Task<bool> DeleteLessonAsync(int id)
    {
        var lesson = await context.Lessons.FindAsync(id);
        if (lesson == null) return false;

        context.Lessons.Remove(lesson);
        await context.SaveChangesAsync();
        return true;
    }
}
