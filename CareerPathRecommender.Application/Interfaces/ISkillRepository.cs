using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Application.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<Skill?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Skill>> SearchSkillsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Skill> AddAsync(Skill skill, CancellationToken cancellationToken = default);
    Task<Skill> UpdateAsync(Skill skill, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}