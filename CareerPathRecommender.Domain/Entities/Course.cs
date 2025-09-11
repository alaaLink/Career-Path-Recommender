namespace CareerPathRecommender.Domain.Entities;

public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public decimal Rating { get; set; }
    public decimal Price { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}