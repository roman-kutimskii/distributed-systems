using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redisDb;

    public IndexModel(ILogger<IndexModel> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _redisDb = redis.GetDatabase();
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost(string text)
    {
        _logger.LogDebug(text);

        string id = Guid.NewGuid().ToString();

        string similarityKey = "SIMILARITY-" + id;
        double similarity = CalculateSimilarity(text);
        _redisDb.StringSet(similarityKey, similarity);

        string textKey = "TEXT-" + id;
        _redisDb.StringSet(textKey, text);

        string rankKey = "RANK-" + id;
        double rank = CalculateRank(text);
        _redisDb.StringSet(rankKey, rank);

        return Redirect($"summary?id={id}");
    }

    private double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        double count = 0;

        foreach (var character in text)
        {
            count += Char.IsLetter(Char.ToLower(character)) ? 1 : 0;
        }

        return 1 - count / text.Length;
    }

    private double CalculateSimilarity(string text)
    {
        var keys = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First()).Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var storedText = _redisDb.StringGet(key);
            if (storedText == text)
            {
                return 1;
            }
        }

        return 0;
    }
}