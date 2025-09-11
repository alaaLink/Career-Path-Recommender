using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Infrastructure.Services;
using Serilog;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Domain.Enums;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/career-path-recommender-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CareerPathRecommender.Web")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add custom services - using free mock AI service instead of paid Azure OpenAI
builder.Services.AddScoped<IAIService, MockAIService>();
builder.Services.AddScoped<IRecommendationService, CareerPathRecommender.Infrastructure.Services.RecommendationService>();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    
    if (!context.Employees.Any())
    {
        await SeedDataAsync(context);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

async Task SeedDataAsync(ApplicationDbContext context)
{
    // Add sample skills
    var skills = new List<Skill>
    {
        new() { Name = "C#", Category = "Programming", Description = "C# programming language" },
        new() { Name = "JavaScript", Category = "Programming", Description = "JavaScript programming" },
        new() { Name = "React", Category = "Frontend", Description = "React framework" },
        new() { Name = "ASP.NET Core", Category = "Backend", Description = "ASP.NET Core framework" },
        new() { Name = "SQL Server", Category = "Database", Description = "SQL Server database" },
        new() { Name = "Azure", Category = "Cloud", Description = "Microsoft Azure cloud platform" },
        new() { Name = "Leadership", Category = "Soft Skills", Description = "Team leadership abilities" },
        new() { Name = "Project Management", Category = "Management", Description = "Project management skills" },
        new() { Name = "Docker", Category = "DevOps", Description = "Containerization with Docker" },
        new() { Name = "Kubernetes", Category = "DevOps", Description = "Container orchestration" }
    };
    
    context.Skills.AddRange(skills);
    await context.SaveChangesAsync();
    
    // Add sample courses
    var courses = new List<Course>
    {
        new() { Title = "Advanced C# Programming", Provider = "Udemy", Category = "Programming", DurationHours = 40, Rating = 4.6m, Price = 89.99m, Url = "https://udemy.com/course/advanced-csharp", Description = "Master advanced C# concepts and design patterns" },
        new() { Title = "React for Beginners", Provider = "freeCodeCamp", Category = "Frontend", DurationHours = 25, Rating = 4.4m, Price = 0m, Url = "https://freecodecamp.org/react", Description = "Learn React fundamentals for free" },
        new() { Title = "Azure Fundamentals", Provider = "Microsoft Learn", Category = "Cloud", DurationHours = 15, Rating = 4.7m, Price = 0m, Url = "https://docs.microsoft.com/learn/azure", Description = "Free Azure certification path" },
        new() { Title = "Leadership Excellence", Provider = "Coursera", Category = "Leadership", DurationHours = 20, Rating = 4.5m, Price = 49.99m, Url = "https://coursera.org/leadership", Description = "Develop leadership and management skills" },
        new() { Title = "Docker Mastery", Provider = "YouTube", Category = "DevOps", DurationHours = 8, Rating = 4.3m, Price = 0m, Url = "https://youtube.com/docker", Description = "Free Docker tutorial series" }
    };
    
    context.Courses.AddRange(courses);
    await context.SaveChangesAsync();
    
    // Add sample employees
    var employees = new List<Employee>
    {
        new() { FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Position = "Software Developer", Department = "Engineering", YearsOfExperience = 3 },
        new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@company.com", Position = "Senior Developer", Department = "Engineering", YearsOfExperience = 7 },
        new() { FirstName = "Michael", LastName = "Smith", Email = "michael.smith@company.com", Position = "Tech Lead", Department = "Engineering", YearsOfExperience = 10 },
        new() { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@company.com", Position = "Frontend Developer", Department = "Engineering", YearsOfExperience = 4 },
        new() { FirstName = "David", LastName = "Wilson", Email = "david.wilson@company.com", Position = "DevOps Engineer", Department = "Engineering", YearsOfExperience = 6 }
    };
    
    context.Employees.AddRange(employees);
    await context.SaveChangesAsync();
    
    // Add employee skills
    var employeeSkills = new List<EmployeeSkill>
    {
        // John Doe skills
        new() { EmployeeId = 1, SkillId = 1, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-12) },
        new() { EmployeeId = 1, SkillId = 2, Level = SkillLevel.Beginner, AcquiredDate = DateTime.Now.AddMonths(-6) },
        new() { EmployeeId = 1, SkillId = 4, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-18) },
        new() { EmployeeId = 1, SkillId = 5, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-24) },
        
        // Sarah Johnson skills
        new() { EmployeeId = 2, SkillId = 1, Level = SkillLevel.Expert, AcquiredDate = DateTime.Now.AddMonths(-36) },
        new() { EmployeeId = 2, SkillId = 2, Level = SkillLevel.Advanced, AcquiredDate = DateTime.Now.AddMonths(-30) },
        new() { EmployeeId = 2, SkillId = 3, Level = SkillLevel.Advanced, AcquiredDate = DateTime.Now.AddMonths(-18) },
        new() { EmployeeId = 2, SkillId = 7, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-12) }
    };
    
    context.EmployeeSkills.AddRange(employeeSkills);
    await context.SaveChangesAsync();
    
    // Add sample projects
    var projects = new List<Project>
    {
        new() { Name = "E-commerce Platform", Description = "Build next-gen e-commerce platform using modern web technologies", StartDate = DateTime.Now.AddDays(30), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 5 },
        new() { Name = "Mobile App Redesign", Description = "Redesign company mobile application with React Native", StartDate = DateTime.Now.AddDays(15), Status = ProjectStatus.Active, Department = "Engineering", MaxTeamSize = 3 },
        new() { Name = "Cloud Migration", Description = "Migrate legacy systems to Azure cloud platform", StartDate = DateTime.Now.AddDays(45), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 4 }
    };
    
    context.Projects.AddRange(projects);
    await context.SaveChangesAsync();
    
    // Add project skills
    var projectSkills = new List<ProjectSkill>
    {
        // E-commerce Platform requirements
        new() { ProjectId = 1, SkillId = 1, RequiredLevel = SkillLevel.Advanced, IsRequired = true },
        new() { ProjectId = 1, SkillId = 3, RequiredLevel = SkillLevel.Intermediate, IsRequired = true },
        new() { ProjectId = 1, SkillId = 5, RequiredLevel = SkillLevel.Intermediate, IsRequired = false },
        
        // Mobile App Redesign requirements
        new() { ProjectId = 2, SkillId = 2, RequiredLevel = SkillLevel.Advanced, IsRequired = true },
        new() { ProjectId = 2, SkillId = 3, RequiredLevel = SkillLevel.Expert, IsRequired = true },
        
        // Cloud Migration requirements
        new() { ProjectId = 3, SkillId = 6, RequiredLevel = SkillLevel.Advanced, IsRequired = true },
        new() { ProjectId = 3, SkillId = 9, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }
    };
    
    context.ProjectSkills.AddRange(projectSkills);
    await context.SaveChangesAsync();
}

app.Run();
