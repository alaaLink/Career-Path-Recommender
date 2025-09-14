using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CareerPathRecommender.Infrastructure.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly ISkillRepository _skillRepository;
    private readonly IAIService _aiService;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        IEmployeeRepository employeeRepository,
        ICourseRepository courseRepository,
        IProjectRepository projectRepository,
        IRecommendationRepository recommendationRepository,
        IAIService aiService,
        ILogger<RecommendationService> logger)
    {
        _employeeRepository = employeeRepository;
        _courseRepository = courseRepository;
        _projectRepository = projectRepository;
        _recommendationRepository = recommendationRepository;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<IEnumerable<RecommendationDto>> GenerateRecommendationsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Execute recommendation types sequentially to avoid DbContext threading issues
            var recommendations = new List<RecommendationDto>();

            var courseRecommendations = await GenerateCourseRecommendationsAsync(employeeId, cancellationToken);
            recommendations.AddRange(courseRecommendations);

            var mentorRecommendations = await GenerateMentorRecommendationsAsync(employeeId, cancellationToken);
            recommendations.AddRange(mentorRecommendations);

            var projectRecommendations = await GenerateProjectRecommendationsAsync(employeeId, cancellationToken);
            recommendations.AddRange(projectRecommendations);

            return recommendations.OrderByDescending(r => r.Priority)
                                .ThenByDescending(r => r.ConfidenceScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations for employee {EmployeeId}. Exception: {ExceptionMessage}", employeeId, ex.Message);
            return new List<RecommendationDto>();
        }
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
        try
        {
            _logger.LogInformation("Generating course recommendations for employee {EmployeeId}", employeeId);

            var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found for course recommendations", employeeId);
                throw new ArgumentException($"Employee with ID {employeeId} not found");
            }

            // Get all available courses efficiently
            var allCourses = await _courseRepository.GetAllAsync(cancellationToken);
            // Pre-compute employee skill data for performance
            var employeeSkillCategories = employee.Skills.Select(s => s.Skill.Category).Distinct().ToHashSet();
            var employeeSkillNames = employee.Skills.Select(s => s.Skill.Name).Distinct().ToHashSet();
            var employeeSkillMap = employee.Skills.ToDictionary(s => s.SkillId, s => s.Level);

        // Get courses already taken by employee to avoid duplicates
        var enrolledCourses = await _courseRepository.GetEnrolledCoursesAsync(employeeId, cancellationToken);
        var enrolledCourseIds = enrolledCourses.Select(c => c.Id).ToHashSet();

        var courseScores = new List<(Course Course, double Score, string Reason)>();

        // Filter and score courses efficiently
        var availableCourses = allCourses.Where(c => !enrolledCourseIds.Contains(c.Id)).ToList();

        foreach (var course in availableCourses)
        {
            var score = CalculateCourseRelevanceScore(course, employee, employeeSkillCategories, employeeSkillNames);
            if (score.Score > 0.3) // Only recommend courses with reasonable relevance
            {
                courseScores.Add(score);
            }
        }

            // Take top 5 most relevant courses
            var topCourses = courseScores
                .OrderByDescending(cs => cs.Score)
                .Take(5)
                .ToList();

            var recommendations = new List<RecommendationDto>();

            foreach (var (course, score, reason) in topCourses)
            {
                var courseDto = MapToCourseDto(course);
                var aiReasoning = await _aiService.GenerateRecommendationReasoningAsync(
                    MapToEmployeeDto(employee), courseDto, cancellationToken);

                var priority = CalculateDynamicCoursePriority(course, score, employee);
                var confidenceScore = Math.Min(0.95m, (decimal)(0.6 + (score * 0.4))); // Scale score to 0.6-1.0 range

                var recommendation = new Recommendation
                {
                    EmployeeId = employeeId,
                    Type = RecommendationType.Course,
                    Title = $"Complete: {course.Title}",
                    Description = course.Description,
                    Reasoning = $"{reason} {aiReasoning}",
                    Priority = priority,
                    ConfidenceScore = confidenceScore,
                    CreatedDate = DateTime.UtcNow,
                    CourseId = course.Id
                };

                var savedRecommendation = await _recommendationRepository.AddAsync(recommendation, cancellationToken);
                recommendations.Add(MapToDto(savedRecommendation));
            }

            _logger.LogInformation("Generated {Count} course recommendations for employee {EmployeeId}", recommendations.Count, employeeId);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating course recommendations for employee {EmployeeId}", employeeId);
            return new List<RecommendationDto>();
        }
    }

    private async Task<IEnumerable<RecommendationDto>> GenerateMentorRecommendationsAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId, cancellationToken);
        if (employee == null)
            throw new ArgumentException($"Employee with ID {employeeId} not found");

        // Get potential mentors efficiently - filter by department and experience upfront
        var potentialMentors = await _employeeRepository.GetMentorCandidatesAsync(employeeId, cancellationToken);
        var mentorScores = new List<(Employee Mentor, double Score, string Reason)>();

        // Pre-compute employee skill map for performance
        var employeeSkillMap = employee.Skills.ToDictionary(s => s.SkillId, s => s.Level);

        // Process mentor scoring synchronously (CPU-bound operation)
        foreach (var mentor in potentialMentors)
        {
            var score = CalculateMentorRelevanceScore(mentor, employee, employeeSkillMap);
            if (score.Score > 0.4) // Only recommend mentors with good relevance
            {
                mentorScores.Add(score);
            }
        }

        // Take top 3 most relevant mentors with performance optimization
        var topMentors = mentorScores
            .OrderByDescending(ms => ms.Score)
            .Take(3)
            .ToList();

        if (!topMentors.Any())
        {
            return new List<RecommendationDto>(); // Early return if no suitable mentors
        }

        var recommendations = new List<RecommendationDto>();

        foreach (var (mentor, score, reason) in topMentors)
        {
            var aiReasoning = await _aiService.GenerateMentorMatchReasoningAsync(
                MapToEmployeeDto(employee), MapToEmployeeDto(mentor), cancellationToken);

            var priority = CalculateDynamicMentorPriority(mentor, score, employee);
            var confidenceScore = Math.Min(0.92m, (decimal)(0.65 + (score * 0.35))); // Scale score appropriately

            var recommendation = new Recommendation
            {
                EmployeeId = employeeId,
                Type = RecommendationType.Mentor,
                Title = $"Connect with: {mentor.FullName}",
                Description = $"{mentor.Position} in {mentor.Department} • {mentor.YearsOfExperience} years experience",
                Reasoning = $"{reason} {aiReasoning}",
                Priority = priority,
                ConfidenceScore = confidenceScore,
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

        var availableProjects = await _projectRepository.GetAvailableProjectsAsync(cancellationToken);
        var projectScores = new List<(object Project, double Score, string Reason)>();

        var employeeSkillMap = employee.Skills.ToDictionary(s => s.SkillId, s => s.Level);

        foreach (var project in availableProjects)
        {
            var score = await CalculateProjectRelevanceScore(project, employee, employeeSkillMap);
            if (score.Score > 0.5) // Only recommend projects with good skill match
            {
                projectScores.Add(score);
            }
        }

        // Take top 4 most suitable projects
        var topProjects = projectScores
            .OrderByDescending(ps => ps.Score)
            .Take(4)
            .ToList();

        var recommendations = new List<RecommendationDto>();

        foreach (var (project, score, reason) in topProjects)
        {
            var aiReasoning = await _aiService.GenerateProjectMatchReasoningAsync(
                MapToEmployeeDto(employee), project, cancellationToken);

            var priority = CalculateDynamicProjectPriority(project, score, employee);
            var confidenceScore = Math.Min(0.95m, (decimal)(0.7 + (score * 0.3))); // Higher base for projects

            // Cast project to access properties (assuming it has Name, Description, Id)
            var projectName = project.GetType().GetProperty("Name")?.GetValue(project)?.ToString() ?? "Unknown Project";
            var projectDesc = project.GetType().GetProperty("Description")?.GetValue(project)?.ToString() ?? "";
            var projectId = (int?)project.GetType().GetProperty("Id")?.GetValue(project) ?? 0;

            var recommendation = new Recommendation
            {
                EmployeeId = employeeId,
                Type = RecommendationType.Project,
                Title = $"Join Project: {projectName}",
                Description = projectDesc,
                Reasoning = $"{reason} {aiReasoning}",
                Priority = priority,
                ConfidenceScore = confidenceScore,
                CreatedDate = DateTime.UtcNow,
                ProjectId = projectId
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

        // Create dictionary of employee's current skills
        var employeeSkillsDict = employee.Skills.ToDictionary(es => es.Skill.Name, es => es.Level);

        // Define skill requirements for different target positions
        var targetSkillRequirements = GetTargetPositionSkillRequirements(targetPosition);

        var missingSkills = new List<SkillGapDto>();
        var skillsToImprove = new List<SkillGapDto>();

        // Analyze each required skill
        foreach (var requirement in targetSkillRequirements)
        {
            var currentLevel = employeeSkillsDict.TryGetValue(requirement.Key, out var level) ? level : SkillLevel.Beginner;
            var requiredLevel = requirement.Value;

            if (currentLevel < requiredLevel)
            {
                var skillGap = new SkillGapDto
                {
                    SkillName = requirement.Key,
                    CurrentLevel = currentLevel,
                    RequiredLevel = requiredLevel,
                    Priority = CalculateSkillPriority(requirement.Key, currentLevel, requiredLevel),
                    Reasoning = GenerateSkillGapReasoning(requirement.Key, currentLevel, requiredLevel, targetPosition),
                    Category = GetSkillCategory(requirement.Key),
                    EstimatedLearningTimeMonths = CalculateSkillLearningTime(currentLevel, requiredLevel),
                    RecommendedResources = GetRecommendedResources(requirement.Key),
                    ImportanceScore = CalculateImportanceScore(requirement.Key, targetPosition)
                };

                if (currentLevel == SkillLevel.Beginner)
                {
                    missingSkills.Add(skillGap);
                }
                else
                {
                    skillsToImprove.Add(skillGap);
                }
            }
        }

        // Generate personalized learning path
        var learningPath = GenerateLearningPath(missingSkills, skillsToImprove, targetPosition, employee.YearsOfExperience);
        var estimatedMonths = CalculateEstimatedTimeToTarget(missingSkills, skillsToImprove);
        var milestones = GenerateMilestoneTimeline(missingSkills, skillsToImprove, estimatedMonths);
        var actionItems = GenerateActionItems(missingSkills, skillsToImprove);

        var totalRequired = targetSkillRequirements.Count;
        var skillsMet = targetSkillRequirements.Count - missingSkills.Count() - skillsToImprove.Count();
        var readiness = totalRequired > 0 ? (decimal)skillsMet / totalRequired * 100 : 100;

        return new SkillGapAnalysisDto
        {
            MissingSkills = missingSkills.OrderByDescending(s => s.Priority),
            SkillsToImprove = skillsToImprove.OrderByDescending(s => s.Priority),
            RecommendedLearningPath = learningPath,
            EstimatedTimeToTargetMonths = estimatedMonths,
            TargetPosition = targetPosition,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            CurrentPosition = employee.Position,
            YearsOfExperience = employee.YearsOfExperience,
            AnalysisDate = DateTime.UtcNow,
            OverallReadiness = readiness,
            TotalSkillsRequired = totalRequired,
            SkillsMet = skillsMet,
            HighPriorityGaps = missingSkills.Count(s => s.Priority >= 4) + skillsToImprove.Count(s => s.Priority >= 4),
            NextActionItems = actionItems,
            MilestoneTimeline = milestones
        };
    }


    private Dictionary<string, SkillLevel> GetTargetPositionSkillRequirements(string targetPosition)
    {
        var requirements = new Dictionary<string, SkillLevel>();

        switch (targetPosition.ToLower())
        {
            case var p when p.Contains("senior") && (p.Contains("developer") || p.Contains("engineer")):
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["C#"] = SkillLevel.Advanced,
                    ["JavaScript"] = SkillLevel.Advanced,
                    ["SQL"] = SkillLevel.Advanced,
                    ["Cloud Computing"] = SkillLevel.Intermediate,
                    ["System Design"] = SkillLevel.Advanced,
                    ["Leadership"] = SkillLevel.Intermediate,
                    ["Mentoring"] = SkillLevel.Intermediate,
                    ["Problem Solving"] = SkillLevel.Advanced
                };
                break;

            case var p when p.Contains("lead") || p.Contains("team lead"):
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["Leadership"] = SkillLevel.Advanced,
                    ["Project Management"] = SkillLevel.Advanced,
                    ["Communication"] = SkillLevel.Advanced,
                    ["Mentoring"] = SkillLevel.Advanced,
                    ["Technical Architecture"] = SkillLevel.Advanced,
                    ["C#"] = SkillLevel.Expert,
                    ["System Design"] = SkillLevel.Expert
                };
                break;

            case var p when p.Contains("manager") || p.Contains("engineering manager"):
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["Leadership"] = SkillLevel.Expert,
                    ["Project Management"] = SkillLevel.Expert,
                    ["Team Management"] = SkillLevel.Expert,
                    ["Strategic Planning"] = SkillLevel.Advanced,
                    ["Budgeting"] = SkillLevel.Intermediate,
                    ["Communication"] = SkillLevel.Expert,
                    ["Performance Management"] = SkillLevel.Advanced
                };
                break;

            case var p when p.Contains("architect"):
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["System Design"] = SkillLevel.Expert,
                    ["Technical Architecture"] = SkillLevel.Expert,
                    ["Cloud Computing"] = SkillLevel.Expert,
                    ["Microservices"] = SkillLevel.Advanced,
                    ["Database Design"] = SkillLevel.Advanced,
                    ["Security"] = SkillLevel.Advanced,
                    ["Performance Optimization"] = SkillLevel.Advanced
                };
                break;

            case var p when p.Contains("full stack"):
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["C#"] = SkillLevel.Advanced,
                    ["JavaScript"] = SkillLevel.Advanced,
                    ["React"] = SkillLevel.Advanced,
                    ["SQL"] = SkillLevel.Advanced,
                    ["HTML/CSS"] = SkillLevel.Advanced,
                    ["API Development"] = SkillLevel.Advanced,
                    ["Database Design"] = SkillLevel.Intermediate
                };
                break;

            default:
                // Default requirements for general developer positions
                requirements = new Dictionary<string, SkillLevel>
                {
                    ["C#"] = SkillLevel.Intermediate,
                    ["JavaScript"] = SkillLevel.Intermediate,
                    ["SQL"] = SkillLevel.Intermediate,
                    ["Problem Solving"] = SkillLevel.Intermediate,
                    ["Communication"] = SkillLevel.Intermediate
                };
                break;
        }

        return requirements;
    }

    private int CalculateSkillPriority(string skillName, SkillLevel currentLevel, SkillLevel requiredLevel)
    {
        var levelGap = (int)requiredLevel - (int)currentLevel;

        // Critical skills get higher priority
        var criticalSkills = new[] { "Leadership", "Communication", "System Design", "Technical Architecture" };
        var isCritical = criticalSkills.Any(cs => skillName.Contains(cs, StringComparison.OrdinalIgnoreCase));

        return levelGap switch
        {
            >= 3 when isCritical => 5,
            >= 3 => 4,
            2 when isCritical => 4,
            2 => 3,
            1 when isCritical => 3,
            1 => 2,
            _ => 1
        };
    }

    private string GenerateSkillGapReasoning(string skillName, SkillLevel currentLevel, SkillLevel requiredLevel, string targetPosition)
    {
        var levelGap = (int)requiredLevel - (int)currentLevel;

        return skillName.ToLower() switch
        {
            var s when s.Contains("leadership") =>
                $"Strong leadership skills are essential for {targetPosition}. Moving from {currentLevel} to {requiredLevel} will enable you to guide teams effectively.",

            var s when s.Contains("communication") =>
                $"Excellent communication is crucial in {targetPosition} roles. Enhancing from {currentLevel} to {requiredLevel} will improve stakeholder interactions.",

            var s when s.Contains("system design") || s.Contains("architecture") =>
                $"System design expertise is fundamental for {targetPosition}. Advancing from {currentLevel} to {requiredLevel} will allow you to architect scalable solutions.",

            var s when s.Contains("c#") || s.Contains("javascript") =>
                $"Advanced {skillName} proficiency is required for {targetPosition}. Growing from {currentLevel} to {requiredLevel} will enhance your technical capabilities.",

            var s when s.Contains("cloud") =>
                $"Cloud computing skills are increasingly important in modern {targetPosition} roles. Upgrading from {currentLevel} to {requiredLevel} will keep you competitive.",

            _ => $"{skillName} proficiency at {requiredLevel} level is needed for {targetPosition}. Current {currentLevel} level needs improvement to meet role expectations."
        };
    }

    private string GenerateLearningPath(IEnumerable<SkillGapDto> missingSkills, IEnumerable<SkillGapDto> skillsToImprove, string targetPosition, int yearsOfExperience)
    {
        var path = new StringBuilder();

        path.AppendLine($"**Phase 1: Foundation Building (Months 1-3)**");
        path.AppendLine("• Start with highest priority missing skills");
        path.AppendLine("• Focus on fundamental concepts and practical application");
        path.AppendLine("• Complete online courses and tutorials");
        path.AppendLine();

        if (missingSkills.Any())
        {
            path.AppendLine("**Critical Skills to Acquire:**");
            foreach (var skill in missingSkills.OrderByDescending(s => s.Priority).Take(3))
            {
                path.AppendLine($"• {skill.SkillName}: {skill.CurrentLevel} → {skill.RequiredLevel}");
            }
            path.AppendLine();
        }

        path.AppendLine($"**Phase 2: Skill Enhancement (Months 4-6)**");
        path.AppendLine("• Work on improving existing skills");
        path.AppendLine("• Seek challenging projects that utilize target skills");
        path.AppendLine("• Consider mentorship opportunities");
        path.AppendLine();

        if (skillsToImprove.Any())
        {
            path.AppendLine("**Skills to Enhance:**");
            foreach (var skill in skillsToImprove.OrderByDescending(s => s.Priority).Take(3))
            {
                path.AppendLine($"• {skill.SkillName}: {skill.CurrentLevel} → {skill.RequiredLevel}");
            }
            path.AppendLine();
        }

        path.AppendLine($"**Phase 3: Mastery & Application (Months 7+)**");
        path.AppendLine("• Apply learned skills in real-world scenarios");
        path.AppendLine("• Lead projects that demonstrate your capabilities");
        path.AppendLine("• Share knowledge through mentoring or presentations");
        path.AppendLine("• Prepare for role transition or promotion");

        return path.ToString();
    }

    private int CalculateEstimatedTimeToTarget(IEnumerable<SkillGapDto> missingSkills, IEnumerable<SkillGapDto> skillsToImprove)
    {
        var totalMissingGaps = missingSkills.Sum(s => (int)s.RequiredLevel - (int)s.CurrentLevel);
        var totalImprovementGaps = skillsToImprove.Sum(s => (int)s.RequiredLevel - (int)s.CurrentLevel);

        // Estimate 2 months per skill level gap for missing skills, 1 month for improvements
        var estimatedMonths = (totalMissingGaps * 2) + (totalImprovementGaps * 1);

        // Minimum 3 months, maximum 24 months
        return Math.Max(3, Math.Min(24, estimatedMonths));
    }

    private string GetSkillCategory(string skillName)
    {
        return skillName.ToLower() switch
        {
            var s when s.Contains("c#") || s.Contains("javascript") || s.Contains("sql") => "Programming",
            var s when s.Contains("leadership") || s.Contains("communication") || s.Contains("management") => "Soft Skills",
            var s when s.Contains("cloud") || s.Contains("architecture") || s.Contains("design") => "Architecture & Cloud",
            var s when s.Contains("project") => "Project Management",
            _ => "Technical"
        };
    }

    private int CalculateSkillLearningTime(SkillLevel currentLevel, SkillLevel requiredLevel)
    {
        var levelGap = (int)requiredLevel - (int)currentLevel;
        return levelGap switch
        {
            1 => 2,
            2 => 4,
            3 => 6,
            _ => 8
        };
    }

    private List<string> GetRecommendedResources(string skillName)
    {
        return skillName.ToLower() switch
        {
            var s when s.Contains("c#") => new List<string> { "Microsoft Learn C# Path", "Pluralsight C# Courses", "C# in Depth Book" },
            var s when s.Contains("javascript") => new List<string> { "MDN JavaScript Guide", "freeCodeCamp", "You Don't Know JS Series" },
            var s when s.Contains("leadership") => new List<string> { "LinkedIn Leadership Courses", "Harvard Business Review", "The 7 Habits of Highly Effective People" },
            var s when s.Contains("cloud") => new List<string> { "Azure Fundamentals", "AWS Cloud Practitioner", "Google Cloud Platform Training" },
            var s when s.Contains("sql") => new List<string> { "SQL Server Documentation", "W3Schools SQL Tutorial", "PostgreSQL Tutorial" },
            _ => new List<string> { "Online Courses", "Documentation", "Hands-on Practice" }
        };
    }

    private decimal CalculateImportanceScore(string skillName, string targetPosition)
    {
        var baseScore = 0.5m;

        if (targetPosition.ToLower().Contains("senior") || targetPosition.ToLower().Contains("lead"))
        {
            if (skillName.ToLower().Contains("leadership") || skillName.ToLower().Contains("communication"))
                return 0.9m;
        }

        if (targetPosition.ToLower().Contains("architect"))
        {
            if (skillName.ToLower().Contains("design") || skillName.ToLower().Contains("architecture"))
                return 0.95m;
        }

        return skillName.ToLower() switch
        {
            var s when s.Contains("c#") || s.Contains("javascript") => 0.8m,
            var s when s.Contains("leadership") => 0.75m,
            var s when s.Contains("cloud") => 0.85m,
            _ => baseScore
        };
    }

    private List<CareerMilestoneDto> GenerateMilestoneTimeline(IEnumerable<SkillGapDto> missingSkills, IEnumerable<SkillGapDto> skillsToImprove, int totalMonths)
    {
        var milestones = new List<CareerMilestoneDto>();
        var allSkills = missingSkills.Concat(skillsToImprove).OrderByDescending(s => s.Priority).ToList();

        var quarterLength = Math.Max(3, totalMonths / 4);

        milestones.Add(new CareerMilestoneDto
        {
            Month = quarterLength,
            Title = "Foundation Phase Complete",
            Description = "Complete basic skill development and start practical application",
            SkillsToComplete = allSkills.Where(s => s.Priority >= 4).Select(s => s.SkillName).Take(3).ToList()
        });

        milestones.Add(new CareerMilestoneDto
        {
            Month = quarterLength * 2,
            Title = "Intermediate Proficiency",
            Description = "Demonstrate improved skills in real projects",
            SkillsToComplete = allSkills.Skip(3).Take(3).Select(s => s.SkillName).ToList()
        });

        milestones.Add(new CareerMilestoneDto
        {
            Month = quarterLength * 3,
            Title = "Advanced Application",
            Description = "Lead initiatives using newly acquired skills",
            SkillsToComplete = allSkills.Skip(6).Take(2).Select(s => s.SkillName).ToList()
        });

        milestones.Add(new CareerMilestoneDto
        {
            Month = totalMonths,
            Title = "Ready for Target Role",
            Description = "All skill gaps addressed, prepared for role transition",
            SkillsToComplete = new List<string> { "All target skills mastered" }
        });

        return milestones;
    }

    private List<string> GenerateActionItems(IEnumerable<SkillGapDto> missingSkills, IEnumerable<SkillGapDto> skillsToImprove)
    {
        var items = new List<string>();

        if (missingSkills.Any())
        {
            var topMissing = missingSkills.OrderByDescending(s => s.Priority).First();
            items.Add($"Start learning {topMissing.SkillName} immediately - this is your highest priority gap");
        }

        if (skillsToImprove.Any())
        {
            var topImprove = skillsToImprove.OrderByDescending(s => s.Priority).First();
            items.Add($"Find a mentor or advanced course for {topImprove.SkillName}");
        }

        items.Add("Schedule weekly review sessions to track progress");
        items.Add("Set up practice projects to apply new skills");
        items.Add("Join relevant professional communities or forums");

        return items;
    }

    private int CalculateCoursePriority(Course course)
    {
        if (course.Category.Contains("AI") || course.Category.Contains("Cloud")) return 5;
        if (course.Rating >= 4.5m) return 4;
        if (course.Rating >= 4.0m) return 3;
        return 2;
    }

    private static CourseDto MapToCourseDto(Course course)
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

    private static RecommendationDto MapToDto(Recommendation recommendation)
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

    private static EmployeeDto MapToEmployeeDto(Employee employee)
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

    // Smart recommendation logic helper methods
    private (Course Course, double Score, string Reason) CalculateCourseRelevanceScore(
        Course course, Employee employee, HashSet<string> employeeSkillCategories, HashSet<string> employeeSkillNames)
    {
        double score = 0;
        var reasons = new List<string>();

        // 1. Category relevance (30% of score)
        if (employeeSkillCategories.Contains(course.Category))
        {
            score += 0.3;
            reasons.Add($"Matches your {course.Category} expertise");
        }

        // 2. Career progression relevance (25% of score)
        var careerBoostCategories = GetCareerBoostCategories(employee.Position, employee.YearsOfExperience);
        if (careerBoostCategories.Contains(course.Category))
        {
            score += 0.25;
            reasons.Add($"Essential for advancing to senior {employee.Position} role");
        }

        // 3. Course quality (20% of score)
        var qualityScore = (double)course.Rating / 5.0;
        score += qualityScore * 0.2;
        if (course.Rating >= 4.5m)
            reasons.Add($"Highly rated course ({course.Rating}/5.0)");

        // 4. Duration appropriateness (15% of score)
        var durationScore = CalculateDurationScore(course.DurationHours, employee.YearsOfExperience);
        score += durationScore * 0.15;
        if (durationScore > 0.7)
            reasons.Add("Perfect duration for your experience level");

        // 5. Trending/High-demand skills (10% of score)
        if (IsHighDemandSkill(course.Category, course.Title))
        {
            score += 0.1;
            reasons.Add("High-demand skill in current market");
        }

        var reason = reasons.Any() ? string.Join("; ", reasons) + "." : "Relevant to your professional development.";
        return (course, score, reason);
    }

    private (Employee Mentor, double Score, string Reason) CalculateMentorRelevanceScore(
        Employee mentor, Employee employee, Dictionary<int, SkillLevel> employeeSkillMap)
    {
        double score = 0;
        var reasons = new List<string>();

        // 1. Career Track Alignment (40% of score) - Must be on same track with more experience
        var isOnSameTrack = IsOnSameCareerTrack(mentor.Position, employee.Position, mentor.Department, employee.Department);
        if (!isOnSameTrack)
        {
            // Mentor must be on same career track - immediate disqualification if not
            return (mentor, 0, "Not on same career progression track");
        }

        // 2. Experience differential (30% of score) - Must have at least 3 years more experience
        var experienceGap = mentor.YearsOfExperience - employee.YearsOfExperience;
        if (experienceGap < 3)
        {
            // Insufficient experience differential - disqualification
            return (mentor, 0, "Insufficient experience gap for mentoring");
        }

        if (experienceGap >= 3 && experienceGap <= 8)
        {
            score += 0.3;
            reasons.Add($"{experienceGap} years more experience in same track");
        }
        else if (experienceGap > 8)
        {
            score += 0.2; // Too much gap might make mentoring less effective
            reasons.Add($"{experienceGap} years more experience (senior mentor)");
        }

        // 3. Department and track relevance (30% of score)
        if (mentor.Department == employee.Department)
        {
            score += 0.3;
            reasons.Add("Same department and career track expertise");
        }
        else if (IsRelatedDepartment(mentor.Department, employee.Department))
        {
            score += 0.2;
            reasons.Add("Related department in same career track");
        }

        // 4. Skill advancement potential (40% of score)
        var skillOverlap = CalculateSkillAdvancementPotential(mentor, employeeSkillMap);
        score += skillOverlap * 0.4;
        if (skillOverlap > 0.7)
            reasons.Add("Excellent skill advancement opportunities in same track");
        else if (skillOverlap > 0.4)
            reasons.Add("Good skill development potential in career track");

        var reason = reasons.Any() ? string.Join("; ", reasons) + "." : "Same career track mentor opportunity.";
        return (mentor, score, reason);
    }

    private async Task<(object Project, double Score, string Reason)> CalculateProjectRelevanceScore(
        object project, Employee employee, Dictionary<int, SkillLevel> employeeSkillMap)
    {
        double score = 0;
        var reasons = new List<string>();

        // Get project skills (placeholder implementation)
        var requiredSkills = await GetProjectRequiredSkills(project);

        // 1. Skill match percentage (40% of score)
        var skillMatch = CalculateSkillMatchPercentage(employeeSkillMap, requiredSkills);
        score += skillMatch * 0.4;
        if (skillMatch > 0.7)
            reasons.Add($"Excellent skill match ({skillMatch:P0})");
        else if (skillMatch > 0.5)
            reasons.Add($"Good skill match ({skillMatch:P0})");

        // 2. Growth opportunity (30% of score)
        var growthOpportunity = CalculateGrowthOpportunity(employeeSkillMap, requiredSkills);
        score += growthOpportunity * 0.3;
        if (growthOpportunity > 0.6)
            reasons.Add("Significant learning opportunities");

        // 3. Department relevance (20% of score)
        var projectDept = project.GetType().GetProperty("Department")?.GetValue(project)?.ToString() ?? "";
        if (projectDept == employee.Department)
        {
            score += 0.2;
            reasons.Add("Within your department");
        }
        else if (IsRelatedDepartment(projectDept, employee.Department))
        {
            score += 0.1;
            reasons.Add("Cross-functional opportunity");
        }

        // 4. Project timing (10% of score)
        var projectStatus = GetProjectStatus(project);
        if (projectStatus == "Planning" || projectStatus == "Active")
        {
            score += 0.1;
            reasons.Add("Perfect timing to join");
        }

        var reason = reasons.Any() ? string.Join("; ", reasons) + "." : "Suitable for your skill level.";
        return (project, score, reason);
    }

    private int CalculateDynamicCoursePriority(Course course, double relevanceScore, Employee employee)
    {
        var basePriority = CalculateCoursePriority(course);

        // Boost priority based on relevance and career stage
        if (relevanceScore > 0.8 && employee.YearsOfExperience < 3) return 5; // High-impact for juniors
        if (relevanceScore > 0.7 && IsLeadershipTrack(course.Category, employee.Position)) return 5;
        if (relevanceScore > 0.6) return Math.Min(5, basePriority + 1);

        return basePriority;
    }

    private int CalculateDynamicMentorPriority(Employee mentor, double relevanceScore, Employee employee)
    {
        // Boost priority for high-relevance mentors
        if (relevanceScore > 0.8) return 5;
        if (relevanceScore > 0.6 && mentor.Department == employee.Department) return 5;
        if (relevanceScore > 0.5) return 4;

        return 3;
    }

    private int CalculateDynamicProjectPriority(object project, double relevanceScore, Employee employee)
    {
        // Adjust priority based on relevance and project characteristics
        if (relevanceScore > 0.9) return 5;
        if (relevanceScore > 0.7) return 4;
        if (relevanceScore > 0.5) return 3;

        return 2;
    }

    private HashSet<string> GetCareerBoostCategories(string position, int yearsOfExperience)
    {
        var categories = new HashSet<string>();

        // Career progression logic based on position and experience
        if (yearsOfExperience < 2)
        {
            categories.UnionWith(new[] { "Programming", "Frontend", "Backend" });
        }
        else if (yearsOfExperience < 5)
        {
            categories.UnionWith(new[] { "Cloud", "DevOps", "Database", "Management" });
        }
        else
        {
            categories.UnionWith(new[] { "Leadership", "Management", "Analytics", "AI" });
        }

        // Position-specific categories
        if (position.Contains("Developer") || position.Contains("Engineer"))
        {
            categories.UnionWith(new[] { "Programming", "Cloud", "DevOps" });
        }
        else if (position.Contains("Manager") || position.Contains("Lead"))
        {
            categories.UnionWith(new[] { "Leadership", "Management", "Analytics" });
        }
        else if (position.Contains("Designer"))
        {
            categories.UnionWith(new[] { "Design", "Frontend" });
        }

        return categories;
    }

    private double CalculateDurationScore(int durationHours, int yearsOfExperience)
    {
        var optimalHours = yearsOfExperience switch
        {
            < 2 => 20, // Junior: shorter courses
            < 5 => 35, // Mid-level: moderate courses
            _ => 50    // Senior: comprehensive courses
        };

        var difference = Math.Abs(durationHours - optimalHours);
        return Math.Max(0, 1.0 - (difference / (double)optimalHours));
    }

    private bool IsHighDemandSkill(string category, string title)
    {
        var highDemandKeywords = new[]
        {
            "AI", "Machine Learning", "Cloud", "Kubernetes", "Docker",
            "React", "Angular", "Vue", "Python", "JavaScript", "TypeScript",
            "DevOps", "Microservices", "Blockchain", "Cybersecurity"
        };

        return highDemandKeywords.Any(keyword =>
            category.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            title.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsRelatedDepartment(string dept1, string dept2)
    {
        var relatedDepts = new Dictionary<string, string[]>
        {
            ["Engineering"] = new[] { "IT Operations", "Quality Assurance", "Security", "Analytics" },
            ["Product"] = new[] { "Engineering", "Marketing", "Design" },
            ["Marketing"] = new[] { "Product", "Sales", "Design" },
            ["Analytics"] = new[] { "Engineering", "Business", "Marketing" }
        };

        return relatedDepts.ContainsKey(dept1) && relatedDepts[dept1].Contains(dept2) ||
               relatedDepts.ContainsKey(dept2) && relatedDepts[dept2].Contains(dept1);
    }

    private double CalculateSkillAdvancementPotential(Employee mentor, Dictionary<int, SkillLevel> employeeSkills)
    {
        if (!mentor.Skills.Any()) return 0.0;

        var advancementCount = 0;
        var totalComparisons = 0;

        foreach (var mentorSkill in mentor.Skills)
        {
            if (employeeSkills.TryGetValue(mentorSkill.SkillId, out var employeeLevel))
            {
                totalComparisons++;
                if (mentorSkill.Level > employeeLevel)
                {
                    advancementCount++;
                }
            }
        }

        return totalComparisons > 0 ? (double)advancementCount / totalComparisons : 0.0;
    }

    private bool IsOnSameCareerTrack(string mentorPosition, string employeePosition, string mentorDepartment, string employeeDepartment)
    {
        // Define comprehensive career tracks with progression paths
        var careerTracks = new Dictionary<string, string[]>
        {
            // Engineering Track
            ["Junior Developer"] = new[] { "Developer", "Software Developer", "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering", "Chief Technology Officer", "Chief Architect" },
            ["Developer"] = new[] { "Software Developer", "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Software Developer"] = new[] { "Senior Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Frontend Developer"] = new[] { "Senior Frontend Developer", "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Backend Developer"] = new[] { "Senior Backend Developer", "DevOps Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Mobile Developer"] = new[] { "Senior Mobile Developer", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Database Developer"] = new[] { "Senior Database Administrator", "Senior Systems Architect", "Tech Lead", "Principal Engineer" },
            ["Senior Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Senior Frontend Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Senior Backend Developer"] = new[] { "DevOps Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Senior Mobile Developer"] = new[] { "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["DevOps Engineer"] = new[] { "Senior Cloud Engineer", "Senior Systems Architect", "Tech Lead", "Principal Engineer", "Engineering Manager" },
            ["Tech Lead"] = new[] { "Principal Engineer", "Engineering Manager", "VP of Engineering" },
            ["Principal Engineer"] = new[] { "Chief Architect", "Engineering Manager", "VP of Engineering", "Chief Technology Officer" },
            ["Engineering Manager"] = new[] { "VP of Engineering", "Chief Technology Officer" },

            // Quality Assurance Track
            ["QA Engineer"] = new[] { "Senior QA Engineer", "QA Manager" },
            ["Senior QA Engineer"] = new[] { "QA Manager" },

            // Data & Analytics Track
            ["Data Analyst"] = new[] { "Senior Data Scientist", "Director of Analytics" },
            ["Senior Data Scientist"] = new[] { "Director of Analytics" },
            ["Business Analyst"] = new[] { "Senior Business Analyst", "Senior Product Manager", "Director of Product" },
            ["Senior Business Analyst"] = new[] { "Senior Product Manager", "Director of Product" },

            // Design Track
            ["UX Designer"] = new[] { "Senior UX Designer" },
            ["UI Designer"] = new[] { "Senior UX Designer" },
            ["Senior UX Designer"] = new[] { "Director of Product" },

            // Management Track
            ["Product Owner"] = new[] { "Senior Product Manager", "Director of Product" },
            ["Senior Product Manager"] = new[] { "Director of Product" },

            // Marketing Track
            ["Marketing Coordinator"] = new[] { "Marketing Manager", "VP of Marketing" },
            ["Content Writer"] = new[] { "Senior Content Strategist", "Marketing Manager" },
            ["Marketing Manager"] = new[] { "VP of Marketing" },
            ["Senior Content Strategist"] = new[] { "Marketing Manager", "VP of Marketing" },

            // Operations & Support Track
            ["Systems Administrator"] = new[] { "Senior Database Administrator", "Senior Systems Architect", "VP of Operations" },
            ["Support Engineer"] = new[] { "Senior Systems Architect", "Engineering Manager" },
            ["Security Analyst"] = new[] { "Senior Security Engineer", "Chief Security Officer" },
            ["Senior Security Engineer"] = new[] { "Chief Security Officer" },

            // HR Track
            ["HR Coordinator"] = new[] { "Senior HR Manager" },
            ["Senior HR Manager"] = new[] { "VP of Operations" },

            // Sales Track
            ["Sales Associate"] = new[] { "Senior Sales Manager" },
            ["Senior Sales Manager"] = new[] { "VP of Marketing" }
        };

        // Check if employee position exists in tracks and mentor position is in the progression
        if (careerTracks.ContainsKey(employeePosition))
        {
            return careerTracks[employeePosition].Contains(mentorPosition);
        }

        // For positions not explicitly mapped, check if they're in similar departments with seniority levels
        if (mentorDepartment == employeeDepartment)
        {
            return IsSeniorPosition(mentorPosition, employeePosition);
        }

        return false;
    }

    private bool IsCareerPathAligned(string mentorPosition, string employeePosition)
    {
        // Legacy method - keeping for compatibility but using new logic
        return IsOnSameCareerTrack(mentorPosition, employeePosition, "", "");
    }

    private bool IsSeniorPosition(string mentorPosition, string employeePosition)
    {
        var seniorityKeywords = new[] { "Senior", "Principal", "Lead", "Manager", "Director", "VP", "Chief" };
        var mentorSeniority = seniorityKeywords.Count(keyword => mentorPosition.Contains(keyword));
        var employeeSeniority = seniorityKeywords.Count(keyword => employeePosition.Contains(keyword));

        return mentorSeniority > employeeSeniority;
    }

    private async Task<Dictionary<int, SkillLevel>> GetProjectRequiredSkills(object project)
    {
        // Placeholder - in real implementation would query ProjectSkills table
        await Task.CompletedTask;
        return new Dictionary<int, SkillLevel>();
    }

    private double CalculateSkillMatchPercentage(Dictionary<int, SkillLevel> employeeSkills, Dictionary<int, SkillLevel> requiredSkills)
    {
        if (!requiredSkills.Any()) return 0.8; // Default good match if no requirements

        var matches = 0;
        foreach (var required in requiredSkills)
        {
            if (employeeSkills.TryGetValue(required.Key, out var employeeLevel) &&
                employeeLevel >= required.Value)
            {
                matches++;
            }
        }

        return (double)matches / requiredSkills.Count;
    }

    private double CalculateGrowthOpportunity(Dictionary<int, SkillLevel> employeeSkills, Dictionary<int, SkillLevel> requiredSkills)
    {
        if (!requiredSkills.Any()) return 0.5;

        var growthOpportunities = 0;
        foreach (var required in requiredSkills)
        {
            if (!employeeSkills.ContainsKey(required.Key) ||
                employeeSkills[required.Key] < required.Value)
            {
                growthOpportunities++;
            }
        }

        var growthRatio = (double)growthOpportunities / requiredSkills.Count;
        return growthRatio > 0.7 ? 0.3 : growthRatio; // Cap if too many missing skills
    }

    private string GetProjectStatus(object project)
    {
        var status = project.GetType().GetProperty("Status")?.GetValue(project);
        return status?.ToString() ?? "Unknown";
    }

    private bool IsLeadershipTrack(string courseCategory, string position)
    {
        return courseCategory.Contains("Leadership") || courseCategory.Contains("Management") ||
               (position.Contains("Senior") && courseCategory.Contains("Soft Skills"));
    }
}