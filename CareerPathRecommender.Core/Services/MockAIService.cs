using CareerPathRecommender.Core.Models;

namespace CareerPathRecommender.Core.Services;

public class MockAIService : IAzureOpenAIService
{
    public Task<string> GenerateRecommendationReasoningAsync(Employee employee, Course course)
    {
        var reasoning = GenerateCourseReasoning(employee, course);
        return Task.FromResult(reasoning);
    }

    public Task<string> GenerateMentorMatchReasoningAsync(Employee employee, Employee mentor)
    {
        var reasoning = GenerateMentorReasoning(employee, mentor);
        return Task.FromResult(reasoning);
    }

    public Task<string> GenerateProjectMatchReasoningAsync(Employee employee, Project project)
    {
        var reasoning = GenerateProjectReasoning(employee, project);
        return Task.FromResult(reasoning);
    }

    public Task<SkillGapAnalysis> AnalyzeCareerPathAsync(Employee employee, string targetPosition)
    {
        var analysis = GenerateSkillGapAnalysis(employee, targetPosition);
        return Task.FromResult(analysis);
    }

    public Task<string> GenerateLearningPathAsync(Employee employee, IEnumerable<SkillGap> skillGaps)
    {
        var learningPath = GenerateLearningPath(employee, skillGaps);
        return Task.FromResult(learningPath);
    }

    private string GenerateCourseReasoning(Employee employee, Course course)
    {
        var reasoningTemplates = new[]
        {
            $"Based on your {employee.YearsOfExperience} years of experience as a {employee.Position}, this {course.Category} course will help you advance your skills and stay current with industry trends.",
            $"This course is highly rated ({course.Rating}/5) and aligns perfectly with your career goals in the {employee.Department} department. It will strengthen your expertise in {course.Title}.",
            $"Given your current skill level, this {course.DurationHours}-hour course provides the right depth to enhance your knowledge while fitting into your learning schedule.",
            $"This course from {course.Provider} covers essential concepts that are directly applicable to your role as {employee.Position} and will boost your career prospects."
        };

        var random = new Random();
        return reasoningTemplates[random.Next(reasoningTemplates.Length)];
    }

    private string GenerateMentorReasoning(Employee employee, Employee mentor)
    {
        var reasoningTemplates = new[]
        {
            $"{mentor.FullName} brings {mentor.YearsOfExperience} years of experience in {mentor.Department} and has advanced skills that complement your current expertise. Their guidance will accelerate your professional growth.",
            $"As a {mentor.Position}, {mentor.FullName} has navigated similar career challenges and can provide valuable insights for your progression from {employee.Position}.",
            $"The {mentor.YearsOfExperience - employee.YearsOfExperience} years of additional experience that {mentor.FullName} has will provide you with strategic career advice and industry knowledge.",
            $"{mentor.FullName}'s expertise in {mentor.Department} makes them an ideal mentor to help you develop leadership skills and technical competencies."
        };

        var random = new Random();
        return reasoningTemplates[random.Next(reasoningTemplates.Length)];
    }

    private string GenerateProjectReasoning(Employee employee, Project project)
    {
        var reasoningTemplates = new[]
        {
            $"The {project.Name} project in {project.Department} offers hands-on experience with modern technologies and methodologies that align with your career development goals.",
            $"This project provides an excellent opportunity to apply your current skills while learning new ones. The collaborative environment will enhance your teamwork and communication abilities.",
            $"Working on {project.Name} will give you exposure to enterprise-level challenges and help you build a portfolio of impactful work that demonstrates your capabilities.",
            $"The project timeline and scope are well-suited to your experience level, providing the right balance of challenge and achievable outcomes."
        };

        var random = new Random();
        return reasoningTemplates[random.Next(reasoningTemplates.Length)];
    }

    private SkillGapAnalysis GenerateSkillGapAnalysis(Employee employee, string targetPosition)
    {
        var missingSkills = new List<SkillGap>();
        var skillsToImprove = new List<SkillGap>();

        // Analyze target position and generate relevant gaps
        var position = targetPosition.ToLower();

        if (position.Contains("senior") || position.Contains("lead"))
        {
            missingSkills.AddRange(new[]
            {
                new SkillGap
                {
                    SkillName = "Leadership & Team Management",
                    CurrentLevel = SkillLevel.Beginner,
                    RequiredLevel = SkillLevel.Advanced,
                    Priority = 5,
                    Reasoning = "Leadership skills are essential for senior positions to guide teams and drive project success."
                },
                new SkillGap
                {
                    SkillName = "Strategic Planning",
                    CurrentLevel = SkillLevel.Beginner,
                    RequiredLevel = SkillLevel.Intermediate,
                    Priority = 4,
                    Reasoning = "Senior roles require ability to plan and execute long-term technical strategies."
                }
            });
        }

        if (position.Contains("architect") || position.Contains("technical"))
        {
            missingSkills.Add(new SkillGap
            {
                SkillName = "System Architecture & Design",
                CurrentLevel = SkillLevel.Intermediate,
                RequiredLevel = SkillLevel.Expert,
                Priority = 5,
                Reasoning = "Advanced system design skills are crucial for architect roles to design scalable solutions."
            });
        }

        if (position.Contains("full") && position.Contains("stack"))
        {
            missingSkills.Add(new SkillGap
            {
                SkillName = "Frontend Technologies (React/Angular)",
                CurrentLevel = SkillLevel.Beginner,
                RequiredLevel = SkillLevel.Advanced,
                Priority = 4,
                Reasoning = "Full-stack roles require proficiency in modern frontend frameworks."
            });
        }

        // Always include communication as improvement area
        skillsToImprove.Add(new SkillGap
        {
            SkillName = "Communication & Presentation",
            CurrentLevel = SkillLevel.Intermediate,
            RequiredLevel = SkillLevel.Advanced,
            Priority = 3,
            Reasoning = "Strong communication skills are valuable for career advancement and team collaboration."
        });

        // Add current skills that need improvement
        foreach (var skill in employee.Skills)
        {
            if (skill.Level < SkillLevel.Expert)
            {
                skillsToImprove.Add(new SkillGap
                {
                    SkillName = skill.Skill.Name,
                    CurrentLevel = skill.Level,
                    RequiredLevel = (SkillLevel)Math.Min((int)skill.Level + 1, (int)SkillLevel.Expert),
                    Priority = 3,
                    Reasoning = $"Advancing your {skill.Skill.Name} skills will strengthen your technical foundation for the target role."
                });
            }
        }

        var estimatedMonths = CalculateTimeToTarget(missingSkills, skillsToImprove);
        var learningPath = GenerateDetailedLearningPath(missingSkills, skillsToImprove, targetPosition);

        return new SkillGapAnalysis
        {
            MissingSkills = missingSkills,
            SkillsToImprove = skillsToImprove,
            RecommendedLearningPath = learningPath,
            EstimatedTimeToTargetMonths = estimatedMonths
        };
    }

    private string GenerateLearningPath(Employee employee, IEnumerable<SkillGap> skillGaps)
    {
        var steps = new List<string>();
        var highPriorityGaps = skillGaps.Where(g => g.Priority >= 4).OrderByDescending(g => g.Priority);
        var mediumPriorityGaps = skillGaps.Where(g => g.Priority == 3);

        steps.Add("**Phase 1: Foundation (Months 1-3)**");
        foreach (var gap in highPriorityGaps.Take(2))
        {
            steps.Add($"• Master {gap.SkillName} through targeted courses and practical projects");
        }

        steps.Add("\n**Phase 2: Skill Enhancement (Months 4-8)**");
        foreach (var gap in mediumPriorityGaps.Take(3))
        {
            steps.Add($"• Improve {gap.SkillName} from {gap.CurrentLevel} to {gap.RequiredLevel} level");
        }

        steps.Add("\n**Phase 3: Leadership & Integration (Months 9-12)**");
        steps.Add("• Seek leadership opportunities on current projects");
        steps.Add("• Find a mentor in the target role");
        steps.Add("• Apply for positions that bridge current and target roles");

        return string.Join("\n", steps);
    }

    private int CalculateTimeToTarget(List<SkillGap> missingSkills, List<SkillGap> skillsToImprove)
    {
        var baseMonths = 6;
        var additionalMonths = missingSkills.Count * 2 + skillsToImprove.Count * 1;
        return Math.Min(baseMonths + additionalMonths, 24); // Cap at 24 months
    }

    private string GenerateDetailedLearningPath(List<SkillGap> missingSkills, List<SkillGap> skillsToImprove, string targetPosition)
    {
        var pathElements = new List<string>
        {
            $"**Career Transition Plan to {targetPosition}**",
            "",
            "**Immediate Actions (Next 1-2 months):**",
            "• Enroll in high-priority skill courses identified in the analysis",
            "• Connect with professionals in similar roles for networking",
            "• Start documenting your current projects and achievements",
            "",
            "**Short-term Development (3-6 months):**",
            "• Complete certification courses for missing critical skills",
            "• Seek stretch assignments that align with target role responsibilities",
            "• Join relevant professional communities and attend industry events",
            "",
            "**Long-term Growth (6-12 months):**",
            "• Apply for internal positions or projects that serve as stepping stones",
            "• Mentor junior team members to develop leadership experience",
            "• Build a portfolio showcasing your expanded skill set"
        };

        return string.Join("\n", pathElements);
    }
}