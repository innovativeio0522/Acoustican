namespace Acoustican.Models;

public class HeroContent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? BackgroundVideoUrl { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? PreviewVideoId { get; set; }
    public string PrimaryButtonText { get; set; } = "Start Learning";
    public string SecondaryButtonText { get; set; } = "Watch Preview";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
