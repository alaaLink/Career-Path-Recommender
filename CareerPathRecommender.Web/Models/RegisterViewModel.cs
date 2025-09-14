using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Position is required")]
    [Display(Name = "Position")]
    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [Display(Name = "Department")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Years of experience is required")]
    [Display(Name = "Years of Experience")]
    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    public int? YearsOfExperience { get; set; }

    [Required(ErrorMessage = "You must accept the terms and conditions")]
    [Display(Name = "Accept Terms")]
    public bool AcceptTerms { get; set; }
}
