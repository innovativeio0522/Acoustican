using Acoustican.DTOs;
using Acoustican.Models;

namespace Acoustican.ViewModels;

public class HomeViewModel
{
    public HeroContent? Hero { get; set; }
    public List<CourseDto> Courses { get; set; } = new();
    public List<TestimonialDto> Testimonials { get; set; } = new();
    public List<PricingTierDto> PricingTiers { get; set; } = new();
}
