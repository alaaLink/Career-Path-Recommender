using CareerPathRecommender.Application.DTOs;

namespace CareerPathRecommender.Application.Interfaces;

public interface IRecommendationService
{
    Task<IEnumerable<RecommendationDto>> GenerateRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<RecommendationDto> AcceptRecommendationAsync(int recommendationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RecommendationDto>> GetEmployeeRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<SkillGapAnalysisDto> AnalyzeSkillGapsAsync(int employeeId, string targetPosition, CancellationToken cancellationToken = default);
}