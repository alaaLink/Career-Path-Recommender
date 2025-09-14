using Xunit;
using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Infrastructure.Repositories;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Tests.Repositories;

public class EmployeeRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly EmployeeRepository _repository;

    public EmployeeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new EmployeeRepository(_context);

        SeedTestData();
    }

    [Fact]
    public async Task GetSkillByNameAsync_ShouldReturnSkill_WhenSkillExists()
    {
        // Arrange
        var skillName = "C#";

        // Act
        var skill = await _repository.GetSkillByNameAsync(skillName);

        // Assert
        Assert.NotNull(skill);
        Assert.Equal(skillName, skill.Name);
    }

    [Fact]
    public async Task GetSkillByNameAsync_ShouldReturnNull_WhenSkillDoesNotExist()
    {
        // Arrange
        var skillName = "NonExistentSkill";

        // Act
        var skill = await _repository.GetSkillByNameAsync(skillName);

        // Assert
        Assert.Null(skill);
    }

    [Fact]
    public async Task GetSkillByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var skillName = "c#"; // lowercase

        // Act
        var skill = await _repository.GetSkillByNameAsync(skillName);

        // Assert
        Assert.NotNull(skill);
        Assert.Equal("C#", skill.Name);
    }

    [Fact]
    public async Task CreateSkillAsync_ShouldCreateNewSkill()
    {
        // Arrange
        var newSkill = new Skill
        {
            Name = "Python",
            Category = "Programming",
            Description = "Python programming language"
        };

        // Act
        var createdSkill = await _repository.CreateSkillAsync(newSkill);

        // Assert
        Assert.NotNull(createdSkill);
        Assert.True(createdSkill.Id > 0);
        Assert.Equal("Python", createdSkill.Name);

        // Verify it's in database
        var skillInDb = await _context.Skills.FindAsync(createdSkill.Id);
        Assert.NotNull(skillInDb);
        Assert.Equal("Python", skillInDb.Name);
    }

    [Fact]
    public async Task AddEmployeeSkillAsync_ShouldAddSkillToEmployee()
    {
        // Arrange
        var employee = await _context.Employees.FirstAsync();
        var skill = await _context.Skills.FirstAsync(s => s.Name == "JavaScript");

        var employeeSkill = new EmployeeSkill
        {
            EmployeeId = employee.Id,
            SkillId = skill.Id,
            Level = SkillLevel.Intermediate,
            AcquiredDate = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddEmployeeSkillAsync(employeeSkill);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employee.Id, result.EmployeeId);
        Assert.Equal(skill.Id, result.SkillId);

        // Verify it's in database
        var employeeSkillInDb = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == employee.Id && es.SkillId == skill.Id);
        Assert.NotNull(employeeSkillInDb);
        Assert.Equal(SkillLevel.Intermediate, employeeSkillInDb.Level);
    }

    [Fact]
    public async Task RemoveEmployeeSkillAsync_ShouldRemoveSkillFromEmployee()
    {
        // Arrange
        var employee = await _context.Employees.FirstAsync();
        var skill = await _context.Skills.FirstAsync(s => s.Name == "C#");

        // Add a skill first
        var employeeSkill = new EmployeeSkill
        {
            EmployeeId = employee.Id,
            SkillId = skill.Id,
            Level = SkillLevel.Advanced,
            AcquiredDate = DateTime.UtcNow
        };
        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RemoveEmployeeSkillAsync(employee.Id, skill.Id);

        // Assert
        var removedSkill = await _context.EmployeeSkills
            .FirstOrDefaultAsync(es => es.EmployeeId == employee.Id && es.SkillId == skill.Id);
        Assert.Null(removedSkill);
    }

    [Fact]
    public async Task RemoveEmployeeSkillAsync_ShouldNotThrow_WhenSkillDoesNotExist()
    {
        // Arrange
        var employee = await _context.Employees.FirstAsync();
        var nonExistentSkillId = 999;

        // Act & Assert - should not throw
        await _repository.RemoveEmployeeSkillAsync(employee.Id, nonExistentSkillId);
    }

    [Fact]
    public async Task GetByIdWithSkillsAsync_ShouldIncludeEmployeeSkills()
    {
        // Arrange
        var employee = await _context.Employees.FirstAsync();
        var skill = await _context.Skills.FirstAsync();

        // Add a skill to employee
        var employeeSkill = new EmployeeSkill
        {
            EmployeeId = employee.Id,
            SkillId = skill.Id,
            Level = SkillLevel.Beginner,
            AcquiredDate = DateTime.UtcNow
        };
        _context.EmployeeSkills.Add(employeeSkill);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdWithSkillsAsync(employee.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Skills);
        Assert.Single(result.Skills);
        Assert.Equal(skill.Id, result.Skills.First().SkillId);
        Assert.NotNull(result.Skills.First().Skill);
        Assert.Equal(skill.Name, result.Skills.First().Skill.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateYearsOfExperience()
    {
        // Arrange
        var employee = await _context.Employees.FirstAsync();
        var originalExperience = employee.YearsOfExperience;
        employee.YearsOfExperience = originalExperience + 2;

        // Act
        var updatedEmployee = await _repository.UpdateAsync(employee);

        // Assert
        Assert.Equal(originalExperience + 2, updatedEmployee.YearsOfExperience);

        // Verify in database
        var employeeInDb = await _context.Employees.FindAsync(employee.Id);
        Assert.Equal(originalExperience + 2, employeeInDb!.YearsOfExperience);
    }

    private void SeedTestData()
    {
        var skills = new List<Skill>
        {
            new() { Name = "C#", Category = "Programming", Description = "C# programming language" },
            new() { Name = "JavaScript", Category = "Programming", Description = "JavaScript programming" },
            new() { Name = "Leadership", Category = "Soft Skills", Description = "Leadership abilities" },
            new() { Name = "Project Management", Category = "Management", Description = "Project management skills" }
        };

        var employees = new List<Employee>
        {
            new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                Position = "Developer",
                Department = "Engineering",
                YearsOfExperience = 3
            },
            new()
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@test.com",
                Position = "Senior Developer",
                Department = "Engineering",
                YearsOfExperience = 8
            }
        };

        _context.Skills.AddRange(skills);
        _context.Employees.AddRange(employees);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}