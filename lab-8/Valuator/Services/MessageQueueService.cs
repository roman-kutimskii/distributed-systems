﻿using System.Text;
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
        await channel.QueueDeclareAsync(queueName, true, false, false);

        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync("", queueName, body);

        await Task.CompletedTask;
    }

    public async Task PublishSimilarityCalculatedEventAsync(string textId, double similarity)
    {
        await using var channel = await rabbitMqConnection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync("events_exchange", ExchangeType.Fanout, true);

        var eventData = new { EventType = "SimilarityCalculated", TextId = textId, Similarity = similarity };
        var eventJson = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(eventJson);

        await channel.BasicPublishAsync("events_exchange", "", body);
    }
}