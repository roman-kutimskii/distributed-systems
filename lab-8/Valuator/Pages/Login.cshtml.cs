using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class LoginModel(IUserService userService, ILogger<LoginModel> logger) : PageModel
{
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet(string? message)
    {
        if (!string.IsNullOrEmpty(message)) SuccessMessage = message;
    }

    public async Task<IActionResult> OnPost(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Логин и пароль не могут быть пустыми.";
            return Page();
        }

        try
        {
            var user = await userService.GetUser(username);

            if (user == null || !userService.ValidatePassword(user, password))
            {
                ErrorMessage = "Неверный логин или пароль.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.Username)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            logger.LogInformation("User {Username} logged in successfully", username);

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for user {Username}", username);
            ErrorMessage = "Произошла ошибка при входе в систему. Попробуйте еще раз.";
            return Page();
        }
    }
}