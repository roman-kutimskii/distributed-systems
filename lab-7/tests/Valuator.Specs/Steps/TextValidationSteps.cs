using Reqnroll;
using System.Net.Http.Json;
using Valuator.Specs.Drivers;
using Valuator.Specs.Fixture;

namespace Valuator.Specs.Steps;

[Binding]
public class TextValidationSteps(TestServerFixture fixture)
{
    private readonly ApiTestDriver _driver = new(fixture);
    private HttpResponseMessage? _response;
    private string _textToValidate = string.Empty;

    [Given(@"I have the Valuator component")]
    public void GivenIHaveTheValuatorComponent()
    {
        Assert.NotNull(_driver);
    }

    [When(@"I submit the text ""(.*)"" for validation")]
    public async Task WhenISubmitTheTextForValidation(string text)
    {
        _textToValidate = text;
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
    public void ThenIShouldReceiveAValidationResponse()
    {
        Assert.NotNull(_response);
        Assert.True(_response!.IsSuccessStatusCode,
            $"Expected successful status code but got {_response.StatusCode}");
    }
}