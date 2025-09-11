using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .OrderBy(c => c.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Where(c => c.Category == category)
            .OrderByDescending(c => c.Rating)
            .ThenBy(c => c.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetTopRatedAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .OrderByDescending(c => c.Rating)
            .ThenBy(c => c.Price)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetByProviderAsync(string provider, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Where(c => c.Provider == provider)
            .OrderByDescending(c => c.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Course>> GetFreeCoursesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Where(c => c.Price == 0)
            .OrderByDescending(c => c.Rating)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course> AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        var entry = await _context.Courses.AddAsync(course, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entry.Entity;
    }

    public async Task<Course> UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        _context.Entry(course).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return course;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await GetByIdAsync(id, cancellationToken);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses.CountAsync(cancellationToken);
    }
}