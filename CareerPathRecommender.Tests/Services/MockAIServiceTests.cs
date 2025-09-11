using Xunit;
using CareerPathRecommender.Core.Services;
using CareerPathRecommender.Core.Models;

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
        var employee = CreateTestEmployee();
        var course = CreateTestCourse();

        // Act
        var reasoning = await _service.GenerateRecommendationReasoningAsync(employee, course);

        // Assert
        Assert.NotNull(reasoning);
        Assert.False(string.IsNullOrWhiteSpace(reasoning));
        Assert.Contains(employee.Position, reasoning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateMentorMatchReasoningAsync_ShouldReturnValidReasoning()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var mentor = CreateTestMentor();

        // Act
        var reasoning = await _service.GenerateMentorMatchReasoningAsync(employee, mentor);

        // Assert
        Assert.NotNull(reasoning);
        Assert.False(string.IsNullOrWhiteSpace(reasoning));
        Assert.Contains(mentor.FullName, reasoning);
    }

    [Theory]
    [InlineData("Senior Software Engineer")]
    [InlineData("Tech Lead")]
    [InlineData("Full Stack Developer")]
    public async Task AnalyzeCareerPathAsync_WithDifferentTargetPositions_ShouldReturnRelevantAnalysis(string targetPosition)
    {
        // Arrange
        var employee = CreateTestEmployee();

        // Act
        var analysis = await _service.AnalyzeCareerPathAsync(employee, targetPosition);

        // Assert
        Assert.NotNull(analysis);
        Assert.NotNull(analysis.MissingSkills);
        Assert.NotNull(analysis.SkillsToImprove);
        Assert.True(analysis.EstimatedTimeToTargetMonths > 0);
        Assert.False(string.IsNullOrEmpty(analysis.RecommendedLearningPath));

        if (targetPosition.Contains("Senior") || targetPosition.Contains("Lead"))
        {
            Assert.Contains(analysis.MissingSkills, gap => gap.SkillName.Contains("Leadership"));
        }

        if (targetPosition.Contains("Full") && targetPosition.Contains("Stack"))
        {
            Assert.Contains(analysis.MissingSkills, gap => gap.SkillName.Contains("Frontend"));
        }
    }

    [Fact]
    public async Task AnalyzeCareerPathAsync_ShouldIncludeCurrentSkillImprovements()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var targetPosition = "Senior Developer";

        // Act
        var analysis = await _service.AnalyzeCareerPathAsync(employee, targetPosition);

        // Assert
        Assert.Contains(analysis.SkillsToImprove, gap => 
            employee.Skills.Any(es => es.Skill.Name == gap.SkillName));
    }

    [Fact]
    public async Task GenerateLearningPathAsync_ShouldReturnStructuredPath()
    {
        // Arrange
        var employee = CreateTestEmployee();
        var skillGaps = new List<SkillGap>
        {
            new() { SkillName = "Leadership", Priority = 5 },
            new() { SkillName = "React", Priority = 4 }
        };

        // Act
        var learningPath = await _service.GenerateLearningPathAsync(employee, skillGaps);

        // Assert
        Assert.NotNull(learningPath);
        Assert.Contains("Phase", learningPath);
        Assert.Contains("Month", learningPath);
    }

    private Employee CreateTestEmployee()
    {
        return new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Position = "Software Developer",
            Department = "Engineering",
            YearsOfExperience = 3,
            Skills = new List<EmployeeSkill>
            {
                new() { 
                    Skill = new Skill { Name = "C#", Category = "Programming" }, 
                    Level = SkillLevel.Intermediate 
                },
                new() { 
                    Skill = new Skill { Name = "SQL", Category = "Database" }, 
                    Level = SkillLevel.Beginner 
                }
            }
        };
    }

    private Employee CreateTestMentor()
    {
        return new Employee
        {
            Id = 2,
            FirstName = "Sarah",
            LastName = "Johnson",
            Position = "Senior Developer",
            Department = "Engineering",
            YearsOfExperience = 8
        };
    }

    private Course CreateTestCourse()
    {
        return new Course
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