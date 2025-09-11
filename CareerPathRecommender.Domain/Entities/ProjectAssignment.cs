namespace CareerPathRecommender.Domain.Entities;

public class ProjectAssignment : BaseEntity
{
    public int ProjectId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime AssignedDate { get; set; }

    public virtual Project Project { get; set; } = null!;
    public virtual Employee Employee { get; set; } = null!;
}