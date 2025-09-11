using CareerPathRecommender.Domain.Entities;

namespace CareerPathRecommender.Web.Models;

public class DashboardViewModel
{
    public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
    public Employee? SelectedEmployee { get; set; }
    public List<Recommendation> Recommendations { get; set; } = new();
    public string TargetPosition { get; set; } = string.Empty;
}