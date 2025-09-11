using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAIService _aiService;

    public RecommendationService(ApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<IEnumerable<RecommendationDto>> GenerateRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var recommendations = new List<RecommendationDto>();
        
        var courseRecommendations = await GenerateCourseRecommendationsAsync(employeeId, cancellationToken);
        var mentorRecommendations = await GenerateMentorRecommendationsAsync(employeeId, cancellationToken);
        var projectRecommendations = await GenerateProjectRecommendationsAsync(employeeId, cancellationToken);
        
        recommendations.AddRange(courseRecommendations);
        recommendations.AddRange(mentorRecommendations);
        recommendations.AddRange(projectRecommendations);
        
        return recommendations.OrderByDescending(r => r.Priority)
                            .ThenByDescending(r => r.ConfidenceScore);
    }

    public async Task<RecommendationDto> AcceptRecommendationAsync(int recommendationId, CancellationToken cancellationToken = default)
    {
        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == recommendationId, cancellationToken);

        if (recommendation == null)
            throw new ArgumentException($"Recommendation with ID {recommendationId} not found");

        recommendation.IsAccepted = true;
        recommendation.AcceptedDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(recommendation);
    }

    public async Task<IEnumerable<RecommendationDto>> GetEmployeeRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var recommendations = await _context.Recommendations
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.ConfidenceScore)
            .ToListAsync(cancellationToken);

        return recommendations.Select(MapToDto);
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateCourseRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId, cancellationToken);
        var courses = await _context.Courses
            .OrderByDescending(c => c.Rating)
            .Take(3)
            .ToListAsync(cancellationToken);

        var recommendations = new List<RecommendationDto>();

        foreach (var course in courses)
        {
            var reasoning = await _aiService.GenerateRecommendationReasoningAsync(
                MapToEmployeeDto(employee), course, cancellationToken);
            
            var recommendation = new Recommendation
            {
                EmployeeId = employeeId,
                Type = RecommendationType.Course,
                Title = $"Complete: {course.Title}",
                Description = course.Description,
                Reasoning = reasoning,
                Priority = CalculateCoursePriority(course),
                ConfidenceScore = 0.85m,
                CreatedDate = DateTime.UtcNow,
                CourseId = course.Id
            };

            _context.Recommendations.Add(recommendation);
            recommendations.Add(MapToDto(recommendation));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return recommendations;
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateMentorRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId, cancellationToken);
        var mentors = await _context.Employees
            .Where(e => e.Id != employeeId && e.YearsOfExperience > employee.YearsOfExperience + 2)
            .Take(2)
            .ToListAsync(cancellationToken);

        var recommendations = new List<RecommendationDto>();

        foreach (var mentor in mentors)
        {
            var reasoning = await _aiService.GenerateMentorMatchReasoningAsync(
                MapToEmployeeDto(employee), MapToEmployeeDto(mentor), cancellationToken);
            
            var recommendation = new Recommendation
            {
                EmployeeId = employeeId,
                Type = RecommendationType.Mentor,
                Title = $"Connect with: {mentor.FullName}",
                Description = $"Senior {mentor.Position} in {mentor.Department}",
                Reasoning = reasoning,
                Priority = 4,
                ConfidenceScore = 0.78m,
                CreatedDate = DateTime.UtcNow,
                MentorEmployeeId = mentor.Id
            };

            _context.Recommendations.Add(recommendation);
            recommendations.Add(MapToDto(recommendation));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return recommendations;
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateProjectRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId, cancellationToken);
        var projects = await _context.Projects
            .Include(p => p.RequiredSkills)
            .Where(p => p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.Active)
            .Take(2)
            .ToListAsync(cancellationToken);

        var recommendations = new List<RecommendationDto>();

        foreach (var project in projects)
        {
            var reasoning = await _aiService.GenerateProjectMatchReasoningAsync(
                MapToEmployeeDto(employee), project, cancellationToken);
            
            var recommendation = new Recommendation
            {
                EmployeeId = employeeId,
                Type = RecommendationType.Project,
                Title = $"Join Project: {project.Name}",
                Description = project.Description,
                Reasoning = reasoning,
                Priority = 5,
                ConfidenceScore = 0.82m,
                CreatedDate = DateTime.UtcNow,
                ProjectId = project.Id
            };

            _context.Recommendations.Add(recommendation);
            recommendations.Add(MapToDto(recommendation));
        }

        await _context.SaveChangesAsync(cancellationToken);
        return recommendations;
    }

    private async Task<Employee> GetEmployeeWithSkillsAsync(int employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees
            .Include(e => e.Skills)
            .ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken) 
            ?? throw new ArgumentException($"Employee with ID {employeeId} not found");
    }

    private int CalculateCoursePriority(Course course)
    {
        if (course.Category.Contains("AI") || course.Category.Contains("Cloud")) return 5;
        if (course.Rating >= 4.5m) return 4;
        if (course.Rating >= 4.0m) return 3;
        return 2;
    }

    private RecommendationDto MapToDto(Recommendation recommendation)
    {
        return new RecommendationDto
        {
            Id = recommendation.Id,
            EmployeeId = recommendation.EmployeeId,
            Type = recommendation.Type,
            Title = recommendation.Title,
            Description = recommendation.Description,
            Reasoning = recommendation.Reasoning,
            Priority = recommendation.Priority,
            ConfidenceScore = recommendation.ConfidenceScore,
            CreatedDate = recommendation.CreatedDate,
            IsAccepted = recommendation.IsAccepted,
            AcceptedDate = recommendation.AcceptedDate,
            CourseId = recommendation.CourseId,
            MentorEmployeeId = recommendation.MentorEmployeeId,
            ProjectId = recommendation.ProjectId
        };
    }

    private EmployeeDto MapToEmployeeDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Position = employee.Position,
            Department = employee.Department,
            YearsOfExperience = employee.YearsOfExperience,
            Skills = employee.Skills.Select(s => new EmployeeSkillDto
            {
                Id = s.Id,
                SkillId = s.SkillId,
                Skill = new SkillDto
                {
                    Id = s.Skill.Id,
                    Name = s.Skill.Name,
                    Category = s.Skill.Category,
                    Description = s.Skill.Description
                },
                Level = s.Level,
                AcquiredDate = s.AcquiredDate
            }).ToList()
        };
    }
}