using Microsoft.AspNetCore.DataProtection;
using RabbitMQ.Client;
using StackExchange.Redis;
using Valuator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var redis = ConnectionMultiplexer.Connect(builder.Configuration["DB_MAIN"]!);
builder.Services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
    .SetApplicationName("Valuator");

var factory = new ConnectionFactory { HostName = "rabbitmq" };
var rabbitMqConnection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(rabbitMqConnection);

builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IMessageQueueService, MessageQueueService>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();