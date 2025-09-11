using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Application.DTOs;

public record EmployeeDto
{
    public int Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; init; } = string.Empty;
    public string Position { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public int YearsOfExperience { get; init; }
    public List<EmployeeSkillDto> Skills { get; init; } = new();
}

public record EmployeeSkillDto
{
    public int Id { get; init; }
    public int SkillId { get; init; }
    public SkillDto Skill { get; init; } = new();
    public SkillLevel Level { get; init; }
    public DateTime AcquiredDate { get; init; }
}

public record SkillDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}