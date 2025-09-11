using CareerPathRecommender.Core.Models;

namespace CareerPathRecommender.Core.Services;

public interface IRecommendationService
{
    Task<IEnumerable<Recommendation>> GenerateRecommendationsAsync(int employeeId);
    Task<IEnumerable<Course>> RecommendCoursesAsync(int employeeId);
    Task<IEnumerable<Employee>> RecommendMentorsAsync(int employeeId);
    Task<IEnumerable<Project>> RecommendProjectsAsync(int employeeId);
    Task<SkillGapAnalysis> AnalyzeSkillGapsAsync(int employeeId, string targetPosition);
}

public class SkillGapAnalysis
{
    public IEnumerable<SkillGap> MissingSkills { get; set; } = new List<SkillGap>();
    public IEnumerable<SkillGap> SkillsToImprove { get; set; } = new List<SkillGap>();
    public string RecommendedLearningPath { get; set; } = string.Empty;
    public int EstimatedTimeToTargetMonths { get; set; }
}

public class SkillGap
{
    public string SkillName { get; set; } = string.Empty;
    public SkillLevel CurrentLevel { get; set; }
    public SkillLevel RequiredLevel { get; set; }
    public int Priority { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}