using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Application.Interfaces;

public interface IEmployeeSkillRepository
{
    Task<EmployeeSkill?> GetByIdAsync(int employeeId, int skillId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeSkill>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmployeeSkill>> GetBySkillIdAsync(int skillId, CancellationToken cancellationToken = default);
    Task<EmployeeSkill> AddAsync(EmployeeSkill employeeSkill, CancellationToken cancellationToken = default);
    Task<EmployeeSkill> UpdateAsync(EmployeeSkill employeeSkill, CancellationToken cancellationToken = default);
    Task DeleteAsync(int employeeId, int skillId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int employeeId, int skillId, CancellationToken cancellationToken = default);
}
