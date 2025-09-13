using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Application.DTOs;

namespace CareerPathRecommender.Web.Models;

public class DashboardViewModel
{
    public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
    public Employee? SelectedEmployee { get; set; }
    public List<RecommendationDto> Recommendations { get; set; } = new();
    public PaginationInfo RecommendationsPagination { get; set; } = new();
    public string TargetPosition { get; set; } = string.Empty;
    public string SearchTerm { get; set; } = string.Empty;
    public PaginationInfo EmployeesPagination { get; set; } = new();
}