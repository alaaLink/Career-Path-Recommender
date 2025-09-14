using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetTopRatedAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetByProviderAsync(string provider, CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetFreeCoursesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Course>> GetEnrolledCoursesAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default);
    Task<Course> UpdateAsync(Course course, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}