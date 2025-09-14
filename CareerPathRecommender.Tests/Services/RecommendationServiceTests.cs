using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Infrastructure.Services;
using CareerPathRecommender.Infrastructure.Repositories;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;

namespace CareerPathRecommender.Tests.Services;

public class RecommendationServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RecommendationService _service;
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ILogger<RecommendationService>> _mockLogger;

    public RecommendationServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockAIService = new Mock<IAIService>();
        _mockLogger = new Mock<ILogger<RecommendationService>>();

        var employeeRepo = new EmployeeRepository(_context);
        var courseRepo = new CourseRepository(_context);
        var projectRepo = new ProjectRepository(_context);
        var recommendationRepo = new RecommendationRepository(_context);

        _service = new RecommendationService(employeeRepo, courseRepo, projectRepo, recommendationRepo, _mockAIService.Object, _mockLogger.Object);

        SeedTestData();
        SetupMockAIService();
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
        _mockAIService.Verify(x => x.GenerateRecommendationReasoningAsync(It.IsAny<CareerPathRecommender.Application.DTOs.EmployeeDto>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetEmployeeRecommendationsAsync_ShouldReturnEmployeeRecommendations()
    {
        // Arrange
        var employeeId = 1;

        // First generate some recommendations
        await _service.GenerateRecommendationsAsync(employeeId);

        // Act
        var recommendations = await _service.GetEmployeeRecommendationsAsync(employeeId);

        // Assert
        Assert.NotNull(recommendations);
        Assert.All(recommendations, r => Assert.Equal(employeeId, r.EmployeeId));
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
            new() { Name = "C#", Category = "Programming", Description = "C# programming language" },
            new() { Name = "JavaScript", Category = "Programming", Description = "JavaScript programming" },
            new() { Name = "Leadership", Category = "Soft Skills", Description = "Leadership abilities" }
        };

        var employees = new List<Employee>
        {
            new() { FirstName = "John", LastName = "Doe", Email = "john.doe@test.com", Position = "Developer", Department = "Engineering", YearsOfExperience = 3 }
        };

        var courses = new List<Course>
        {
            new() { Title = "Advanced C#", Provider = "Udemy", Category = "Programming", DurationHours = 40, Rating = 4.5m, Price = 99.99m, Url = "test.com", Description = "Advanced C# course" },
            new() { Title = "JavaScript Fundamentals", Provider = "freeCodeCamp", Category = "Programming", DurationHours = 30, Rating = 4.3m, Price = 0m, Url = "test.com", Description = "JS fundamentals" }
        };

        _context.Skills.AddRange(skills);
        _context.Employees.AddRange(employees);
        _context.Courses.AddRange(courses);
        _context.SaveChanges();

        var employeeSkills = new List<EmployeeSkill>
        {
            new() { EmployeeId = employees[0].Id, SkillId = skills[0].Id, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.UtcNow.AddMonths(-6) }
        };

        _context.EmployeeSkills.AddRange(employeeSkills);
        _context.SaveChanges();
    }

    private void SetupMockAIService()
    {
        _mockAIService.Setup(x => x.GenerateRecommendationReasoningAsync(
            It.IsAny<CareerPathRecommender.Application.DTOs.EmployeeDto>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is a mock recommendation reasoning.");

        _mockAIService.Setup(x => x.GenerateMentorMatchReasoningAsync(
            It.IsAny<CareerPathRecommender.Application.DTOs.EmployeeDto>(), It.IsAny<CareerPathRecommender.Application.DTOs.EmployeeDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is a mock mentor match reasoning.");

        _mockAIService.Setup(x => x.GenerateProjectMatchReasoningAsync(
            It.IsAny<CareerPathRecommender.Application.DTOs.EmployeeDto>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("This is a mock project match reasoning.");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}