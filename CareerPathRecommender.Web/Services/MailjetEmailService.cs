using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Options;

namespace CareerPathRecommender.Web.Services
{
    public class MailjetEmailService : IEmailService
    {
        private readonly MailjetClient _client;
        private readonly MailjetSettings _settings;
        private readonly ILogger<MailjetEmailService> _logger;
        private readonly IWebHostEnvironment _environment;

        public MailjetEmailService(IOptions<MailjetSettings> settings, ILogger<MailjetEmailService> logger, IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _logger = logger;
            _environment = environment;
            _client = new MailjetClient(_settings.ApiKey, _settings.ApiSecret);
        }

        public async Task<bool> SendEmailConfirmationAsync(string email, string confirmationToken, string callbackUrl)
        {
            try
            {
                _logger.LogInformation("SendEmailConfirmationAsync called for {Email} with URL: {CallbackUrl}", email, callbackUrl);
                
                var subject = "Confirm Your Email - Career Path Recommender";
                
                // Load HTML template
                var htmlTemplate = await LoadEmailTemplateAsync("EmailConfirmationTemplate.html");
                var htmlContent = htmlTemplate.Replace("{{CONFIRMATION_URL}}", callbackUrl);
                _logger.LogInformation("HTML template loaded and URL replaced for {Email}", email);
                
                // Load text template
                var textTemplate = await LoadEmailTemplateAsync("EmailConfirmationTemplate.txt");
                var textContent = textTemplate.Replace("{{CONFIRMATION_URL}}", callbackUrl);
                _logger.LogInformation("Text template loaded and URL replaced for {Email}", email);

                var result = await SendEmailAsync(email, subject, htmlContent, textContent);
                _logger.LogInformation("SendEmailAsync result for {Email}: {Result}", email, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in SendEmailConfirmationAsync for {Email}", email);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl)
        {
            var subject = "Reset Your Password - Career Path Recommender";
            
            // Load HTML template
            var htmlTemplate = await LoadEmailTemplateAsync("PasswordResetEmailTemplate.html");
            var htmlContent = htmlTemplate.Replace("{{CALLBACK_URL}}", callbackUrl);
            
            // Load text template
            var textTemplate = await LoadEmailTemplateAsync("PasswordResetEmailTemplate.txt");
            var textContent = textTemplate.Replace("{{CALLBACK_URL}}", callbackUrl);

            return await SendEmailAsync(email, subject, htmlContent, textContent);
        }

        private async Task<string> LoadEmailTemplateAsync(string templateName)
        {
            try
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", templateName);
                
                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Email template not found at {TemplatePath}", templatePath);
                    return GetFallbackTemplate(templateName);
                }

                return await File.ReadAllTextAsync(templatePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template {TemplateName}", templateName);
                return GetFallbackTemplate(templateName);
            }
        }

        private string GetFallbackTemplate(string templateName)
        {
            if (templateName.Contains("EmailConfirmation"))
            {
                if (templateName.EndsWith(".html"))
                {
                    return @"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h1>Confirm Your Email</h1>
                        <p>Please click the link below to confirm your email address:</p>
                        <a href='{{CONFIRMATION_URL}}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
                        <p>If you didn't create an account, please ignore this email.</p>
                    </div>";
                }
                else
                {
                    return @"
                    Email Confirmation Required

                    Please click the link below to confirm your email address:
                    {{CONFIRMATION_URL}}

                    If you didn't create an account, please ignore this email.";
                }
            }
            else if (templateName.Contains("PasswordReset"))
            {
                if (templateName.EndsWith(".html"))
                {
                    return @"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h1>Password Reset Request</h1>
                        <p>Click the link below to reset your password:</p>
                        <a href='{{CALLBACK_URL}}'>Reset Password</a>
                        <p>If you didn't request this, please ignore this email.</p>
                    </div>";
                }
                else
                {
                    return @"
                    Password Reset Request

                    Click the link below to reset your password:
                    {{CALLBACK_URL}}

                    If you didn't request this, please ignore this email.";
                }
            }
            
            // Generic fallback
            return "Email template not found.";
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string textContent = "")
        {
            try
            {
                _logger.LogInformation("Attempting to send email to {Email} with subject: {Subject}", to, subject);
                _logger.LogInformation("Mailjet settings - FromEmail: {FromEmail}, FromName: {FromName}", _settings.FromEmail, _settings.FromName);
                
                var email = new TransactionalEmailBuilder()
                    .WithFrom(new SendContact(_settings.FromEmail, _settings.FromName))
                    .WithTo(new SendContact(to))
                    .WithSubject(subject)
                    .WithHtmlPart(htmlContent)
                    .WithTextPart(string.IsNullOrEmpty(textContent) ? "" : textContent)
                    .Build();

                _logger.LogInformation("Email object built, sending via Mailjet...");
                var response = await _client.SendTransactionalEmailAsync(email);
                _logger.LogInformation("Mailjet response received for {Email}", to);
                
                if (response.Messages != null && response.Messages.Any())
                {
                    var message = response.Messages.First();
                    _logger.LogInformation("Message status: {Status} for {Email}", message.Status, to);
                    
                    if (message.Status == "success")
                    {
                        _logger.LogInformation("Email sent successfully to {Email}", to);
                        return true;
                    }
                    else
                    {
                        var errorMessage = message.Errors?.FirstOrDefault()?.ErrorMessage ?? "Unknown error";
                        _logger.LogError("Failed to send email to {Email}. Status: {Status}, Error: {Error}", 
                            to, message.Status, errorMessage);
                        return false;
                    }
                }
                
                _logger.LogError("No response messages received from Mailjet for email to {Email}", to);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {Email}", to);
                return false;
            }
        }
    }

    public class MailjetSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}
