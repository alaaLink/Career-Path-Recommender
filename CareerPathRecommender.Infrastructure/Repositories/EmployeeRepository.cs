using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Skills)
            .ThenInclude(es => es.Skill)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByIdWithCoursesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Courses)
            .ThenInclude(ec => ec.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => e.Department == department)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetMentorCandidatesAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await GetByIdAsync(employeeId, cancellationToken);
        if (employee == null) return Enumerable.Empty<Employee>();

        return await _context.Employees
            .Where(e => e.Id != employeeId && e.YearsOfExperience > employee.YearsOfExperience + 2)
            .OrderByDescending(e => e.YearsOfExperience)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Entry(employee).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await GetByIdAsync(id, cancellationToken);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees.CountAsync(cancellationToken);
    }

    public async Task<Skill?> GetSkillByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<Skill> CreateSkillAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);
        return skill;
    }

    public async Task<EmployeeSkill> AddEmployeeSkillAsync(EmployeeSkill employeeSkill, CancellationToken cancellationToken = default)
    {
        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync(cancellationToken);
        return employeeSkill;
    }

    public async Task RemoveEmployeeSkillAsync(int employeeId, int skillId, CancellationToken cancellationToken = default)
    {
        var employeeSkill = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == employeeId && es.SkillId == skillId, cancellationToken);

        if (employeeSkill != null)
        {
            _context.EmployeeSkills.Remove(employeeSkill);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}