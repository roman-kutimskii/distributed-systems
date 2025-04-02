using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Valuator.Services;

public interface IMessageQueueService
{
    Task PublishMessageAsync(string queueName, string message);
    Task PublishSimilarityCalculatedEventAsync(string textId, double similarity);
}

public class MessageQueueService(IConnection rabbitMqConnection) : IMessageQueueService
{
    public async Task PublishMessageAsync(string queueName, string message)
    {
        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync("", routingKey: queueName, body);

        await Task.CompletedTask;
    }

    public Task PublishSimilarityCalculatedEventAsync(string textId, double similarity)
    {
        var eventData = new { EventType = "SimilarityCalculated", TextId = textId, Similarity = similarity };
        var eventJson = JsonSerializer.Serialize(eventData);

        return PublishMessageAsync("events_queue", eventJson);
    }
}