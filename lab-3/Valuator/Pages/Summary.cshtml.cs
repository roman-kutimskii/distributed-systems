﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace Valuator.Pages;

public class SummaryModel(IConnectionMultiplexer redis) : PageModel
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public double Rank { get; private set; }
    public double Similarity { get; private set; }

    public void OnGet(string id)
    {
        var rankValue = _redisDb.StringGet("RANK-" + id);
        if (rankValue.HasValue && double.TryParse(rankValue, out var rank))
        {
            Rank = rank;
        }

        var similarityValue = _redisDb.StringGet("SIMILARITY-" + id);
        if (similarityValue.HasValue && double.TryParse(similarityValue, out var similarity))
        {
            Similarity = similarity;
        }
    }
}