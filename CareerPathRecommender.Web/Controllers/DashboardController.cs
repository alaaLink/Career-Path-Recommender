using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, IRecommendationService recommendationService, ILogger<DashboardController> logger)
    {
        _context = context;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var employees = await _context.Employees.ToListAsync();
            var model = new DashboardViewModel
            {
                Employees = employees
            };
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SelectEmployee(int employeeId)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Skills)
                .ThenInclude(s => s.Skill)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var recommendations = await _recommendationService.GenerateRecommendationsAsync(employeeId);

            var model = new DashboardViewModel
            {
                SelectedEmployee = employee,
                Recommendations = recommendations.ToList()
            };

            return PartialView("_EmployeeProfile", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting employee {EmployeeId}", employeeId);
            return Json(new { success = false, error = "Failed to load employee data" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AnalyzeSkillGaps(int employeeId, string targetPosition)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(targetPosition))
            {
                return Json(new { success = false, error = "Target position is required" });
            }

            var analysis = await _recommendationService.AnalyzeSkillGapsAsync(employeeId, targetPosition);

            return PartialView("_SkillGapAnalysis", analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing skill gaps for employee {EmployeeId}", employeeId);
            return Json(new { success = false, error = "Failed to analyze skill gaps" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AcceptRecommendation(int recommendationId)
    {
        try
        {
            // In a real application, we would update the recommendation status in the database
            _logger.LogInformation("Recommendation {RecommendationId} accepted", recommendationId);
            
            return Json(new { success = true, message = "Recommendation accepted! You'll receive follow-up information shortly." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting recommendation {RecommendationId}", recommendationId);
            return Json(new { success = false, error = "Failed to accept recommendation" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRecommendationDetails(int recommendationId)
    {
        try
        {
            // In a real application, we would fetch detailed recommendation data
            var details = new
            {
                Id = recommendationId,
                Details = "This is a detailed view of the recommendation with additional information, prerequisites, and action items."
            };

            return Json(new { success = true, data = details });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommendation details for {RecommendationId}", recommendationId);
            return Json(new { success = false, error = "Failed to load recommendation details" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeeMetrics()
    {
        try
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var activeCourses = await _context.EmployeeCourses
                .Where(ec => ec.Status == CourseStatus.InProgress)
                .CountAsync();

            var metrics = new
            {
                TotalEmployees = totalEmployees,
                TotalCourses = totalCourses,
                TotalProjects = totalProjects,
                ActiveCourses = activeCourses
            };

            return Json(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading employee metrics");
            return Json(new { success = false, error = "Failed to load metrics" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchEmployees(string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<object>());
            }

            var employees = await _context.Employees
                .Where(e => e.FirstName.Contains(query) || e.LastName.Contains(query) || e.Position.Contains(query))
                .Select(e => new
                {
                    e.Id,
                    Name = e.FullName,
                    e.Position,
                    e.Department
                })
                .Take(10)
                .ToListAsync();

            return Json(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with query: {Query}", query);
            return Json(new { success = false, error = "Search failed" });
        }
    }
}