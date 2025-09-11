using CareerPathRecommender.Application.DTOs;
using CareerPathRecommender.Domain.Interfaces;

namespace CareerPathRecommender.Application.UseCases;

public record GetEmployeeQuery(int EmployeeId);

public class GetEmployeeQueryHandler
{
    private readonly IEmployeeRepository _employeeRepository;

    public GetEmployeeQueryHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
    }

    public async Task<EmployeeDto?> HandleAsync(GetEmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdWithSkillsAsync(query.EmployeeId, cancellationToken);
        
        if (employee == null)
            return null;

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
}