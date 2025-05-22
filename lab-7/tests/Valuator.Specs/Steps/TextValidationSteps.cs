using Reqnroll;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Valuator.Specs.Drivers;
using Valuator.Specs.Fixture;

namespace Valuator.Specs.Steps;

[Binding]
public class TextValidationSteps(TestServerFixture fixture)
{
    private readonly ApiTestDriver _driver = new(fixture);
    private HttpResponseMessage? _response;

    [Given(@"I have the Valuator component")]
    public void GivenIHaveTheValuatorComponent()
    {
        Assert.NotNull(_driver);
    }

    [When(@"I submit the text ""(.*)"" for validation")]
    public async Task WhenISubmitTheTextForValidation(string text)
    {
        _response = await _driver.SubmitText(text);
    }

    [Then(@"the validation process should be initiated")]
    public void ThenTheValidationProcessShouldBeInitiated()
    {
        Assert.NotNull(_response);
        Assert.True(_response!.IsSuccessStatusCode,
            $"Expected successful status code but got {_response.StatusCode}");
    }

    [Then(@"I should receive a validation response")]
    public async Task ThenIShouldReceiveAValidationResponse()
    {
        var content = await _response!.Content.ReadAsStringAsync();

        Assert.NotNull(content);
        Assert.True(_response!.IsSuccessStatusCode,
            $"Expected successful status code but got {_response.StatusCode}");

        var similarityValue = ExtractSimilarityValue(content);
        Assert.Equal("0", similarityValue);
    }

    private static string? ExtractSimilarityValue(string content)
    {
        Regex regex = new("<span id=\"similarity-value\">(.*?)</span>");
        var match = regex.Match(content);

        return match is { Success: true, Groups.Count: > 1 } ? match.Groups[1].Value : null;
    }
}