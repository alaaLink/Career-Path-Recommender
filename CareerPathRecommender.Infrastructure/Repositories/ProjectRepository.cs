using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Project?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Include(p => p.RequiredSkills)
            .ThenInclude(ps => ps.Skill)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == status)
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Department == department)
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == ProjectStatus.Active)
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Project>> GetAvailableProjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.Active)
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Projects.AddAsync(project, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _context.Entry(project).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var project = await GetByIdAsync(id, cancellationToken);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Projects.CountAsync(cancellationToken);
    }
}