using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
                { "ConnectionStrings:MainConnection", dbConnectionString }
            });
        });

        return base.CreateHost(builder);
    }
}