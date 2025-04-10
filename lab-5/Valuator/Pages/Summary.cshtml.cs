using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public double? Rank { get; private set; }
    public double? Similarity { get; private set; }

    public void OnGet(string id)
    {
        TryGetData(id);
    }

    public JsonResult OnGetCheckData(string id)
    {
        var isDataAvailable = TryGetData(id);

        return new JsonResult(new
        {
            isAvailable = isDataAvailable,
            rank = Rank,
            similarity = Similarity
        });
    }

    private bool TryGetData(string id)
    {
        var hasRank = false;
        var hasSimilarity = false;

        var rankValue = _redisDb.StringGet("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue, out var rank))
        {
            Rank = rank;
            hasRank = true;
        }

        var similarityValue = _redisDb.StringGet("SIMILARITY-" + id);
        if (similarityValue.HasValue && double.TryParse(similarityValue, out var similarity))
        {
            Similarity = similarity;
            hasSimilarity = true;
        }

        return hasRank && hasSimilarity;
    }
}