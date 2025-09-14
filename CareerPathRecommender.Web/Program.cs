using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CareerPathRecommender.Infrastructure.Repositories;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Infrastructure.Services;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Web.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/career-path-recommender-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("CareerPathRecommender.Infrastructure"));
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableServiceProviderCaching(false); // Disable caching to avoid threading issues
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add OAuth authentication
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    });
    // Microsoft OAuth temporarily disabled - uncomment when credentials are ready
    //.AddMicrosoftAccount(options =>
    //{
    //    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? "";
    //    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? "";
    //});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add memory cache for performance optimization
builder.Services.AddMemoryCache();

// Register repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

// Register services with caching decorator pattern
builder.Services.AddScoped<CareerPathRecommender.Infrastructure.Services.RecommendationService>();
builder.Services.AddScoped<CareerPathRecommender.Application.Interfaces.IRecommendationService>(provider =>
{
    var innerService = provider.GetRequiredService<CareerPathRecommender.Infrastructure.Services.RecommendationService>();
    var cache = provider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
    var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CareerPathRecommender.Infrastructure.Services.CachedRecommendationService>>();
    return new CareerPathRecommender.Infrastructure.Services.CachedRecommendationService(innerService, cache, logger);
});
builder.Services.AddScoped<IAIService, CareerPathRecommender.Infrastructure.Services.MockAIService>();

// Configure Mailjet settings
builder.Services.Configure<MailjetSettings>(builder.Configuration.GetSection("Mailjet"));
builder.Services.AddScoped<IEmailService, MailjetEmailService>();

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
        await DataSeeder.SeedDataAsync(employeeRepository, courseRepository, projectRepository, skillRepository, context);
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
    pattern: "{controller=Account}/{action=Login}/{id?}");


app.Run();
