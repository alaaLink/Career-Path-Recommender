using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly ApplicationDbContext _context;

    public SkillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Skill>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Where(s => s.Category == category)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<Skill>> SearchSkillsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .Where(s => s.Name.Contains(searchTerm) || 
                       s.Category.Contains(searchTerm) || 
                       s.Description.Contains(searchTerm))
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Skills.AddAsync(skill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Skill> UpdateAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Entry(skill).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return skill;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var skill = await GetByIdAsync(id, cancellationToken);
        if (skill != null)
        {
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Skills.CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Skills.AnyAsync(s => s.Id == id, cancellationToken);
    }
}