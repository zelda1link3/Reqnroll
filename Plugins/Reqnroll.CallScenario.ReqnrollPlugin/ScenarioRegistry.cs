using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.CallScenario
{
    /// <summary>
    /// Represents a callable scenario that can be executed from step definitions
    /// </summary>
    public class CallableScenario
    {
        public string FeatureName { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public Func<Task> ExecuteAsync { get; set; } = () => Task.CompletedTask;
    }

    /// <summary>
    /// Interface for scenario registry
    /// </summary>
    public interface IScenarioRegistry
    {
        void RegisterScenario(string featureName, string scenarioName, Func<Task> executeAsync);
        void RegisterScenario(string featureName, string scenarioName, Action execute);
        CallableScenario? FindScenario(string featureName, string scenarioName);
        Task<bool> ExecuteScenarioAsync(string featureName, string scenarioName);
    }

    /// <summary>
    /// Registry for managing callable scenarios
    /// </summary>
    public class ScenarioRegistry : IScenarioRegistry
    {
        private readonly Dictionary<string, CallableScenario> _scenarios = new Dictionary<string, CallableScenario>();

        public void RegisterScenario(string featureName, string scenarioName, Func<Task> executeAsync)
        {
            if (string.IsNullOrWhiteSpace(featureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));

            if (string.IsNullOrWhiteSpace(scenarioName))
                throw new ArgumentException("Scenario name cannot be null or empty", nameof(scenarioName));

            if (executeAsync == null)
                throw new ArgumentNullException(nameof(executeAsync));

            var key = GetScenarioKey(featureName, scenarioName);
            _scenarios[key] = new CallableScenario
            {
                FeatureName = featureName,
                ScenarioName = scenarioName,
                ExecuteAsync = executeAsync
            };
        }

        public void RegisterScenario(string featureName, string scenarioName, Action execute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            RegisterScenario(featureName, scenarioName, () =>
            {
                execute();
                return Task.CompletedTask;
            });
        }

        public CallableScenario? FindScenario(string featureName, string scenarioName)
        {
            if (string.IsNullOrWhiteSpace(featureName) || string.IsNullOrWhiteSpace(scenarioName))
                return null;

            var key = GetScenarioKey(featureName, scenarioName);
            return _scenarios.TryGetValue(key, out var scenario) ? scenario : null;
        }

        public async Task<bool> ExecuteScenarioAsync(string featureName, string scenarioName)
        {
            var scenario = FindScenario(featureName, scenarioName);
            if (scenario == null)
                return false;

            try
            {
                await scenario.ExecuteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute scenario '{scenarioName}' from feature '{featureName}': {ex.Message}", ex);
            }
        }

        private string GetScenarioKey(string featureName, string scenarioName)
        {
            return $"{featureName}::{scenarioName}";
        }
    }
}