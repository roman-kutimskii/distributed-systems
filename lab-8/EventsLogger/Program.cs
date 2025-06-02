using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventsLogger;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Starting EventsLogger...");

        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq",
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var queueName = $"events_queue_{Guid.NewGuid()}";

        await channel.QueueDeclareAsync(queueName, false, true, true);

        await channel.QueueBindAsync(queueName, "events_exchange", "");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var message = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
            try
            {
                var eventData = JsonSerializer.Deserialize<EventData>(message);

                if (eventData != null)
                    switch (eventData.EventType)
                    {
                        case "RankCalculated":
                            Console.WriteLine($"Event: {eventData.EventType}");
                            Console.WriteLine($"TextId: {eventData.TextId}");
                            Console.WriteLine($"Rank: {eventData.Rank}");
                            break;
                        case "SimilarityCalculated":
                            Console.WriteLine($"Event: {eventData.EventType}");
                            Console.WriteLine($"TextId: {eventData.TextId}");
                            Console.WriteLine($"Similarity: {eventData.Similarity}");
                            break;
                        default:
                            Console.WriteLine($"Unknown event type: {eventData.EventType}");
                            break;
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }

            await Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(queueName, true, consumer);

        Console.WriteLine("EventsLogger is running.");
        await Task.Delay(Timeout.Infinite);
    }
}

internal class EventData
{
    public string EventType { get; set; } = string.Empty;
    public string TextId { get; set; } = string.Empty;
    public double? Rank { get; set; }
    public double? Similarity { get; set; }
}