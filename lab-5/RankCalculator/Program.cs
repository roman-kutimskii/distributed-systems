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
        var redis = await ConnectionMultiplexer.ConnectAsync("redis:6379");
        var db = redis.GetDatabase();

        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        await using var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("text_queue", true, false, false);

        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

            var text = await db.StringGetAsync("TEXT-" + message);
            var textStr = text.ToString();
            
            await Task.Delay(new Random().Next(3, 15) * 1000);
            
            var rank = CalculateRank(textStr);

            await db.StringSetAsync("RANK-" + message, rank);

            var eventData = new { EventType = "RankCalculated", TextId = message, Rank = rank };
            var eventJson = JsonSerializer.Serialize(eventData);
            var eventBody = Encoding.UTF8.GetBytes(eventJson);

            await channel.BasicPublishAsync(
                "events_exchange",
                "",
                eventBody);
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