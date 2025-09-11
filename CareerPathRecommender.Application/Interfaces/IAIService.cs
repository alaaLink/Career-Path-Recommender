using CareerPathRecommender.Application.DTOs;

namespace CareerPathRecommender.Application.Interfaces;

public interface IAIService
{
    Task<string> GenerateRecommendationReasoningAsync(EmployeeDto employee, object item, CancellationToken cancellationToken = default);
    Task<string> GenerateMentorMatchReasoningAsync(EmployeeDto employee, EmployeeDto mentor, CancellationToken cancellationToken = default);
    Task<string> GenerateProjectMatchReasoningAsync(EmployeeDto employee, object project, CancellationToken cancellationToken = default);
}