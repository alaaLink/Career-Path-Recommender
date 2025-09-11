using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdWithSkillsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetActiveProjectsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Project>> GetAvailableProjectsAsync(CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}