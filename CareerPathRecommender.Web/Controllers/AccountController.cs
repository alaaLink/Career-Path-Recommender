using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CareerPathRecommender.Web.Models;
using CareerPathRecommender.Domain.Entities;
using CareerPathRecommender.Application.Interfaces;
using CareerPathRecommender.Web.Services;
using System.Security.Claims;

namespace CareerPathRecommender.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IEmployeeRepository employeeRepository,
        IEmailService emailService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _employeeRepository = employeeRepository;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["ReturnUrl"] = model.ReturnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully", model.Email);
                
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                
                return RedirectToAction("Index", "Dashboard");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("LoginWith2fa", new { ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account locked out", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    _logger.LogInformation("Setting ViewBag.ShowResendConfirmation = true for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "You must confirm your email before you can log in. Please check your email for the confirmation link.");
                    ViewBag.ShowResendConfirmation = true;
                    ViewBag.Email = model.Email;
                   // _logger.LogInformation("ViewBag values set - ShowResendConfirmation: {Show}, Email: {Email}", ViewBag.ShowResendConfirmation, ViewBag.Email);
                    return View(model);
                }
                ModelState.AddModelError(string.Empty, "Your account is not allowed to sign in.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists,You can try Login");
                return View(model);
            }

            //// Check if employee with this email already exists
            var existingEmployee = await _employeeRepository.GetByEmailAsync(model.Email);
            if (existingEmployee != null)
            {
                ModelState.AddModelError("Email", "An employee with this email address already exists,You can try Login");
                return View(model);
            }

            var user = new IdentityUser 
            { 
                UserName = model.Email, 
                Email = model.Email 
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully", model.Email);

                // Create corresponding Employee record
                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Position = model.Position,
                    Department = model.Department,
                    YearsOfExperience = model.YearsOfExperience??0
                };

                await _employeeRepository.AddAsync(employee);
                _logger.LogInformation("Employee record created for {Email}", model.Email);

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // Create confirmation URL
                var confirmationUrl = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new { userId = user.Id, token },
                    Request.Scheme);

                // Send confirmation email
                try
                {
                    await _emailService.SendEmailConfirmationAsync(user.Email!, token, confirmationUrl!);
                    _logger.LogInformation("Email confirmation sent to {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
                }

                TempData["InfoMessage"] = "Registration successful! Please check your email and click the confirmation link to activate your account.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
        }

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendEmailConfirmation(string email)
    {
        _logger.LogInformation("*** ResendEmailConfirmation ACTION CALLED *** with email: {Email}", email);

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("ResendEmailConfirmation: Email is null or empty");
            TempData["ErrorMessage"] = "Email address is required.";
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("ResendEmailConfirmation: User not found for email {Email}", email);
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Login");
        }

        if (await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogInformation("ResendEmailConfirmation: Email already confirmed for {Email}", email);
            TempData["InfoMessage"] = "Your email is already confirmed. You can log in now.";
            return RedirectToAction("Login");
        }

        try
        {
            // Generate new email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            _logger.LogInformation("ResendEmailConfirmation: Generated token for {Email}", email);
            
            // Create confirmation URL
            var confirmationUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token },
                Request.Scheme);

            _logger.LogInformation("ResendEmailConfirmation: Confirmation URL created: {Url}", confirmationUrl);

            // Send confirmation email
            var emailSent = await _emailService.SendEmailConfirmationAsync(user.Email!, token, confirmationUrl!);
            
            if (emailSent)
            {
                _logger.LogInformation("Email confirmation resent successfully to {Email}", user.Email);
                TempData["SuccessMessage"] = "Confirmation email has been resent. Please check your email.";
            }
            else
            {
                _logger.LogError("Failed to send confirmation email to {Email}", user.Email);
                TempData["ErrorMessage"] = "Failed to send confirmation email. Please try again later.";
                return RedirectToAction("Login");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend confirmation email to {Email}", user.Email);
            TempData["ErrorMessage"] = "Failed to send confirmation email. Please try again later.";
        }

        return RedirectToAction("Login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out");
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            _logger.LogError("External login error: {Error}", remoteError);
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction("Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            _logger.LogWarning("External login info is null");
            return RedirectToAction("Login");
        }

        // Sign in the user with this external login provider if the user already has a login
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Name} provider", info.LoginProvider);
            
            // Always redirect to dashboard after successful Google login
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("Index", "Dashboard");
        }

        // Handle existing users and new user creation
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email != null)
        {
            _logger.LogInformation("Processing external login for email: {Email}", email);
            
            // Check if user already exists by email
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogInformation("Found existing user for email: {Email}", email);
                
                // Check if this external login is already associated with the user
                var existingLogins = await _userManager.GetLoginsAsync(existingUser);
                var hasGoogleLogin = existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey);
                
                if (hasGoogleLogin)
                {
                    // External login already exists, just sign in the user
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    _logger.LogInformation("User {Email} signed in with existing Google login", email);
                }
                else
                {
                    // User exists but Google login doesn't exist - link the Google account
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        _logger.LogInformation("Linked Google account and signed in existing user {Email}", email);
                    }
                    else
                    {
                        _logger.LogError("Failed to link Google account for user {Email}: {Errors}", email, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                        ModelState.AddModelError(string.Empty, "Unable to link your Google account. Please try logging in with your email and password.");
                        return RedirectToAction("Login");
                    }
                }
                
                // Redirect to dashboard for existing users
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                // Create new user
                var user = new IdentityUser { UserName = email, Email = email };
                var createResult = await _userManager.CreateAsync(user);
                
                if (createResult.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider", info.LoginProvider);
                        
                        // Create employee record
                        var employee = new Employee
                        {
                            Email = email,
                            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "User",
                            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                            Position = "Employee",
                            Department = "General",
                            YearsOfExperience = 0
                        };
                        
                        await _employeeRepository.AddAsync(employee);
                        
                        // Always redirect to dashboard after successful Google login
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        
                        return RedirectToAction("Index", "Dashboard");
                    }
                    else
                    {
                        _logger.LogError("Failed to add external login: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    _logger.LogError("Failed to create user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }
        }
        else
        {
            _logger.LogError("Email claim not found in external login info");
        }

        ModelState.AddModelError(string.Empty, "Error creating account with external login.");
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Create callback URL
            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { token, email = user.Email },
                Request.Scheme);

            if (callbackUrl != null)
            {
                try
                {
                    // Send email
                    var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email!, token, callbackUrl);
                    
                    if (emailSent)
                    {
                        _logger.LogInformation("Password reset email sent to {Email}", user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send password reset email to {Email}", user.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending password reset email to {Email}", user.Email);
                }
            }

            // Always redirect to confirmation page (don't reveal if email exists)
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? token = null, string? email = null)
    {
        if (token == null || email == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid password reset token.");
        }
        
        var model = new ResetPasswordViewModel { Token = token, Email = email };
        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            _logger.LogInformation("Password reset successful for user {Email}", model.Email);
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId == null || token == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed for user {Email}", user.Email);
            TempData["SuccessMessage"] = "Your email has been confirmed successfully! You can now log in to your account.";
            return RedirectToAction("Login");
        }

        TempData["ErrorMessage"] = "Error confirming your email. The confirmation link may have expired.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CheckEmailAvailability(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Json(new { available = false, message = "Email is required" });
        }

        try
        {
            // Check if email already exists in Identity
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Json(new { available = false, message = "An account with this email address already exists" });
            }

            // Check if employee with this email already exists
            var existingEmployee = await _employeeRepository.GetByEmailAsync(email);
            if (existingEmployee != null)
            {
                return Json(new { available = false, message = "An employee with this email address already exists" });
            }

            return Json(new { available = true, message = "Email is available" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for {Email}", email);
            return Json(new { available = false, message = "Error checking email availability" });
        }
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} changed password successfully", user.Email);
                await _signInManager.SignOutAsync();
                TempData["SuccessMessage"] = "Your password has been changed successfully. Please log in with your new password.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user");
            ModelState.AddModelError(string.Empty, "An error occurred while changing your password. Please try again.");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult DeleteAccount()
    {
        return View(new DeleteAccountViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(DeleteAccountViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.ConfirmationText != "DELETE")
        {
            ModelState.AddModelError("ConfirmationText", "Please type 'DELETE' exactly as shown to confirm.");
            return View(model);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Verify password
            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!passwordCheck)
            {
                ModelState.AddModelError("Password", "The password you entered is incorrect.");
                return View(model);
            }

            // Delete associated employee record
            var employee = await _employeeRepository.GetByEmailAsync(user.Email!);
            if (employee != null)
            {
                await _employeeRepository.DeleteAsync(employee.Id);
                _logger.LogInformation("Employee record deleted for {Email}", user.Email);
            }

            // Sign out the user
            await _signInManager.SignOutAsync();

            // Delete the user account
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User account {Email} deleted successfully", user.Email);
                TempData["InfoMessage"] = "Your account has been permanently deleted. We're sorry to see you go.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user account");
            ModelState.AddModelError(string.Empty, "An error occurred while deleting your account. Please try again.");
        }

        return View(model);
    }

}
