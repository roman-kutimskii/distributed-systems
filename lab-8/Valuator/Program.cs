using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

// Create Redis connection with authentication
var redisPassword = builder.Configuration["REDIS_PASSWORD"];
var redisConnectionString = string.IsNullOrEmpty(redisPassword)
    ? builder.Configuration["DB_MAIN"]!
    : $"{builder.Configuration["DB_MAIN"]!},password={redisPassword}";

var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("Valuator");

try
{
    // Create RabbitMQ connection with authentication
    var factory = new ConnectionFactory
    {
        HostName = "rabbitmq",
        UserName = builder.Configuration["RABBITMQ_USERNAME"] ?? "guest",
        Password = builder.Configuration["RABBITMQ_PASSWORD"] ?? "guest"
    };
    var rabbitMqConnection = await factory.CreateConnectionAsync();
    builder.Services.AddSingleton(rabbitMqConnection);
}
catch
{
}

builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();
builder.Services.AddSingleton<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program;