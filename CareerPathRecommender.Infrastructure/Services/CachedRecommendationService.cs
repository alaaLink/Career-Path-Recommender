using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CareerPathRecommender.Infrastructure.Services;

public class CachedRecommendationService : IRecommendationService
{
    private readonly IRecommendationService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedRecommendationService> _logger;

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ShortCacheExpiration = TimeSpan.FromMinutes(5);

    public CachedRecommendationService(
        IRecommendationService innerService,
        IMemoryCache cache,
        ILogger<CachedRecommendationService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<RecommendationDto>> GenerateRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"recommendations_{employeeId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<RecommendationDto>? cachedRecommendations))
        {
            _logger.LogInformation("Cache hit for employee {EmployeeId} recommendations", employeeId);
            return cachedRecommendations!;
        }

        _logger.LogInformation("Cache miss for employee {EmployeeId} recommendations, generating new ones", employeeId);
        var recommendations = await _innerService.GenerateRecommendationsAsync(employeeId, cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiration,
            SlidingExpiration = ShortCacheExpiration,
            Priority = CacheItemPriority.Normal
        };

        _cache.Set(cacheKey, recommendations, cacheEntryOptions);
        return recommendations;
    }

    public async Task<RecommendationDto> AcceptRecommendationAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var result = await _innerService.AcceptRecommendationAsync(recommendationId, cancellationToken);

        // Invalidate cache for the employee when recommendation is accepted
        var cacheKey = $"recommendations_{result.EmployeeId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Invalidated cache for employee {EmployeeId} after accepting recommendation", result.EmployeeId);

        return result;
    }

    public async Task<IEnumerable<RecommendationDto>> GetEmployeeRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"employee_recommendations_{employeeId}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<RecommendationDto>? cachedRecommendations))
        {
            _logger.LogInformation("Cache hit for employee {EmployeeId} stored recommendations", employeeId);
            return cachedRecommendations!;
        }

        _logger.LogInformation("Cache miss for employee {EmployeeId} stored recommendations", employeeId);
        var recommendations = await _innerService.GetEmployeeRecommendationsAsync(employeeId, cancellationToken);

        var cacheEntryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ShortCacheExpiration,
            Priority = CacheItemPriority.Low
        };

        _cache.Set(cacheKey, recommendations, cacheEntryOptions);
        return recommendations;
    }

    public Task<SkillGapAnalysisDto> AnalyzeSkillGapsAsync(int employeeId, string targetPosition, CancellationToken cancellationToken = default)
    {
        // Skill gap analysis results are more dynamic and shouldn't be cached as aggressively
        return _innerService.AnalyzeSkillGapsAsync(employeeId, targetPosition, cancellationToken);
    }
}