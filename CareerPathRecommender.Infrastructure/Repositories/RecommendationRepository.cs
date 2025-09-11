using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly ApplicationDbContext _context;

    public RecommendationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Recommendation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Include(r => r.Employee)
            .Include(r => r.Course)
            .Include(r => r.Project)
            .Include(r => r.MentorEmployee)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Include(r => r.Employee)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Where(r => r.EmployeeId == employeeId)
            .Include(r => r.Course)
            .Include(r => r.Project)
            .Include(r => r.MentorEmployee)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.ConfidenceScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetByTypeAsync(RecommendationType type, CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Where(r => r.Type == type)
            .Include(r => r.Employee)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetActiveRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Where(r => r.EmployeeId == employeeId && !r.IsAccepted)
            .Include(r => r.Course)
            .Include(r => r.Project)
            .Include(r => r.MentorEmployee)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.ConfidenceScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetAcceptedRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Recommendations
            .Where(r => r.EmployeeId == employeeId && r.IsAccepted)
            .Include(r => r.Course)
            .Include(r => r.Project)
            .Include(r => r.MentorEmployee)
            .OrderByDescending(r => r.AcceptedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recommendation> AddAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Recommendations.AddAsync(recommendation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Recommendation> UpdateAsync(Recommendation recommendation, CancellationToken cancellationToken = default)
    {
        _context.Entry(recommendation).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return recommendation;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        
        if (recommendation != null)
        {
            _context.Recommendations.Remove(recommendation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsViewedAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);
        
        if (recommendation != null)
        {
            recommendation.IsViewed = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAsAcceptedAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);
        
        if (recommendation != null)
        {
            recommendation.IsAccepted = true;
            recommendation.AcceptedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}