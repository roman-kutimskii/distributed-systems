using Valuator.Services;

namespace Valuator.Specs.TestDoubles.Modules.MessageQueueService;

public class FakeMessageQueueService : IMessageQueueService

{
    public Task PublishMessageAsync(string queueName, string message)
    {
        return Task.CompletedTask;
    }

    public Task PublishSimilarityCalculatedEventAsync(string textId, double similarity)
    {
        return Task.CompletedTask;
    }
}