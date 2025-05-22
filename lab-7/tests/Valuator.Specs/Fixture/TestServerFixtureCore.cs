using Reqnroll;
using Testcontainers.Redis;

namespace Valuator.Specs.Fixture;

[Binding]
public class TestServerFixtureCore
{
    public static readonly TestServerFixtureCore Instance = new();

    private HttpClient? _httpClient;

    private readonly RedisContainer _containerMain = new RedisBuilder()
        .WithImage("redis")
        .Build();

    private readonly RedisContainer _containerRu = new RedisBuilder()
        .WithImage("redis")
        .Build();

    private readonly RedisContainer _containerEu = new RedisBuilder()
        .WithImage("redis")
        .Build();

    private readonly RedisContainer _containerAsia = new RedisBuilder()
        .WithImage("redis")
        .Build();

    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("Fixture was not initialized");

    private TestServerFixtureCore()
    {
    }

    [BeforeTestRun]
    public static Task BeforeTestRun()
    {
        return Instance.InitializeAsync();
    }

    [AfterTestRun]
    public static Task AfterTestRun()
    {
        return Instance.DisposeAsync();
    }

    private async Task InitializeAsync()
    {
        await _containerMain.StartAsync();
        await _containerRu.StartAsync();
        await _containerEu.StartAsync();
        await _containerAsia.StartAsync();

        CustomWebApplicationFactory<Program> factory = new(_containerMain.GetConnectionString(),
            _containerRu.GetConnectionString(), _containerEu.GetConnectionString(),
            _containerAsia.GetConnectionString());
        _httpClient = factory.CreateClient();
    }

    private async Task DisposeAsync()
    {
        _httpClient = null;
        await _containerMain.DisposeAsync();
        await _containerRu.DisposeAsync();
        await _containerEu.DisposeAsync();
        await _containerAsia.DisposeAsync();
    }
}