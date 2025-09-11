namespace CareerPathRecommender.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}