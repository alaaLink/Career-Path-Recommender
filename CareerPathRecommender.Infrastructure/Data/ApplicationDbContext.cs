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
            .WithMany()
            .HasForeignKey(es => es.SkillId);
            
        modelBuilder.Entity<ProjectSkill>()
            .HasOne(ps => ps.Project)
            .WithMany(p => p.RequiredSkills)
            .HasForeignKey(ps => ps.ProjectId);
            
        modelBuilder.Entity<ProjectSkill>()
            .HasOne(ps => ps.Skill)
            .WithMany()
            .HasForeignKey(ps => ps.SkillId);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.Employee)
            .WithMany(e => e.Recommendations)
            .HasForeignKey(r => r.EmployeeId);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<Recommendation>()
            .HasOne(r => r.MentorEmployee)
            .WithMany()
            .HasForeignKey(r => r.MentorEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Employee)
            .WithMany()
            .HasForeignKey(pa => pa.EmployeeId);
            
        modelBuilder.Entity<ProjectAssignment>()
            .HasOne(pa => pa.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(pa => pa.ProjectId);
    }
}