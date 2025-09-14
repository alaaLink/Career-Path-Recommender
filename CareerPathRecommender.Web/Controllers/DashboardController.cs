using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Infrastructure.Data;
using CareerPathRecommender.Web.Models;
using CareerPathRecommender.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CareerPathRecommender.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IRecommendationService _recommendationService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IEmployeeRepository employeeRepository,
        ICourseRepository courseRepository,
        IProjectRepository projectRepository,
        IRecommendationService recommendationService, 
        ILogger<DashboardController> logger)
    {
        _employeeRepository = employeeRepository;
        _courseRepository = courseRepository;
        _projectRepository = projectRepository;
        _recommendationService = recommendationService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string searchTerm = "", int page = 1, int pageSize = 12)
    {
        try
        {
            var allEmployees = await _employeeRepository.GetAllAsync();

            // Apply search filter if search term is provided
            var filteredEmployees = allEmployees;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                filteredEmployees = allEmployees.Where(e =>
                    e.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Position.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    e.Department.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Calculate pagination
            var totalEmployees = filteredEmployees.Count();
            var paginatedEmployees = filteredEmployees
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new DashboardViewModel
            {
                Employees = paginatedEmployees,
                SearchTerm = searchTerm,
                EmployeesPagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalEmployees
                }
            };

            // Pass total count to ViewBag for search results display
            ViewBag.TotalEmployees = allEmployees.Count();

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> SelectEmployee(int employeeId, int recPage = 1, int recPageSize = 6)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var allRecommendations = await _recommendationService.GenerateRecommendationsAsync(employeeId);

            // Apply pagination to recommendations
            var totalRecommendations = allRecommendations.Count();
            var paginatedRecommendations = allRecommendations
                .Skip((recPage - 1) * recPageSize)
                .Take(recPageSize)
                .ToList();

            var model = new DashboardViewModel
            {
                SelectedEmployee = employee,
                Recommendations = paginatedRecommendations,
                RecommendationsPagination = new PaginationInfo
                {
                    CurrentPage = recPage,
                    PageSize = recPageSize,
                    TotalItems = totalRecommendations
                }
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
            await _recommendationService.AcceptRecommendationAsync(recommendationId);
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
            // Fetch the detailed recommendation data from the service
            var recommendations = await _recommendationService.GetEmployeeRecommendationsAsync(0); // This should use proper employee context
            var recommendation = recommendations.FirstOrDefault(r => r.Id == recommendationId);
            
            if (recommendation == null)
            {
                return Json(new { success = false, error = "Recommendation not found" });
            }

            var details = new
            {
                Id = recommendationId,
                Title = recommendation.Title,
                Description = recommendation.Description,
                Reasoning = recommendation.Reasoning,
                Type = recommendation.Type.ToString(),
                Priority = recommendation.Priority,
                ConfidenceScore = recommendation.ConfidenceScore,
                Details = "This recommendation is based on your current skill set and career trajectory. Consider the prerequisites and time commitment required."
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
            var totalEmployees = await _employeeRepository.GetTotalCountAsync();
            var totalCourses = await _courseRepository.GetTotalCountAsync();
            var totalProjects = await _projectRepository.GetTotalCountAsync();
            var activeCourses = 0; // This would need a specific repository method to count active enrollments

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
    public async Task<IActionResult> Recommendations(int employeeId, int recPage = 1, int recPageSize = 6)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId);

            if (employee == null)
            {
                return RedirectToAction("Index");
            }

            var allRecommendations = await _recommendationService.GenerateRecommendationsAsync(employeeId);

            // Apply pagination to recommendations
            var totalRecommendations = allRecommendations.Count();
            var paginatedRecommendations = allRecommendations
                .Skip((recPage - 1) * recPageSize)
                .Take(recPageSize)
                .ToList();

            var model = new DashboardViewModel
            {
                SelectedEmployee = employee,
                Recommendations = paginatedRecommendations,
                RecommendationsPagination = new PaginationInfo
                {
                    CurrentPage = recPage,
                    PageSize = recPageSize,
                    TotalItems = totalRecommendations
                }
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading recommendations for employee {EmployeeId}", employeeId);
            return RedirectToAction("Index");
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

            var allEmployees = await _employeeRepository.GetAllAsync();
            var employees = allEmployees
                .Where(e => e.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           e.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                           e.Position.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(e => new
                {
                    e.Id,
                    Name = e.FullName,
                    e.Position,
                    e.Department
                })
                .Take(10)
                .ToList();

            return Json(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching employees with query: {Query}", query);
            return Json(new { success = false, error = "Search failed" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateExperience(int employeeId, int yearsOfExperience)
    {
        try
        {
            if (yearsOfExperience < 0 || yearsOfExperience > 50)
            {
                return Json(new { success = false, message = "Years of experience must be between 0 and 50" });
            }

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            employee.YearsOfExperience = yearsOfExperience;
            await _employeeRepository.UpdateAsync(employee);

            _logger.LogInformation("Updated experience for employee {EmployeeId} to {YearsOfExperience} years",
                employeeId, yearsOfExperience);

            return Json(new { success = true, message = "Experience updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating experience for employee {EmployeeId}", employeeId);
            return Json(new { success = false, message = "Failed to update experience" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddEmployeeSkill(int employeeId, string skillName, string category, int level, string description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                return Json(new { success = false, message = "Skill name is required" });
            }

            var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Check if employee already has this skill
            var existingSkill = employee.Skills.FirstOrDefault(s =>
                s.Skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (existingSkill != null)
            {
                return Json(new { success = false, message = "Employee already has this skill" });
            }

            // Create or find the skill
            var skill = await GetOrCreateSkillAsync(skillName, category, description);

            // Add the skill to the employee
            var employeeSkill = new CareerPathRecommender.Domain.Entities.EmployeeSkill
            {
                EmployeeId = employeeId,
                SkillId = skill.Id,
                Level = (SkillLevel)level,
                AcquiredDate = DateTime.UtcNow
            };

            await _employeeRepository.AddEmployeeSkillAsync(employeeSkill);

            _logger.LogInformation("Added skill {SkillName} at level {Level} to employee {EmployeeId}",
                skillName, level, employeeId);

            // Return skill info for UI update
            var skillInfo = new
            {
                id = skill.Id,
                name = skillName,
                level = level,
                levelName = ((SkillLevel)level).ToString()
            };

            return Json(new { success = true, message = "Skill added successfully", skill = skillInfo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding skill {SkillName} to employee {EmployeeId}", skillName, employeeId);
            return Json(new { success = false, message = "Failed to add skill" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdatePersonalInfo(int id, string firstName, string lastName, string position, string department)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(position) || string.IsNullOrWhiteSpace(department))
            {
                return Json(new { success = false, message = "All fields are required" });
            }

            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Update employee information
            employee.FirstName = firstName;
            employee.LastName = lastName;
            employee.Position = position;
            employee.Department = department;

            await _employeeRepository.UpdateAsync(employee);

            _logger.LogInformation("Updated personal information for employee {EmployeeId}", id);

            return Json(new { success = true, message = "Personal information updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal information for employee {EmployeeId}", id);
            return Json(new { success = false, message = "Failed to update personal information" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoveEmployeeSkill(int employeeId, int skillId)
    {
        try
        {
            var employee = await _employeeRepository.GetByIdWithSkillsAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            var employeeSkill = employee.Skills.FirstOrDefault(s => s.SkillId == skillId);
            if (employeeSkill == null)
            {
                return Json(new { success = false, message = "Skill not found for this employee" });
            }

            await _employeeRepository.RemoveEmployeeSkillAsync(employeeId, skillId);

            _logger.LogInformation("Removed skill {SkillId} from employee {EmployeeId}", skillId, employeeId);

            return Json(new { success = true, message = "Skill removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing skill {SkillId} from employee {EmployeeId}", skillId, employeeId);
            return Json(new { success = false, message = "Failed to remove skill" });
        }
    }

    private async Task<CareerPathRecommender.Domain.Entities.Skill> GetOrCreateSkillAsync(string name, string category, string description)
    {
        // Try to find existing skill
        var existingSkill = await _employeeRepository.GetSkillByNameAsync(name);
        if (existingSkill != null)
        {
            return existingSkill;
        }

        // Create new skill
        var newSkill = new CareerPathRecommender.Domain.Entities.Skill
        {
            Name = name,
            Category = category,
            Description = description ?? $"{name} skill"
        };

        return await _employeeRepository.CreateSkillAsync(newSkill);
    }
}