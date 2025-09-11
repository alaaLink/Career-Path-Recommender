using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; }
    public DbSet<ProjectSkill> ProjectSkills { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<EmployeeCourse> EmployeeCourses { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships
        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Employee)
            .WithMany(e => e.Skills)
            .HasForeignKey(es => es.EmployeeId);
            
        modelBuilder.Entity<EmployeeSkill>()
            .HasOne(es => es.Skill)
            .WithMany(s => s.EmployeeSkills)
            .HasForeignKey(es => es.SkillId);
            
        modelBuilder.Entity<ProjectSkill>()
            .HasOne(ps => ps.Project)
            .WithMany(p => p.RequiredSkills)
            .HasForeignKey(ps => ps.ProjectId);
            
        modelBuilder.Entity<ProjectSkill>()
            .HasOne(ps => ps.Skill)
            .WithMany(s => s.ProjectSkills)
            .HasForeignKey(ps => ps.SkillId);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.Employee)
            .WithMany(e => e.Recommendations)
            .HasForeignKey(r => r.EmployeeId);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.Course)
            .WithMany(c => c.Recommendations)
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.MentorEmployee)
            .WithMany()
            .HasForeignKey(r => r.MentorEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Employee)
            .WithMany(e => e.ProjectAssignments)
            .HasForeignKey(pa => pa.EmployeeId);
            
        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(pa => pa.ProjectId);
            
        modelBuilder.Entity<EmployeeCourse>()
            .HasKey(ec => new { ec.EmployeeId, ec.CourseId });
            
        modelBuilder.Entity<EmployeeCourse>()
            .HasOne(ec => ec.Employee)
            .WithMany(e => e.Courses)
            .HasForeignKey(ec => ec.EmployeeId);
            
        modelBuilder.Entity<EmployeeCourse>()
            .HasOne(ec => ec.Course)
            .WithMany(c => c.EmployeeCourses)
            .HasForeignKey(ec => ec.CourseId);
    }
}