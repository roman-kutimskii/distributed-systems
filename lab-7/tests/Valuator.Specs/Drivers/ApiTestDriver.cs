using Valuator.Specs.Fixture;

namespace Valuator.Specs.Drivers;

public class ApiTestDriver(ITestServerFixture fixture)
{
    private HttpClient HttpClient => fixture.HttpClient;
}