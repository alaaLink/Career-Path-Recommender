using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdWithCoursesAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<IEnumerable<Employee>> GetMentorCandidatesAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}