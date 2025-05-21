using System.Net;
using System.Threading.Tasks;
using Reqnroll;
using Valuator.Specs.Fixture;
using Xunit;

namespace Valuator.Specs.Steps;

[Binding]
public class TestEnvironmentSteps
{
    private readonly TestServerFixtureCore _fixture;
    private HttpResponseMessage? _response;

    public TestEnvironmentSteps()
    {
        _fixture = TestServerFixtureCore.Instance;
    }

    [Given(@"the test environment is initialized")]
    public void GivenTheTestEnvironmentIsInitialized()
    {
        Assert.NotNull(_fixture.HttpClient);
    }

    [When(@"I check the system health")]
    public async Task WhenICheckTheSystemHealth()
    {
        // This is a placeholder. In a real scenario, you might want to call
        // a health check endpoint or perform some other system verification
        _response = await _fixture.HttpClient.GetAsync("/");
    }

    [Then(@"the system should be ready for testing")]
    public void ThenTheSystemShouldBeReadyForTesting()
    {
        // The actual verification might differ based on your application
        // This is just a simple check that we can communicate with the server
        Assert.NotNull(_response);
        
        // Note: The response status might not be OK if the endpoint doesn't exist
        // This is just to verify we can reach the server
    }
}
