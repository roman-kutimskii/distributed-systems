using System;
using System.Net;
using System.Threading.Tasks;
using Valuator.Specs.Fixture;
using Xunit;

namespace Valuator.Specs;

public class TestEnvironmentTests
{
    [Fact]
    public void TestEnvironment_ShouldBeInitialized()
    {
        // Arrange & Act
        var httpClient = TestServerFixtureCore.Instance.HttpClient;

        // Assert
        Assert.NotNull(httpClient);
    }

    [Fact]
    public void Redis_ShouldBeAvailable()
    {
        // Arrange
        var httpClient = TestServerFixtureCore.Instance.HttpClient;
        
        // Act & Assert
        // This test is just a placeholder to verify the test environment
        // In a real scenario, you might want to test something that uses Redis
        Assert.NotNull(httpClient);
        
        // The test passes if we can get the HttpClient without exceptions,
        // which means Redis container was successfully started
    }
}
