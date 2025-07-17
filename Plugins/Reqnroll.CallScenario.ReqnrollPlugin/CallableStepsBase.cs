using System;
using System.Threading.Tasks;
using Reqnroll.BoDi;

namespace Reqnroll.CallScenario
{
    /// <summary>
    /// Base class for step definitions that want to register callable scenarios
    /// </summary>
    public abstract class CallableStepsBase
    {
        protected readonly IScenarioRegistry _scenarioRegistry;

        protected CallableStepsBase(IScenarioRegistry scenarioRegistry)
        {
            _scenarioRegistry = scenarioRegistry ?? throw new ArgumentNullException(nameof(scenarioRegistry));
        }

        /// <summary>
        /// Register a scenario that can be called from other features
        /// </summary>
        /// <param name="featureName">Name of the feature</param>
        /// <param name="scenarioName">Name of the scenario</param>
        /// <param name="execute">Action to execute the scenario</param>
        protected void RegisterScenario(string featureName, string scenarioName, Action execute)
        {
            _scenarioRegistry.RegisterScenario(featureName, scenarioName, execute);
        }

        /// <summary>
        /// Register an async scenario that can be called from other features
        /// </summary>
        /// <param name="featureName">Name of the feature</param>
        /// <param name="scenarioName">Name of the scenario</param>
        /// <param name="executeAsync">Async function to execute the scenario</param>
        protected void RegisterScenario(string featureName, string scenarioName, Func<Task> executeAsync)
        {
            _scenarioRegistry.RegisterScenario(featureName, scenarioName, executeAsync);
        }
    }
}