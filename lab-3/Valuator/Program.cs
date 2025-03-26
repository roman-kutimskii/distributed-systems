using RabbitMQ.Client;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var redis = ConnectionMultiplexer.Connect("redis:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

var factory = new ConnectionFactory { HostName = "rabbitmq" };
var rabbitMqConnection = await factory.CreateConnectionAsync();
builder.Services.AddSingleton(rabbitMqConnection);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();