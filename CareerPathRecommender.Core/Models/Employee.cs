using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Core.Models;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    
    public virtual ICollection<EmployeeSkill> Skills { get; set; } = new List<EmployeeSkill>();
    public virtual ICollection<EmployeeCourse> Courses { get; set; } = new List<EmployeeCourse>();
    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
    public virtual ICollection<ProjectAssignment> ProjectAssignments { get; set; } = new List<ProjectAssignment>();
    
    public string FullName => $"{FirstName} {LastName}";
}