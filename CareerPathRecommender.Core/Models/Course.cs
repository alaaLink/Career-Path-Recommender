using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Core.Models;

public class Course
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Provider { get; set; } = string.Empty; // Udemy, Coursera, etc.
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationHours { get; set; }
    public decimal Rating { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    public virtual ICollection<EmployeeCourse> EmployeeCourses { get; set; } = new List<EmployeeCourse>();
    public virtual ICollection<Recommendation> Recommendations { get; set; } = new List<Recommendation>();
}

public class EmployeeCourse
{
    public int EmployeeId { get; set; }
    public int CourseId { get; set; }
    
    public CourseStatus Status { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? Progress { get; set; } // 0-100%
    
    public virtual Employee Employee { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}

public enum CourseStatus
{
    NotStarted,
    InProgress,
    Completed,
    Abandoned
}