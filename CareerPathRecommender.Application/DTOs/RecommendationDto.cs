using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.DTOs;

public record RecommendationDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public RecommendationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Reasoning { get; init; } = string.Empty;
    public int Priority { get; init; }
    public decimal ConfidenceScore { get; init; }
    public DateTime CreatedDate { get; init; }
    public bool IsAccepted { get; init; }
    public DateTime? AcceptedDate { get; init; }
    public int? CourseId { get; init; }
    public int? MentorEmployeeId { get; init; }
    public int? ProjectId { get; init; }
}