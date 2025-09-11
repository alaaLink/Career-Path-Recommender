using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Application.Interfaces;

namespace CareerPathRecommender.Infrastructure.Services;

public class MockAIService : IAIService
{
    public Task<string> GenerateRecommendationReasoningAsync(EmployeeDto employee, object item, CancellationToken cancellationToken = default)
    {
        var reasoning = item switch
        {
            CourseDto course => GenerateCourseReasoning(employee, course),
            _ => "AI analysis suggests this recommendation aligns well with your career goals."
        };
        return Task.FromResult(reasoning);
    }

    public Task<string> GenerateMentorMatchReasoningAsync(EmployeeDto employee, EmployeeDto mentor, CancellationToken cancellationToken = default)
    {
        var reasoning = GenerateMentorReasoning(employee, mentor);
        return Task.FromResult(reasoning);
    }

    public Task<string> GenerateProjectMatchReasoningAsync(EmployeeDto employee, object project, CancellationToken cancellationToken = default)
    {
        var reasoning = GenerateProjectReasoning(employee, project);
        return Task.FromResult(reasoning);
    }

    private string GenerateCourseReasoning(EmployeeDto employee, CourseDto course)
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

    private string GenerateMentorReasoning(EmployeeDto employee, EmployeeDto mentor)
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

    private string GenerateProjectReasoning(EmployeeDto employee, object project)
    {
        var reasoningTemplates = new[]
        {
            $"This project offers hands-on experience with modern technologies and methodologies that align with your career development goals.",
            $"This project provides an excellent opportunity to apply your current skills while learning new ones. The collaborative environment will enhance your teamwork and communication abilities.",
            $"Working on this project will give you exposure to enterprise-level challenges and help you build a portfolio of impactful work that demonstrates your capabilities.",
            $"The project timeline and scope are well-suited to your experience level, providing the right balance of challenge and achievable outcomes."
        };

        var random = new Random();
        return reasoningTemplates[random.Next(reasoningTemplates.Length)];
    }
}

public record CourseDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public int DurationHours { get; init; }
    public decimal Rating { get; init; }
    public decimal Price { get; init; }
    public string Url { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}