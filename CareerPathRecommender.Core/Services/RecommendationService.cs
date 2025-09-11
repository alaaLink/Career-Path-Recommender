using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Core.Data;
using CareerPathRecommender.Core.Models;

namespace CareerPathRecommender.Core.Services;

public class RecommendationService : IRecommendationService
{
    private readonly ApplicationDbContext _context;
    private readonly IAzureOpenAIService _aiService;

    public RecommendationService(ApplicationDbContext context, IAzureOpenAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public async Task<IEnumerable<Recommendation>> GenerateRecommendationsAsync(int employeeId)
    {
        var recommendations = new List<Recommendation>();
        
        var courseRecommendations = await GenerateCourseRecommendationsAsync(employeeId);
        var mentorRecommendations = await GenerateMentorRecommendationsAsync(employeeId);
        var projectRecommendations = await GenerateProjectRecommendationsAsync(employeeId);
        
        recommendations.AddRange(courseRecommendations);
        recommendations.AddRange(mentorRecommendations);
        recommendations.AddRange(projectRecommendations);
        
        return recommendations.OrderByDescending(r => r.Priority)
                            .ThenByDescending(r => r.ConfidenceScore);
    }

    public async Task<IEnumerable<Course>> RecommendCoursesAsync(int employeeId)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var employeeSkills = employee.Skills.Select(s => s.SkillId).ToHashSet();
        
        var courses = await _context.Courses
            .Where(c => !_context.EmployeeCourses
                .Any(ec => ec.EmployeeId == employeeId && ec.CourseId == c.Id))
            .ToListAsync();
            
        return courses
            .OrderByDescending(c => c.Rating)
            .Take(10);
    }

    public async Task<IEnumerable<Employee>> RecommendMentorsAsync(int employeeId)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var employeeSkillIds = employee.Skills.Select(s => s.SkillId).ToHashSet();
        
        var potentialMentors = await _context.Employees
            .Include(e => e.Skills)
            .Where(e => e.Id != employeeId && 
                        e.YearsOfExperience > employee.YearsOfExperience + 2)
            .ToListAsync();
        
        var mentors = potentialMentors
            .Where(m => m.Skills.Any(s => employeeSkillIds.Contains(s.SkillId) && 
                                         s.Level > employee.Skills
                                             .Where(es => es.SkillId == s.SkillId)
                                             .Select(es => es.Level)
                                             .FirstOrDefault()))
            .Take(5)
            .ToList();
        
        return mentors;
    }

    public async Task<IEnumerable<Project>> RecommendProjectsAsync(int employeeId)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var employeeSkills = employee.Skills.ToDictionary(s => s.SkillId, s => s.Level);
        
        var availableProjects = await _context.Projects
            .Include(p => p.RequiredSkills)
            .ThenInclude(ps => ps.Skill)
            .Where(p => p.Status == ProjectStatus.Planning || p.Status == ProjectStatus.Active)
            .Where(p => p.Assignments.Count() < p.MaxTeamSize)
            .ToListAsync();
        
        var scoredProjects = new List<(Project Project, int Score)>();
        
        foreach (var project in availableProjects)
        {
            int matchScore = CalculateProjectMatchScore(employeeSkills, project.RequiredSkills);
            if (matchScore >= 60) // Minimum 60% match
            {
                scoredProjects.Add((project, matchScore));
            }
        }
        
        return scoredProjects
            .OrderByDescending(sp => sp.Score)
            .Take(5)
            .Select(sp => sp.Project);
    }

    public async Task<SkillGapAnalysis> AnalyzeSkillGapsAsync(int employeeId, string targetPosition)
    {
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        return await _aiService.AnalyzeCareerPathAsync(employee, targetPosition);
    }

    private async Task<Employee> GetEmployeeWithSkillsAsync(int employeeId)
    {
        return await _context.Employees
            .Include(e => e.Skills)
            .ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(e => e.Id == employeeId) 
            ?? throw new ArgumentException($"Employee with ID {employeeId} not found");
    }

    private async Task<IEnumerable<Recommendation>> GenerateCourseRecommendationsAsync(int employeeId)
    {
        var courses = await RecommendCoursesAsync(employeeId);
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var recommendations = new List<Recommendation>();

        foreach (var course in courses.Take(3))
        {
            var reasoning = await _aiService.GenerateRecommendationReasoningAsync(employee, course);
            
            recommendations.Add(new Recommendation
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
            });
        }

        return recommendations;
    }

    private async Task<IEnumerable<Recommendation>> GenerateMentorRecommendationsAsync(int employeeId)
    {
        var mentors = await RecommendMentorsAsync(employeeId);
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var recommendations = new List<Recommendation>();

        foreach (var mentor in mentors.Take(2))
        {
            var reasoning = await _aiService.GenerateMentorMatchReasoningAsync(employee, mentor);
            
            recommendations.Add(new Recommendation
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
            });
        }

        return recommendations;
    }

    private async Task<IEnumerable<Recommendation>> GenerateProjectRecommendationsAsync(int employeeId)
    {
        var projects = await RecommendProjectsAsync(employeeId);
        var employee = await GetEmployeeWithSkillsAsync(employeeId);
        var recommendations = new List<Recommendation>();

        foreach (var project in projects.Take(2))
        {
            var reasoning = await _aiService.GenerateProjectMatchReasoningAsync(employee, project);
            
            recommendations.Add(new Recommendation
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
            });
        }

        return recommendations;
    }

    private int CalculateProjectMatchScore(Dictionary<int, SkillLevel> employeeSkills, IEnumerable<ProjectSkill> requiredSkills)
    {
        if (!requiredSkills.Any()) return 100;

        int totalScore = 0;
        int skillCount = 0;

        foreach (var requiredSkill in requiredSkills)
        {
            skillCount++;
            if (employeeSkills.TryGetValue(requiredSkill.SkillId, out var employeeLevel))
            {
                if (employeeLevel >= requiredSkill.RequiredLevel)
                {
                    totalScore += 100;
                }
                else
                {
                    var levelDiff = (int)requiredSkill.RequiredLevel - (int)employeeLevel;
                    totalScore += Math.Max(0, 100 - (levelDiff * 25));
                }
            }
            else if (!requiredSkill.IsRequired)
            {
                totalScore += 50; // Partial credit for optional skills
            }
        }

        return skillCount > 0 ? totalScore / skillCount : 0;
    }

    private int CalculateCoursePriority(Course course)
    {
        if (course.Category.Contains("AI") || course.Category.Contains("Cloud")) return 5;
        if (course.Rating >= 4.5m) return 4;
        if (course.Rating >= 4.0m) return 3;
        return 2;
    }
}