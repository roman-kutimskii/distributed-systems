using StackExchange.Redis;

namespace Valuator.Services;

public interface IRedisService
{
    Task SaveText(string id, string text);
    Task SaveSimilarity(string id, double similarity);
    double CalculateSimilarity(string id, string text);
}

public class RedisService(IConnectionMultiplexer redis) : IRedisService
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    public Task SaveText(string id, string text)
    {
        return _redisDb.StringSetAsync("TEXT-" + id, text);
    }

    public Task SaveSimilarity(string id, double similarity)
    {
        return _redisDb.StringSetAsync("SIMILARITY-" + id, similarity);
    }

    public double CalculateSimilarity(string id, string text)
    {
        var keys = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First()).Keys(pattern: "TEXT-*");

        foreach (var key in keys)
        {
            var storedText = _redisDb.StringGet(key);
            if (storedText == text && key != "TEXT-" + id) return 1;
        }

        return 0;
    }
}