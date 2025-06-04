using Valuator.Models;

namespace Valuator.Services;

public interface IUserService
{
    Task<User?> GetUser(string username);
    Task<User?> CreateUser(string username, string password);
    bool ValidatePassword(User user, string password);
}

public class UserService(IRedisService redisService, ILogger<UserService> logger) : IUserService
{
    public async Task<User?> GetUser(string username)
    {
        try
        {
            var db = redisService.GetMainDatabase();
            var userJson = await db.StringGetAsync($"USER:{username}");

            if (!userJson.HasValue)
                return null;

            var parts = userJson.ToString().Split('|');
            if (parts.Length != 4)
                return null;

            return new User
            {
                Id = parts[0],
                Username = parts[1],
                PasswordHash = parts[2],
                CreatedAt = DateTime.Parse(parts[3])
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user {Username}", username);
            return null;
        }
    }

    public async Task<User?> CreateUser(string username, string password)
    {
        try
        {
            var existingUser = await GetUser(username);
            if (existingUser != null)
                return existingUser;

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            var db = redisService.GetMainDatabase();
            var userJson = $"{user.Id}|{user.Username}|{user.PasswordHash}|{user.CreatedAt:O}";
            await db.StringSetAsync($"USER:{username}", userJson);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user {Username}", username);
            return null;
        }
    }

    public bool ValidatePassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}