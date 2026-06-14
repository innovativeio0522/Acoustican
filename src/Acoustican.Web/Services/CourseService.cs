using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface ICourseService
{
    Task<List<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(int id);
    Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
    Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto dto);
    Task<bool> DeleteCourseAsync(int id);
    Task<bool> PublishCourseAsync(int id);
    Task<bool> UnpublishCourseAsync(int id);
}

public class CourseService(ApplicationDbContext context, IMapper mapper) : ICourseService
{
    public async Task<List<CourseDto>> GetAllCoursesAsync()
    {
        var courses = await context.Courses
            .Include(c => c.Modules.OrderBy(m => m.DisplayOrder).ThenBy(m => m.ModuleNumber))
                .ThenInclude(m => m.Lessons)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return mapper.Map<List<CourseDto>>(courses);
    }

    public async Task<CourseDto?> GetCourseByIdAsync(int id)
    {
        var course = await context.Courses
            .Include(c => c.Modules.OrderBy(m => m.DisplayOrder).ThenBy(m => m.ModuleNumber))
                .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == id);
        return mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
    {
        var course = mapper.Map<Course>(dto);
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return mapper.Map<CourseDto>(course);
    }

    public async Task<CourseDto?> UpdateCourseAsync(int id, UpdateCourseDto dto)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return null;

        mapper.Map(dto, course);
        course.UpdatedAt = DateTime.UtcNow;
        context.Courses.Update(course);
        await context.SaveChangesAsync();
        return mapper.Map<CourseDto>(course);
    }

    public async Task<bool> DeleteCourseAsync(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return false;

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishCourseAsync(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return false;

        course.IsPublished = true;
        course.UpdatedAt = DateTime.UtcNow;
        context.Courses.Update(course);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishCourseAsync(int id)
    {
        var course = await context.Courses.FindAsync(id);
        if (course == null) return false;

        course.IsPublished = false;
        course.UpdatedAt = DateTime.UtcNow;
        context.Courses.Update(course);
        await context.SaveChangesAsync();
        return true;
    }
}
