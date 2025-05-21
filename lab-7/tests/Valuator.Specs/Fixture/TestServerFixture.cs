using Microsoft.AspNetCore.Mvc.Testing;

namespace Valuator.Specs.Fixture;

public class TestServerFixture : ITestServerFixture
{
    public HttpClient HttpClient => TestServerFixtureCore.Instance.HttpClient;
}