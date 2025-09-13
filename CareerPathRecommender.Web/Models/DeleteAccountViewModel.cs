using System.ComponentModel.DataAnnotations;

namespace CareerPathRecommender.Web.Models;

public class DeleteAccountViewModel
{
    [Required(ErrorMessage = "Password is required to delete your account")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "You must confirm account deletion")]
    [Display(Name = "Confirm Deletion")]
    public bool ConfirmDeletion { get; set; }

    [Required(ErrorMessage = "Please type 'DELETE' to confirm")]
    [Display(Name = "Type DELETE to confirm")]
    [Compare("DeleteConfirmation", ErrorMessage = "Please type 'DELETE' exactly as shown")]
    public string ConfirmationText { get; set; } = string.Empty;

    public string DeleteConfirmation => "DELETE";
}
