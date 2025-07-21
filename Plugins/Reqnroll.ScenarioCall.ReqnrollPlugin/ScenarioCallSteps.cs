using System.Threading.Tasks;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    [Binding]
    public class ScenarioCallSteps
    {
        private readonly IScenarioCallService _scenarioCallService;

        public ScenarioCallSteps(IScenarioCallService scenarioCallService)
        {
            _scenarioCallService = scenarioCallService;
        }

        [Given(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task GivenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _scenarioCallService.CallScenarioAsync(scenarioName, featureName);
        }

        [When(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task WhenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _scenarioCallService.CallScenarioAsync(scenarioName, featureName);
        }

        [Then(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task ThenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _scenarioCallService.CallScenarioAsync(scenarioName, featureName);
        }
    }
}