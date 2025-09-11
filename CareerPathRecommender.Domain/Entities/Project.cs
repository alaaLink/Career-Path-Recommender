using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Domain.Entities;

public class Project : BaseEntity
{
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