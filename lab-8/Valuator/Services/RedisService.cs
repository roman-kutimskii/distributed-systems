using StackExchange.Redis;

namespace Valuator.Services;

public interface IRedisService
{
    Task SaveRegion(string id, string region);
    Task SaveText(string id, string text, string region);
    Task SaveSimilarity(string id, double similarity, string region);
    double CalculateSimilarity(string id, string text, string region);
    Task<string> GetRegionForId(string id);
    IDatabase GetRegionalDb(string region);
}

public class RedisService : IRedisService
{
    private readonly ILogger<RedisService> _logger;
    private readonly IDatabase _mainRedisDb;
    private readonly Dictionary<string, IDatabase> _regionalDbs;

    public RedisService(IConfiguration configuration, ILogger<RedisService> logger)
    {
        _logger = logger;

        var redisPassword = configuration["REDIS_PASSWORD"];

        var mainRedis = ConnectionMultiplexer.Connect(CreateConnectionString(configuration["DB_MAIN"]!, redisPassword));
        _mainRedisDb = mainRedis.GetDatabase();

        _regionalDbs = new Dictionary<string, IDatabase>
        {
            ["RU"] = ConnectionMultiplexer.Connect(CreateConnectionString(configuration["DB_RU"]!, redisPassword))
                .GetDatabase(),
            ["EU"] = ConnectionMultiplexer.Connect(CreateConnectionString(configuration["DB_EU"]!, redisPassword))
                .GetDatabase(),
            ["ASIA"] = ConnectionMultiplexer.Connect(CreateConnectionString(configuration["DB_ASIA"]!, redisPassword))
                .GetDatabase()
        };
    }

    public IDatabase GetRegionalDb(string region)
    {
        if (!_regionalDbs.TryGetValue(region, out var db))
            throw new ArgumentException($"Redis database for region '{region}' not configured.");

        return db;
    }

    public async Task SaveRegion(string id, string region)
    {
        await _mainRedisDb.StringSetAsync($"REGION-{id}", region);

        _logger.LogInformation($"LOOKUP: {id}, MAIN");
    }

    public async Task SaveText(string id, string text, string region)
    {
        var regionalDb = GetRegionalDb(region);
        await regionalDb.StringSetAsync($"TEXT-{id}", text);

        _logger.LogInformation($"LOOKUP: {id}, {region}");
    }

    public async Task SaveSimilarity(string id, double similarity, string region)
    {
        var regionalDb = GetRegionalDb(region);
        await regionalDb.StringSetAsync($"SIMILARITY-{id}", similarity);

        _logger.LogInformation($"LOOKUP: {id}, {region}");
    }

    public double CalculateSimilarity(string id, string text, string region)
    {
        var regionalDb = GetRegionalDb(region);

        var keys = regionalDb.Multiplexer.GetServer(regionalDb.Multiplexer.GetEndPoints().First())
            .Keys(pattern: "TEXT-*");

        _logger.LogInformation($"LOOKUP: {id}, {region}");

        foreach (var key in keys)
        {
            var storedText = regionalDb.StringGet(key);
            if (storedText == text && key != "TEXT-" + id) return 1;
        }

        return 0;
    }

    public async Task<string> GetRegionForId(string id)
    {
        var regionValue = await _mainRedisDb.StringGetAsync($"REGION-{id}");
        if (!regionValue.HasValue) throw new KeyNotFoundException($"Region not found for ID: {id}");

        var region = regionValue.ToString();

        _logger.LogInformation($"LOOKUP: {id}, MAIN");

        return region;
    }

    private static string CreateConnectionString(string hostAndPort, string? password)
    {
        return string.IsNullOrEmpty(password) ? hostAndPort : $"{hostAndPort},password={password}";
    }
}