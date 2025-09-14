using Xunit;
using CareerPathRecommender.Infrastructure.Services;
using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Tests.Services;

public class MockAIServiceTests
{
    private readonly MockAIService _service;

    public MockAIServiceTests()
    {
        _service = new MockAIService();
    }

    [Fact]
    public async Task GenerateRecommendationReasoningAsync_ShouldReturnValidReasoning()
    {
        // Arrange
        var employee = CreateTestEmployeeDto();
        var course = CreateTestCourseDto();

        // Act
        var reasoning = await _service.GenerateRecommendationReasoningAsync(employee, course);

        // Assert
        Assert.NotNull(reasoning);
        Assert.False(string.IsNullOrWhiteSpace(reasoning));
        // The MockAIService generates reasoning based on templates, so check for meaningful content
        Assert.True(reasoning.Length > 50);
        Assert.Contains("course", reasoning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateMentorMatchReasoningAsync_ShouldReturnValidReasoning()
    {
        // Arrange
        var employee = CreateTestEmployeeDto();
        var mentor = CreateTestMentorDto();

        // Act
        var reasoning = await _service.GenerateMentorMatchReasoningAsync(employee, mentor);

        // Assert
        Assert.NotNull(reasoning);
        Assert.False(string.IsNullOrWhiteSpace(reasoning));
        Assert.Contains($"{mentor.FirstName} {mentor.LastName}", reasoning);
    }

    [Theory]
    [InlineData("Senior Software Engineer")]
    [InlineData("Tech Lead")]
    [InlineData("Full Stack Developer")]
    public async Task GenerateProjectMatchReasoningAsync_WithDifferentProjects_ShouldReturnValidReasoning(string projectName)
    {
        // Arrange
        var employee = CreateTestEmployeeDto();
        var project = new { Name = projectName, Description = $"Working on {projectName} project" };

        // Act
        var reasoning = await _service.GenerateProjectMatchReasoningAsync(employee, project);

        // Assert
        Assert.NotNull(reasoning);
        Assert.False(string.IsNullOrWhiteSpace(reasoning));
        Assert.Contains("project", reasoning, StringComparison.OrdinalIgnoreCase);
        Assert.True(reasoning.Length > 50); // Ensure it's a meaningful response
    }

    [Fact]
    public async Task GenerateRecommendationReasoningAsync_WithCancellation_ShouldHandleCancellation()
    {
        // Arrange
        var employee = CreateTestEmployeeDto();
        var course = CreateTestCourseDto();
        var cancellationToken = new CancellationToken(true);

        // Act & Assert - TaskCanceledException is a subclass of OperationCanceledException
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.GenerateRecommendationReasoningAsync(employee, course, cancellationToken));
    }

    private EmployeeDto CreateTestEmployeeDto()
    {
        return new EmployeeDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Position = "Software Developer",
            Department = "Engineering",
            YearsOfExperience = 3,
            Skills = new List<EmployeeSkillDto>
            {
                new() {
                    Skill = new SkillDto { Name = "C#", Category = "Programming" },
                    Level = SkillLevel.Intermediate
                },
                new() {
                    Skill = new SkillDto { Name = "SQL", Category = "Database" },
                    Level = SkillLevel.Beginner
                }
            }
        };
    }

    private EmployeeDto CreateTestMentorDto()
    {
        return new EmployeeDto
        {
            Id = 2,
            FirstName = "Sarah",
            LastName = "Johnson",
            Position = "Senior Developer",
            Department = "Engineering",
            YearsOfExperience = 8
        };
    }

    private CourseDto CreateTestCourseDto()
    {
        return new CourseDto
        {
            Id = 1,
            Title = "Advanced C# Programming",
            Provider = "Udemy",
            Category = "Programming",
            Rating = 4.5m,
            DurationHours = 40
        };
    }
}