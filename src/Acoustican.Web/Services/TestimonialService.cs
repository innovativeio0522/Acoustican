using Acoustican.Data;
using Acoustican.DTOs;
using Acoustican.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Acoustican.Services;

public interface ITestimonialService
{
    Task<List<TestimonialDto>> GetAllTestimonialsAsync();
    Task<List<TestimonialDto>> GetPublishedTestimonialsAsync();
    Task<TestimonialDto?> GetTestimonialByIdAsync(int id);
    Task<TestimonialDto> CreateTestimonialAsync(CreateTestimonialDto dto);
    Task<TestimonialDto?> UpdateTestimonialAsync(int id, UpdateTestimonialDto dto);
    Task<bool> DeleteTestimonialAsync(int id);
    Task<bool> PublishTestimonialAsync(int id);
    Task<bool> UnpublishTestimonialAsync(int id);
}

public class TestimonialService(ApplicationDbContext context, IMapper mapper, ILogger<TestimonialService> logger) : ITestimonialService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<TestimonialService> _logger = logger;

    public async Task<List<TestimonialDto>> GetAllTestimonialsAsync()
    {
        var testimonials = await _context.Testimonials
            .AsNoTracking()
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
        return _mapper.Map<List<TestimonialDto>>(testimonials);
    }

    public async Task<List<TestimonialDto>> GetPublishedTestimonialsAsync()
    {
        var testimonials = await _context.Testimonials
            .AsNoTracking()
            .Where(t => t.IsPublished)
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
        return _mapper.Map<List<TestimonialDto>>(testimonials);
    }

    public async Task<TestimonialDto?> GetTestimonialByIdAsync(int id)
    {
        var testimonial = await _context.Testimonials
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
        return _mapper.Map<TestimonialDto>(testimonial);
    }

    public async Task<TestimonialDto> CreateTestimonialAsync(CreateTestimonialDto dto)
    {
        var testimonial = _mapper.Map<Testimonial>(dto);
        _context.Testimonials.Add(testimonial);
        await _context.SaveChangesAsync();
        return _mapper.Map<TestimonialDto>(testimonial);
    }

    public async Task<TestimonialDto?> UpdateTestimonialAsync(int id, UpdateTestimonialDto dto)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return null;

        _mapper.Map(dto, testimonial);
        testimonial.UpdatedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return _mapper.Map<TestimonialDto>(testimonial);
    }

    public async Task<bool> DeleteTestimonialAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return false;

        _context.Testimonials.Remove(testimonial);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PublishTestimonialAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return false;

        testimonial.IsPublished = true;
        testimonial.UpdatedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnpublishTestimonialAsync(int id)
    {
        var testimonial = await _context.Testimonials.FindAsync(id);
        if (testimonial == null) return false;

        testimonial.IsPublished = false;
        testimonial.UpdatedAt = DateTime.UtcNow;
        _context.Testimonials.Update(testimonial);
        await _context.SaveChangesAsync();
        return true;
    }
}
