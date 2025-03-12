using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(@"/keys"))
            .SetApplicationName("Valuator");

        // Add services to the container.
        builder.Services.AddRazorPages();

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        var redis = ConnectionMultiplexer.Connect(redisConnectionString);
        builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}