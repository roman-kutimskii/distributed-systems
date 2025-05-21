using Microsoft.AspNetCore.Mvc.Testing;

namespace Valuator.Specs.Fixture;

public class TestServerFixture
{
    public HttpClient HttpClient { get; }

    public TestServerFixture()
    {
        WebApplicationFactory<Program> factory = new();
        HttpClient = factory.CreateClient();
    }
}