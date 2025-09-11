using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Core.Models;

public class Skill
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public virtual ICollection<EmployeeSkill> EmployeeSkills { get; set; } = new List<EmployeeSkill>();
    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
}

public class EmployeeSkill
{
    public int EmployeeId { get; set; }
    public int SkillId { get; set; }
    
    public SkillLevel Level { get; set; }
    public DateTime AcquiredDate { get; set; }
    
    public virtual Employee Employee { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}

public enum SkillLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}