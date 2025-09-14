using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IAIService _aiService;

    public RecommendationService(
        IEmployeeRepository employeeRepository,
        ICourseRepository courseRepository,
        IProjectRepository projectRepository,
        IRecommendationRepository recommendationRepository,
        IAIService aiService,
        ISkillRepository skillRepository)
    {
        _employeeRepository = employeeRepository;
        _courseRepository = courseRepository;
        _projectRepository = projectRepository;
        _recommendationRepository = recommendationRepository;
        _aiService = aiService;
        _skillRepository = skillRepository;
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
        var recommendation = await _recommendationRepository.GetByIdAsync(recommendationId, cancellationToken);

        if (recommendation == null)
            throw new ArgumentException($"Recommendation with ID {recommendationId} not found");

        await _recommendationRepository.MarkAsAcceptedAsync(recommendationId, cancellationToken);

        return MapToDto(recommendation);
    }

    public async Task<IEnumerable<RecommendationDto>> GetEmployeeRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendationRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return recommendations.Select(MapToDto);
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateCourseRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
        if (employee == null) 
            throw new ArgumentException($"Employee with ID {employeeId} not found");

        var courses = await _courseRepository.GetTopRatedAsync(3, cancellationToken);
        var recommendations = new List<RecommendationDto>();

        foreach (var course in courses)
        {
            var courseDto = MapToCourseDto(course);
            var reasoning = await _aiService.GenerateRecommendationReasoningAsync(
                MapToEmployeeDto(employee), courseDto, cancellationToken);
            
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

            var savedRecommendation = await _recommendationRepository.AddAsync(recommendation, cancellationToken);
            recommendations.Add(MapToDto(savedRecommendation));
        }

        return recommendations;
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateMentorRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
        if (employee == null) 
            throw new ArgumentException($"Employee with ID {employeeId} not found");

        var mentors = await _employeeRepository.GetMentorCandidatesAsync(employeeId, cancellationToken);
        var recommendations = new List<RecommendationDto>();

        foreach (var mentor in mentors.Take(2))
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

            var savedRecommendation = await _recommendationRepository.AddAsync(recommendation, cancellationToken);
            recommendations.Add(MapToDto(savedRecommendation));
        }

        return recommendations;
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateProjectRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
        if (employee == null) 
            throw new ArgumentException($"Employee with ID {employeeId} not found");

        var projects = await _projectRepository.GetAvailableProjectsAsync(cancellationToken);
        var recommendations = new List<RecommendationDto>();

        foreach (var project in projects.Take(2))
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

            var savedRecommendation = await _recommendationRepository.AddAsync(recommendation, cancellationToken);
            recommendations.Add(MapToDto(savedRecommendation));
        }

        return recommendations;
    }

    public async Task<SkillGapAnalysisDto> AnalyzeSkillGapsAsync(int employeeId, string targetPosition, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
        if (employee == null) 
            throw new ArgumentException($"Employee with ID {employeeId} not found");
        

        //var skills = await _skillRepository.GetAllAsync(cancellationToken);
        
        // Mock implementation - in real world this would analyze skills vs target position
        var missingSkills = new List<SkillGapDto>
        {
            new() { 
                SkillName = "Leadership", 
                CurrentLevel = SkillLevel.Beginner, 
                RequiredLevel = SkillLevel.Advanced, 
                Priority = 5,
                Reasoning = "Leadership skills are critical for senior positions"
            },
            new() { 
                SkillName = "Cloud Architecture", 
                CurrentLevel = SkillLevel.Beginner, 
                RequiredLevel = SkillLevel.Expert, 
                Priority = 4,
                Reasoning = "Cloud expertise is essential for modern development"
            }
        };

        var skillsToImprove = new List<SkillGapDto>
        {
            new() { 
                SkillName = "C#", 
                CurrentLevel = SkillLevel.Intermediate, 
                RequiredLevel = SkillLevel.Advanced, 
                Priority = 3,
                Reasoning = "Advanced C# knowledge needed for complex projects"
            }
        };

        return new SkillGapAnalysisDto
        {
            MissingSkills = missingSkills,
            SkillsToImprove = skillsToImprove,
            RecommendedLearningPath = "Focus on leadership development and cloud certifications",
            EstimatedTimeToTargetMonths = 12
        };
    }


    private int CalculateCoursePriority(Course course)
    {
        if (course.Category.Contains("AI") || course.Category.Contains("Cloud")) return 5;
        if (course.Rating >= 4.5m) return 4;
        if (course.Rating >= 4.0m) return 3;
        return 2;
    }

    private CourseDto MapToCourseDto(Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Provider = course.Provider,
            Category = course.Category,
            DurationHours = course.DurationHours,
            Rating = course.Rating,
            Price = course.Price,
            Url = course.Url,
            Description = course.Description
        };
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