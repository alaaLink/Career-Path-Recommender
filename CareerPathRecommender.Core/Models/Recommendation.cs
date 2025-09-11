using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Core.Models;

public class Recommendation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty; // AI-generated explanation
    public int Priority { get; set; } // 1-5
    public decimal ConfidenceScore { get; set; } // 0-1
    
    public DateTime CreatedDate { get; set; }
    public bool IsViewed { get; set; }
    public bool IsAccepted { get; set; }
    
    // Course recommendation specific
    public int? CourseId { get; set; }
    
    // Mentor recommendation specific
    public int? MentorEmployeeId { get; set; }
    
    // Project recommendation specific
    public int? ProjectId { get; set; }
    
    public virtual Employee Employee { get; set; } = null!;
    public virtual Course? Course { get; set; }
    public virtual Employee? MentorEmployee { get; set; }
    public virtual Project? Project { get; set; }
}

public enum RecommendationType
{
    Course,
    Mentor,
    Project,
    Certification,
    SkillDevelopment
}