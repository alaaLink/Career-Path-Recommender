using System;
using CareerPathRecommender.Domain.Enums;

namespace CareerPathRecommender.Domain.Entities;

public class EmployeeCourse
{
    public int EmployeeId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? Progress { get; set; }
    public CourseStatus Status { get; set; }
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
    public virtual Course Course { get; set; } = null!;
}
