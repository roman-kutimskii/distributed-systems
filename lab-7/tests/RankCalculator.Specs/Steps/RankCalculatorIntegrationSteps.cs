using Reqnroll;

namespace RankCalculator.Specs.Steps;

[Binding]
public class RankCalculatorIntegrationSteps
{
    [Given("the RankCalculator service is running")]
    public void GivenTheRankCalculatorServiceIsRunning()
    {
        ScenarioContext.StepIsPending();
    }

    [Given("a text with ID {string} is stored in the database")]
    public async Task GivenATextWithIdIsStoredInTheDatabase(string textId)
    {
        ScenarioContext.StepIsPending();
    }

    [Given("the text content is {string}")]
    public async Task GivenTheTextContentIs(string textContent)
    {
        ScenarioContext.StepIsPending();
    }

    [When("the rank calculation is triggered for text ID {string}")]
    public async Task WhenTheRankCalculationIsTriggeredForTextId(string textId)
    {
        ScenarioContext.StepIsPending();
    }

    [Then("the calculated rank should be stored in the database")]
    public async Task ThenTheCalculatedRankShouldBeStoredInTheDatabase()
    {
        ScenarioContext.StepIsPending();
    }

    [Then("the calculated rank should be approximately {decimal}")]
    public async Task ThenTheCalculatedRankShouldBeApproximately(decimal expectedRank)
    {
        ScenarioContext.StepIsPending();
    }

    [Then("a rank calculation event should be published")]
    public void ThenARankCalculationEventShouldBePublished()
    {
        ScenarioContext.StepIsPending();
    }
}