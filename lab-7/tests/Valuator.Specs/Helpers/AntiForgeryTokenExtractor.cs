using System.Text.RegularExpressions;

namespace Valuator.Specs.Helpers;

public static class AntiForgeryTokenExtractor
{
    private static async Task<string> ExtractAntiForgeryToken(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var match = Regex.Match(content, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");

        if (!match.Success)
        {
            throw new InvalidOperationException("Unable to find antiforgery token on the page");
        }

        return match.Groups[1].Value;
    }

    public static async Task<HttpResponseMessage> PostFormWithAntiForgeryToken(
        HttpClient client,
        string url,
        Dictionary<string, string> formData)
    {
        var token = await ExtractAntiForgeryToken(client, url);

        var formDataWithToken = new Dictionary<string, string>(formData) { { "__RequestVerificationToken", token } };

        return await client.PostAsync(url, new FormUrlEncodedContent(formDataWithToken));
    }
}