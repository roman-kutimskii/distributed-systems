using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Valuator.Services;
using Valuator.Specs.TestDoubles.Modules.MessageQueueService;

namespace Valuator.Specs.Fixture;

public class CustomWebApplicationFactory<TEntryPoint>(string dbConnectionString)
    : WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "DB_MAIN", dbConnectionString }
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IMessageQueueService));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IMessageQueueService, FakeMessageQueueService>();
        });


        return base.CreateHost(builder);
    }
}