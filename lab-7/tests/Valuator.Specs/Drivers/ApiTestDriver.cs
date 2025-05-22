using Valuator.Specs.Fixture;
using Valuator.Specs.Helpers;

namespace Valuator.Specs.Drivers;

public class ApiTestDriver(ITestServerFixture fixture)
{
    private HttpClient HttpClient => fixture.HttpClient;

    public async Task<HttpResponseMessage> SubmitText(string text)
    {
        var formData = new Dictionary<string, string>
        {
            { "text", text },
            { "country", "Russia" }
        };

        var response = await AntiForgeryTokenExtractor.PostFormWithAntiForgeryToken(HttpClient, "/Index", formData);
        return response;
    }
}