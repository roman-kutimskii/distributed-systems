using System.Text;
using System.Text.Json;

namespace RankCalculator;

public class CentrifugoService
{
    private readonly HttpClient _httpClient;

    public CentrifugoService()
    {
        Console.WriteLine("Initializing CentrifugoService");

        try
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://centrifugo:8000")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"apikey my_api_key");
            Console.WriteLine("CentrifugoService initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Error initializing CentrifugoService");
            throw;
        }
    }

    public async Task PublishAsync(string channel, object data)
    {
        try
        {
            Console.WriteLine("Publishing to channel {0}", channel);

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

            if (!response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Centrifugo API returned error: {0}, Content: {1}",
                    response.StatusCode, responseContent);
            }

            response.EnsureSuccessStatusCode();
            Console.WriteLine ("Successfully published to channel {0}", channel);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message, "Error publishing to Centrifugo channel", channel);
            throw;
        }
    }
}