using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Core.Models;

public class Project
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public string Department { get; set; } = string.Empty;
    public int MaxTeamSize { get; set; }
    
    public virtual ICollection<ProjectSkill> RequiredSkills { get; set; } = new List<ProjectSkill>();
    public virtual ICollection<ProjectAssignment> Assignments { get; set; } = new List<ProjectAssignment>();
}

public class ProjectSkill
{
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    
    public SkillLevel RequiredLevel { get; set; }
    public bool IsRequired { get; set; }
    
    public virtual Project Project { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}

public class ProjectAssignment
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    
    public string Role { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public int MatchScore { get; set; } // 0-100%
    
    public virtual Employee Employee { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
}

public enum ProjectStatus
{
    Planning,
    Active,
    OnHold,
    Completed,
    Cancelled
}