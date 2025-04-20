using System.Globalization;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace RankCalculator;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var mainRedisConnection = Environment.GetEnvironmentVariable("DB_MAIN");
        var mainRedis = await ConnectionMultiplexer.ConnectAsync(mainRedisConnection!);
        var mainDb = mainRedis.GetDatabase();

        var regionalRedisConnections = new Dictionary<string, IConnectionMultiplexer>();
        var regionalDbs = new Dictionary<string, IDatabase>();

        var ruRedisConnection = Environment.GetEnvironmentVariable("DB_RU");
        var euRedisConnection = Environment.GetEnvironmentVariable("DB_EU");
        var asiaRedisConnection = Environment.GetEnvironmentVariable("DB_ASIA");

        regionalRedisConnections["RU"] = await ConnectionMultiplexer.ConnectAsync(ruRedisConnection!);
        regionalRedisConnections["EU"] = await ConnectionMultiplexer.ConnectAsync(euRedisConnection!);
        regionalRedisConnections["ASIA"] = await ConnectionMultiplexer.ConnectAsync(asiaRedisConnection!);

        regionalDbs["RU"] = regionalRedisConnections["RU"].GetDatabase();
        regionalDbs["EU"] = regionalRedisConnections["EU"].GetDatabase();
        regionalDbs["ASIA"] = regionalRedisConnections["ASIA"].GetDatabase();

        var centrifugoService = new CentrifugoService();

        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("text_queue", true, false, false);

        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var textId = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                Console.WriteLine($"LOOKUP: {textId}, MAIN");

                var regionValue = await mainDb.StringGetAsync($"REGION-{textId}");

                if (!regionValue.HasValue)
                {
                    Console.WriteLine($"Region not found for text ID: {textId}");
                    return;
                }

                var region = regionValue.ToString();

                if (!regionalDbs.TryGetValue(region, out var regionalDb))
                {
                    Console.WriteLine($"Regional database not found for region: {region}");
                    return;
                }

                Console.WriteLine($"LOOKUP: {textId}, {region}");

                var text = await regionalDb.StringGetAsync($"TEXT-{textId}");
                if (!text.HasValue)
                {
                    Console.WriteLine($"Text not found in {region} database for ID: {textId}");
                    return;
                }

                var textStr = text.ToString();

                await Task.Delay(new Random().Next(3, 5) * 1000);

                var rank = CalculateRank(textStr);

                Console.WriteLine($"LOOKUP: {textId}, {region}");

                await regionalDb.StringSetAsync($"RANK-{textId}", rank);

                await centrifugoService.PublishAsync($"text:{textId}", rank.ToString(CultureInfo.InvariantCulture));

                var eventData = new { EventType = "RankCalculated", TextId = textId, Rank = rank, Region = region };
                var eventJson = JsonSerializer.Serialize(eventData);
                var eventBody = Encoding.UTF8.GetBytes(eventJson);

                await channel.BasicPublishAsync("events_exchange", "", eventBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync("text_queue", true, consumer);

        await Task.Delay(Timeout.Infinite);
    }

    private static double CalculateRank(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        double count = 0;

        foreach (var character in text) count += char.IsLetter(char.ToLower(character)) ? 1 : 0;

        return 1 - count / text.Length;
    }
}