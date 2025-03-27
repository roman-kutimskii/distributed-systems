using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel(IConnectionMultiplexer redis, IConnection rabbitMqConnection) : PageModel
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost(string text)
    {
        var id = Guid.NewGuid().ToString();

        _redisDb.StringSet("TEXT-" + id, text);

        double similarity = CalculateSimilarity(id, text);
        _redisDb.StringSet("SIMILARITY-" + id, similarity);

        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync("text_queue", true, false, false);

        var body = System.Text.Encoding.UTF8.GetBytes(id);
        await channel.BasicPublishAsync("", "text_queue", body);

        return RedirectToPage("/Summary", new { id });
    }

    private double CalculateSimilarity(string id, string text)
    {
        var keys = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First()).Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var storedText = _redisDb.StringGet(key);
            if (storedText == text && key != "TEXT-" + id)
            {
                return 1;
            }
        }

        return 0;
    }
}