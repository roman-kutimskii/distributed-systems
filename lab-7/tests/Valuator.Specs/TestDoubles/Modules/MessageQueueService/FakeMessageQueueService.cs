using Valuator.Services;

namespace Valuator.Specs.TestDoubles.Modules.MessageQueueService;

public class FakeMessageQueueService : IMessageQueueService

{
    public Task PublishMessageAsync(string queueName, string message)
    {
        throw new NotImplementedException();
    }

    public Task PublishSimilarityCalculatedEventAsync(string textId, double similarity)
    {
        throw new NotImplementedException();
    }
}