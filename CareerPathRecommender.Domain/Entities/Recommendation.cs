using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Domain.Entities;

public class Recommendation : BaseEntity
{
    public int EmployeeId { get; set; }
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public int Priority { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedDate { get; set; }

    // Optional foreign keys
    public int? CourseId { get; set; }
    public int? MentorEmployeeId { get; set; }
    public int? ProjectId { get; set; }

    public virtual Employee Employee { get; set; } = null!;
    public virtual Course? Course { get; set; }
    public virtual Employee? MentorEmployee { get; set; }
    public virtual Project? Project { get; set; }

}