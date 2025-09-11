using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.Interfaces;

public interface IRecommendationRepository
{
    Task<Recommendation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Recommendation>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recommendation>> GetByTypeAsync(RecommendationType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recommendation>> GetActiveRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Recommendation>> GetAcceptedRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default);
    Task<Recommendation> UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task MarkAsViewedAsync(int recommendationId, CancellationToken cancellationToken = default);
    Task MarkAsAcceptedAsync(int recommendationId, CancellationToken cancellationToken = default);
}