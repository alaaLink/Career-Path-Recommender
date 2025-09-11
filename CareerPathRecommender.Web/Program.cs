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
        b => b.MigrationsAssembly("CareerPathRecommender.Infrastructure")));

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

// Add repository services
builder.Services.AddScoped<IEmployeeRepository, CareerPathRecommender.Infrastructure.Repositories.EmployeeRepository>();
builder.Services.AddScoped<ICourseRepository, CareerPathRecommender.Infrastructure.Repositories.CourseRepository>();
builder.Services.AddScoped<IProjectRepository, CareerPathRecommender.Infrastructure.Repositories.ProjectRepository>();
builder.Services.AddScoped<IRecommendationRepository, CareerPathRecommender.Infrastructure.Repositories.RecommendationRepository>();
builder.Services.AddScoped<ISkillRepository, CareerPathRecommender.Infrastructure.Repositories.SkillRepository>();

// Add custom services - using free mock AI service instead of paid Azure OpenAI
builder.Services.AddScoped<IAIService, MockAIService>();
builder.Services.AddScoped<IRecommendationService, CareerPathRecommender.Infrastructure.Services.RecommendationService>();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
    var courseRepository = scope.ServiceProvider.GetRequiredService<ICourseRepository>();
    var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
    var skillRepository = scope.ServiceProvider.GetRequiredService<ISkillRepository>();
    
    await context.Database.MigrateAsync();
    
    var totalEmployees = await employeeRepository.GetTotalCountAsync();
    if (totalEmployees == 0)
    {
        await SeedDataAsync(employeeRepository, courseRepository, projectRepository, skillRepository);
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

async Task SeedDataAsync(IEmployeeRepository employeeRepository, ICourseRepository courseRepository, IProjectRepository projectRepository, ISkillRepository skillRepository)
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
    
    var savedSkills = new List<Skill>();
    foreach (var skill in skills)
    {
        var savedSkill = await skillRepository.AddAsync(skill);
        savedSkills.Add(savedSkill);
    }
    
    // Add sample courses
    var courses = new List<Course>
    {
        new() { Title = "Advanced C# Programming", Provider = "Udemy", Category = "Programming", DurationHours = 40, Rating = 4.6m, Price = 89.99m, Url = "https://udemy.com/course/advanced-csharp", Description = "Master advanced C# concepts and design patterns" },
        new() { Title = "React for Beginners", Provider = "freeCodeCamp", Category = "Frontend", DurationHours = 25, Rating = 4.4m, Price = 0m, Url = "https://freecodecamp.org/react", Description = "Learn React fundamentals for free" },
        new() { Title = "Azure Fundamentals", Provider = "Microsoft Learn", Category = "Cloud", DurationHours = 15, Rating = 4.7m, Price = 0m, Url = "https://docs.microsoft.com/learn/azure", Description = "Free Azure certification path" },
        new() { Title = "Leadership Excellence", Provider = "Coursera", Category = "Leadership", DurationHours = 20, Rating = 4.5m, Price = 49.99m, Url = "https://coursera.org/leadership", Description = "Develop leadership and management skills" },
        new() { Title = "Docker Mastery", Provider = "YouTube", Category = "DevOps", DurationHours = 8, Rating = 4.3m, Price = 0m, Url = "https://youtube.com/docker", Description = "Free Docker tutorial series" }
    };
    
    var savedCourses = new List<Course>();
    foreach (var course in courses)
    {
        var savedCourse = await courseRepository.AddAsync(course);
        savedCourses.Add(savedCourse);
    }
    
    // Add sample employees
    var employees = new List<Employee>
    {
        new() { FirstName = "John", LastName = "Doe", Email = "john.doe@company.com", Position = "Software Developer", Department = "Engineering", YearsOfExperience = 3 },
        new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@company.com", Position = "Senior Developer", Department = "Engineering", YearsOfExperience = 7 },
        new() { FirstName = "Michael", LastName = "Smith", Email = "michael.smith@company.com", Position = "Tech Lead", Department = "Engineering", YearsOfExperience = 10 },
        new() { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@company.com", Position = "Frontend Developer", Department = "Engineering", YearsOfExperience = 4 },
        new() { FirstName = "David", LastName = "Wilson", Email = "david.wilson@company.com", Position = "DevOps Engineer", Department = "Engineering", YearsOfExperience = 6 }
    };
    
    var savedEmployees = new List<Employee>();
    foreach (var employee in employees)
    {
        var savedEmployee = await employeeRepository.AddAsync(employee);
        savedEmployees.Add(savedEmployee);
    }
    
    // Add employee skills using actual saved IDs
    var employeeSkills = new List<EmployeeSkill>
    {
        // John Doe (savedEmployees[0]) skills
        new() { EmployeeId = savedEmployees[0].Id, SkillId = savedSkills[0].Id, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-12) }, // C#
        new() { EmployeeId = savedEmployees[0].Id, SkillId = savedSkills[1].Id, Level = SkillLevel.Beginner, AcquiredDate = DateTime.Now.AddMonths(-6) }, // JavaScript
        new() { EmployeeId = savedEmployees[0].Id, SkillId = savedSkills[3].Id, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-18) }, // ASP.NET Core
        new() { EmployeeId = savedEmployees[0].Id, SkillId = savedSkills[4].Id, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-24) }, // SQL Server
        
        // Sarah Johnson (savedEmployees[1]) skills
        new() { EmployeeId = savedEmployees[1].Id, SkillId = savedSkills[0].Id, Level = SkillLevel.Expert, AcquiredDate = DateTime.Now.AddMonths(-36) }, // C#
        new() { EmployeeId = savedEmployees[1].Id, SkillId = savedSkills[1].Id, Level = SkillLevel.Advanced, AcquiredDate = DateTime.Now.AddMonths(-30) }, // JavaScript
        new() { EmployeeId = savedEmployees[1].Id, SkillId = savedSkills[2].Id, Level = SkillLevel.Advanced, AcquiredDate = DateTime.Now.AddMonths(-18) }, // React
        new() { EmployeeId = savedEmployees[1].Id, SkillId = savedSkills[6].Id, Level = SkillLevel.Intermediate, AcquiredDate = DateTime.Now.AddMonths(-12) } // Leadership
    };
    
    // Note: EmployeeSkills is a junction table - would need IEmployeeSkillRepository for full async pattern
    // For now, using context directly since it's a simple many-to-many relationship
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.EmployeeSkills.AddRange(employeeSkills);
    await context.SaveChangesAsync();
    
    // Add sample projects
    var projects = new List<Project>
    {
        new() { Name = "E-commerce Platform", Description = "Build next-gen e-commerce platform using modern web technologies", StartDate = DateTime.Now.AddDays(30), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 5 },
        new() { Name = "Mobile App Redesign", Description = "Redesign company mobile application with React Native", StartDate = DateTime.Now.AddDays(15), Status = ProjectStatus.Active, Department = "Engineering", MaxTeamSize = 3 },
        new() { Name = "Cloud Migration", Description = "Migrate legacy systems to Azure cloud platform", StartDate = DateTime.Now.AddDays(45), Status = ProjectStatus.Planning, Department = "Engineering", MaxTeamSize = 4 }
    };
    
    var savedProjects = new List<Project>();
    foreach (var project in projects)
    {
        var savedProject = await projectRepository.AddAsync(project);
        savedProjects.Add(savedProject);
    }
    
    // Add project skills using actual saved IDs
    var projectSkills = new List<ProjectSkill>
    {
        // E-commerce Platform requirements
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[0].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // C#
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[2].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = true }, // React
        new() { ProjectId = savedProjects[0].Id, SkillId = savedSkills[4].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false }, // SQL Server
        
        // Mobile App Redesign requirements
        new() { ProjectId = savedProjects[1].Id, SkillId = savedSkills[1].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // JavaScript
        new() { ProjectId = savedProjects[1].Id, SkillId = savedSkills[2].Id, RequiredLevel = SkillLevel.Expert, IsRequired = true }, // React
        
        // Cloud Migration requirements
        new() { ProjectId = savedProjects[2].Id, SkillId = savedSkills[5].Id, RequiredLevel = SkillLevel.Advanced, IsRequired = true }, // Azure
        new() { ProjectId = savedProjects[2].Id, SkillId = savedSkills[8].Id, RequiredLevel = SkillLevel.Intermediate, IsRequired = false } // Docker
    };
    
    // ProjectSkills is also a junction table - using context directly
    context.ProjectSkills.AddRange(projectSkills);
    await context.SaveChangesAsync();
}

app.Run();
