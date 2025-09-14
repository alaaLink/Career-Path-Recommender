using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class EmployeeSkillRepository : IEmployeeSkillRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeSkillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeSkill?> GetByIdAsync(int employeeId, int skillId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId, cancellationToken);
    }

    public async Task<IEnumerable<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeSkills
            .Where(es => es.EmployeeId == employeeId)
            .Include(es => es.Skill)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EmployeeSkill>> GetBySkillIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeSkills
            .Where(es => es.SkillId == skillId)
            .Include(es => es.Employee)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeSkill> AddAsync(EmployeeSkill employeeSkill, CancellationToken cancellationToken = default)
    {
        await _context.EmployeeSkills.AddAsync(employeeSkill, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return employeeSkill;
    }

    public async Task<EmployeeSkill> UpdateAsync(EmployeeSkill employeeSkill, CancellationToken cancellationToken = default)
    {
        _context.Entry(employeeSkill).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return employeeSkill;
    }

    public async Task DeleteAsync(int employeeId, int skillId, CancellationToken cancellationToken = default)
    {
        var employeeSkill = await GetByIdAsync(employeeId, skillId, cancellationToken);
        if (employeeSkill != null)
        {
            _context.EmployeeSkills.Remove(employeeSkill);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(int employeeId, int skillId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeSkills
            .AnyAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId, cancellationToken);
    }
}
