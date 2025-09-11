using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Core.Data;
using CareerPathRecommender.Core.Services;
using CareerPathRecommender.Core.Models;

namespace CareerPathRecommender.Tests.Services;

public class RecommendationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RecommendationService _service;

    public RecommendationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        var mockAIService = new MockAIService();
        _service = new RecommendationService(_context, mockAIService);

        SeedTestData();
    }

    [Fact]
    public async Task GenerateRecommendationsAsync_ShouldReturnRecommendations()
    {
        // Arrange
        var employeeId = 1;

        // Act
        var recommendations = await _service.GenerateRecommendationsAsync(employeeId);

        // Assert
        Assert.NotNull(recommendations);
        Assert.True(recommendations.Any());
        Assert.All(recommendations, r => Assert.True(r.Priority >= 1 && r.Priority <= 5));
        Assert.All(recommendations, r => Assert.True(r.ConfidenceScore >= 0 && r.ConfidenceScore <= 1));
    }

    [Fact]
    public async Task RecommendCoursesAsync_ShouldReturnFilteredCourses()
    {
        // Arrange
        var employeeId = 1;

        // Act
        var courses = await _service.RecommendCoursesAsync(employeeId);

        // Assert
        Assert.NotNull(courses);
        // Should not recommend courses the employee is already enrolled in
        Assert.DoesNotContain(courses, c => c.Id == 1); // Assuming employee 1 is enrolled in course 1
    }

    [Fact]
    public async Task AnalyzeSkillGapsAsync_ShouldReturnAnalysis()
    {
        // Arrange
        var employeeId = 1;
        var targetPosition = "Senior Software Engineer";

        // Act
        var analysis = await _service.AnalyzeSkillGapsAsync(employeeId, targetPosition);

        // Assert
        Assert.NotNull(analysis);
        Assert.NotNull(analysis.MissingSkills);
        Assert.NotNull(analysis.SkillsToImprove);
        Assert.True(analysis.EstimatedTimeToTargetMonths > 0);
        Assert.False(string.IsNullOrEmpty(analysis.RecommendedLearningPath));
    }

    [Theory]
    [InlineData("Senior Developer")]
    [InlineData("Tech Lead")]
    [InlineData("Engineering Manager")]
    public async Task AnalyzeSkillGapsAsync_WithDifferentTargetPositions_ShouldReturnRelevantGaps(string targetPosition)
    {
        // Arrange
        var employeeId = 1;

        // Act
        var analysis = await _service.AnalyzeSkillGapsAsync(employeeId, targetPosition);

        // Assert
        Assert.NotNull(analysis);
        if (targetPosition.Contains("Senior") || targetPosition.Contains("Lead") || targetPosition.Contains("Manager"))
        {
            Assert.Contains(analysis.MissingSkills, gap => 
                gap.SkillName.Contains("Leadership") || gap.SkillName.Contains("Management"));
        }
    }

    private void SeedTestData()
    {
        var skills = new List<Skill>
        {
            new() { Id = 1, Name = "C#", Category = "Programming" },
            new() { Id = 2, Name = "JavaScript", Category = "Programming" },
            new() { Id = 3, Name = "Leadership", Category = "Soft Skills" }
        };

        var employees = new List<Employee>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Position = "Developer", Department = "Engineering", YearsOfExperience = 3 }
        };

        var courses = new List<Course>
        {
            new() { Id = 1, Title = "Advanced C#", Provider = "Udemy", Rating = 4.5m, Price = 99.99m },
            new() { Id = 2, Title = "JavaScript Fundamentals", Provider = "freeCodeCamp", Rating = 4.3m, Price = 0m }
        };

        var employeeSkills = new List<EmployeeSkill>
        {
            new() { EmployeeId = 1, SkillId = 1, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-6) }
        };

        _context.Skills.AddRange(skills);
        _context.Employees.AddRange(employees);
        _context.Courses.AddRange(courses);
        _context.EmployeeSkills.AddRange(employeeSkills);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}