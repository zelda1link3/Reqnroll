using System;
using System.Threading.Tasks;
using Reqnroll.BoDi;

namespace Reqnroll.CallScenario
{
    /// <summary>
    /// Step definitions for calling scenarios from other features
    /// </summary>
    [Binding]
    public class CallScenarioSteps
    {
        private readonly IScenarioRegistry _scenarioRegistry;

        public CallScenarioSteps(IScenarioRegistry scenarioRegistry)
        {
            _scenarioRegistry = scenarioRegistry ?? throw new ArgumentNullException(nameof(scenarioRegistry));
        }

        [Given(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [When(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [Then(@"I call scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task CallScenarioFromFeature(string scenarioName, string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));

            if (string.IsNullOrWhiteSpace(scenarioName))
                throw new ArgumentException("Scenario name cannot be null or empty", nameof(scenarioName));

            var executed = await _scenarioRegistry.ExecuteScenarioAsync(featureName, scenarioName);
            
            if (!executed)
            {
                throw new InvalidOperationException($"Scenario '{scenarioName}' not found in feature '{featureName}' or could not be executed. " +
                                                    "Make sure the scenario exists and is properly marked with [ReqnrollScenario] attribute.");
            }
        }

        [Given(@"scenario ""([^""]*)"" from feature ""([^""]*)"" is called")]
        [When(@"scenario ""([^""]*)"" from feature ""([^""]*)"" is called")]
        [Then(@"scenario ""([^""]*)"" from feature ""([^""]*)"" is called")]
        public async Task ScenarioFromFeatureIsCalled(string scenarioName, string featureName)
        {
            await CallScenarioFromFeature(scenarioName, featureName);
        }

        [Given(@"I invoke scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [When(@"I invoke scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [Then(@"I invoke scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task InvokeScenarioFromFeature(string scenarioName, string featureName)
        {
            await CallScenarioFromFeature(scenarioName, featureName);
        }

        [Given(@"I execute scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [When(@"I execute scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [Then(@"I execute scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task ExecuteScenarioFromFeature(string scenarioName, string featureName)
        {
            await CallScenarioFromFeature(scenarioName, featureName);
        }

        [Given(@"I run scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [When(@"I run scenario ""([^""]*)"" from feature ""([^""]*)""")]
        [Then(@"I run scenario ""([^""]*)"" from feature ""([^""]*)""")]
        public async Task RunScenarioFromFeature(string scenarioName, string featureName)
        {
            await CallScenarioFromFeature(scenarioName, featureName);
        }

        [Given(@"the scenario ""([^""]*)"" from feature ""([^""]*)"" is executed")]
        [When(@"the scenario ""([^""]*)"" from feature ""([^""]*)"" is executed")]
        [Then(@"the scenario ""([^""]*)"" from feature ""([^""]*)"" is executed")]
        public async Task TheScenarioFromFeatureIsExecuted(string scenarioName, string featureName)
        {
            await CallScenarioFromFeature(scenarioName, featureName);
        }
    }
}