namespace CareerPathRecommender.Web.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string callbackUrl);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl);
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = "");
    }
}
