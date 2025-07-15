using System.Threading.Tasks;

namespace Reqnroll
{
    /// <summary>
    /// Built-in step definitions for CallScenario functionality.
    /// These step definitions are automatically available to all Reqnroll projects.
    /// </summary>
    [Binding]
    public class CallScenarioSteps
    {
        private readonly ITestRunner _testRunner;

        public CallScenarioSteps(ITestRunner testRunner)
        {
            _testRunner = testRunner;
        }

        /// <summary>
        /// Calls a scenario from another feature by name.
        /// </summary>
        /// <param name="scenarioName">The name of the scenario to call</param>
        /// <param name="featureName">The name of the feature containing the scenario</param>
        [Given("I call scenario {string} from feature {string}")]
        public async Task GivenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _testRunner.CallScenarioAsync(featureName, scenarioName);
        }

        /// <summary>
        /// Calls a scenario from another feature by name (When step).
        /// </summary>
        /// <param name="scenarioName">The name of the scenario to call</param>
        /// <param name="featureName">The name of the feature containing the scenario</param>
        [When("I call scenario {string} from feature {string}")]
        public async Task WhenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _testRunner.CallScenarioAsync(featureName, scenarioName);
        }

        /// <summary>
        /// Calls a scenario from another feature by name (Then step).
        /// </summary>
        /// <param name="scenarioName">The name of the scenario to call</param>
        /// <param name="featureName">The name of the feature containing the scenario</param>
        [Then("I call scenario {string} from feature {string}")]
        public async Task ThenICallScenarioFromFeature(string scenarioName, string featureName)
        {
            await _testRunner.CallScenarioAsync(featureName, scenarioName);
        }
    }
}