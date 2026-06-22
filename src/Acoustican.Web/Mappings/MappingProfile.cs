using Acoustican.DTOs;
using Acoustican.Models;
using Acoustican.Services;
using AutoMapper;

namespace Acoustican.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Course Mappings
        CreateMap<Course, CourseDto>().ReverseMap();
        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>();

        // Review Mappings
        CreateMap<CourseReview, CourseReviewDto>()
            .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));

        // Module Mappings
        CreateMap<CourseModule, CourseModuleDto>().ReverseMap();
        CreateMap<CreateCourseModuleDto, CourseModule>();
        CreateMap<UpdateCourseModuleDto, CourseModule>();

        // Lesson Mappings
        CreateMap<Lesson, LessonDto>().ReverseMap();
        CreateMap<CreateLessonDto, Lesson>();
        CreateMap<UpdateLessonDto, Lesson>();

        // Testimonial Mappings
        CreateMap<Testimonial, TestimonialDto>().ReverseMap();
        CreateMap<CreateTestimonialDto, Testimonial>();
        CreateMap<UpdateTestimonialDto, Testimonial>();

        // Pricing Mappings
        CreateMap<PricingTier, PricingTierDto>().ReverseMap();
        CreateMap<CreatePricingTierDto, PricingTier>();
        CreateMap<UpdatePricingTierDto, PricingTier>();
        CreateMap<PricingFeature, PricingFeatureDto>().ReverseMap();

        // Admin User Mappings
        CreateMap<AdminUser, AdminUserDto>().ReverseMap();

        // Hero Content Mappings
        CreateMap<HeroContent, HeroContent>().ReverseMap();
    }
}
