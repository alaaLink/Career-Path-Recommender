using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Domain.Entities;

public class EmployeeSkill : BaseEntity
{
    public int EmployeeId { get; set; }
    public int SkillId { get; set; }
    public SkillLevel Level { get; set; }
    public DateTime AcquiredDate { get; set; }

    public virtual Employee Employee { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}