using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.DTOs;

public class SkillGapAnalysisDto
{
    public IEnumerable<SkillGapDto> MissingSkills { get; set; } = new List<SkillGapDto>();
    public IEnumerable<SkillGapDto> SkillsToImprove { get; set; } = new List<SkillGapDto>();
    public string RecommendedLearningPath { get; set; } = string.Empty;
    public int EstimatedTimeToTargetMonths { get; set; }
}

public class SkillGapDto
{
    public string SkillName { get; set; } = string.Empty;
    public SkillLevel CurrentLevel { get; set; }
    public SkillLevel RequiredLevel { get; set; }
    public int Priority { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}