using CareerPathRecommender.Core.Models;

namespace CareerPathRecommender.Core.Services;

public interface IAzureOpenAIService
{
    Task<string> GenerateRecommendationReasoningAsync(Employee employee, Course course);
    Task<string> GenerateMentorMatchReasoningAsync(Employee employee, Employee mentor);
    Task<string> GenerateProjectMatchReasoningAsync(Employee employee, Project project);
    Task<SkillGapAnalysis> AnalyzeCareerPathAsync(Employee employee, string targetPosition);
    Task<string> GenerateLearningPathAsync(Employee employee, IEnumerable<SkillGap> skillGaps);
}