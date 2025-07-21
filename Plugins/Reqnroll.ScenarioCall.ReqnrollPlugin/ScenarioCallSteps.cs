using System.Threading.Tasks;
using Reqnroll.Infrastructure;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    [Binding]
    public class ScenarioCallSteps
    {
        private readonly IScenarioCallService _scenarioCallService;
        private readonly ITestRunner _testRunner;
        private readonly ScenarioContext _scenarioContext;

        public ScenarioCallSteps(ITestRunner testRunner, ScenarioContext scenarioContext, ITestExecutionEngine testExecutionEngine)
        {
            _testRunner = testRunner;
            _scenarioContext = scenarioContext;
            _scenarioCallService = new ScenarioCallService(testExecutionEngine, testRunner, scenarioContext);
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