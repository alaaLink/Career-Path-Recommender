using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.DTOs;

public class SkillGapAnalysisDto
{
    public IEnumerable<SkillGapDto> MissingSkills { get; set; } = new List<SkillGapDto>();
    public IEnumerable<SkillGapDto> SkillsToImprove { get; set; } = new List<SkillGapDto>();
    public string RecommendedLearningPath { get; set; } = string.Empty;
    public int EstimatedTimeToTargetMonths { get; set; }

    // Additional dynamic properties
    public string TargetPosition { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string CurrentPosition { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public decimal OverallReadiness { get; set; }
    public int TotalSkillsRequired { get; set; }
    public int SkillsMet { get; set; }
    public int HighPriorityGaps { get; set; }
    public List<string> NextActionItems { get; set; } = new List<string>();
    public List<CareerMilestoneDto> MilestoneTimeline { get; set; } = new List<CareerMilestoneDto>();
}

public class SkillGapDto
{
    public string SkillName { get; set; } = string.Empty;
    public SkillLevel CurrentLevel { get; set; }
    public SkillLevel RequiredLevel { get; set; }
    public int Priority { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int EstimatedLearningTimeMonths { get; set; }
    public List<string> RecommendedResources { get; set; } = new List<string>();
    public decimal ImportanceScore { get; set; }
}

public class CareerMilestoneDto
{
    public int Month { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SkillsToComplete { get; set; } = new List<string>();
    public bool IsCompleted { get; set; }
}