using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Valuator.Services;

namespace Valuator.Pages;

public class RegisterModel(IUserService userService, ILogger<RegisterModel> logger) : PageModel
{
    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string username, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Логин и пароль не могут быть пустыми.";
            return Page();
        }

        if (password != confirmPassword)
        {
            ErrorMessage = "Пароли не совпадают.";
            return Page();
        }

        if (password.Length < 6)
        {
            ErrorMessage = "Пароль должен содержать минимум 6 символов.";
            return Page();
        }

        try
        {
            var user = await userService.CreateUser(username, password);

            if (user == null)
            {
                ErrorMessage = "Пользователь с таким логином уже существует.";
                return Page();
            }

            logger.LogInformation("User {Username} registered successfully", username);
            return RedirectToPage("/Login", new { message = "Регистрация прошла успешно. Войдите в систему." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration for user {Username}", username);
            ErrorMessage = "Произошла ошибка при регистрации. Попробуйте еще раз.";
            return Page();
        }
    }
}