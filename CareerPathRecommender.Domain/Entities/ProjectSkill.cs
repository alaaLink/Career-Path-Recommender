using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Domain.Entities;

public class ProjectSkill : BaseEntity
{
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public SkillLevel RequiredLevel { get; set; }
    public bool IsRequired { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}