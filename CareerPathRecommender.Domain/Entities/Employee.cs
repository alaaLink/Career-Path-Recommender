namespace CareerPathRecommender.Domain.Entities;

public class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    public virtual ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public virtual ICollection<EmployeeCourse> Courses { get; set; } = new List<EmployeeCourse>();
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
}