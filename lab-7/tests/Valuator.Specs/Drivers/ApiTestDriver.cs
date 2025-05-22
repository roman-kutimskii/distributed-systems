using Valuator.Specs.Fixture;

namespace Valuator.Specs.Drivers;

public class ApiTestDriver(ITestServerFixture fixture)
{
    private HttpClient HttpClient => fixture.HttpClient;

    public async Task<HttpResponseMessage> SubmitText(string text)
    {
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("text", text),
            new KeyValuePair<string, string>("country", "Russia")
        ]);

        var response = await HttpClient.PostAsync("/Index", content);
        return response;
    }
}