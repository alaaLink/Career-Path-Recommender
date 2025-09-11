using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Domain.Interfaces;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetPotentialMentorsAsync(int employeeId, CancellationToken cancellationToken = default);
}