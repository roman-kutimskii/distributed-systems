using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Valuator.Services;

public interface ICentrifugoService
{
    Task PublishAsync(string channel, object data);
}

public class CentrifugoService : ICentrifugoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CentrifugoService> _logger;

    public CentrifugoService(IConfiguration configuration, ILogger<CentrifugoService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Initializing CentrifugoService");
        
        try
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://centrifugo:8000")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"apikey my_api_key");
            _logger.LogInformation("CentrifugoService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing CentrifugoService");
            throw;
        }
    }

    public async Task PublishAsync(string channel, object data)
    {
        try
        {
            _logger.LogInformation("Publishing to channel {Channel}", channel);
            
            var request = new
            {
                method = "publish",
                @params = new
                {
                    channel,
                    data
                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api", content);
            response.EnsureSuccessStatusCode();
            
            _logger.LogInformation("Successfully published to channel {Channel}", channel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing to Centrifugo channel {Channel}", channel);
            throw;
        }
    }
}