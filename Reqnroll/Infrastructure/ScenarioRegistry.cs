using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reqnroll.Infrastructure
{
    /// <summary>
    /// Represents a discoverable scenario that can be called from other scenarios
    /// </summary>
    public class ScenarioDescriptor
    {
        public string FeatureName { get; set; }
        public string ScenarioName { get; set; }
        public FeatureInfo FeatureInfo { get; set; }
        public ScenarioInfo ScenarioInfo { get; set; }
        public Func<Task> ExecuteAsync { get; set; }
    }

    /// <summary>
    /// Registry for discovering and executing scenarios by name
    /// </summary>
    public interface IScenarioRegistry
    {
        void RegisterScenario(ScenarioDescriptor descriptor);
        ScenarioDescriptor FindScenario(string featureName, string scenarioName);
        Task ExecuteScenarioAsync(string featureName, string scenarioName);
    }

    /// <summary>
    /// Default implementation of scenario registry
    /// </summary>
    public class ScenarioRegistry : IScenarioRegistry
    {
        private readonly Dictionary<string, ScenarioDescriptor> _scenarios = new Dictionary<string, ScenarioDescriptor>();
        private readonly ITestExecutionEngine _testExecutionEngine;

        public ScenarioRegistry(ITestExecutionEngine testExecutionEngine)
        {
            _testExecutionEngine = testExecutionEngine;
        }

        public void RegisterScenario(ScenarioDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (string.IsNullOrEmpty(descriptor.FeatureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(descriptor));

            if (string.IsNullOrEmpty(descriptor.ScenarioName))
                throw new ArgumentException("Scenario name cannot be null or empty", nameof(descriptor));

            var key = GetScenarioKey(descriptor.FeatureName, descriptor.ScenarioName);
            _scenarios[key] = descriptor;
        }

        public ScenarioDescriptor FindScenario(string featureName, string scenarioName)
        {
            if (string.IsNullOrEmpty(featureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(featureName));

            if (string.IsNullOrEmpty(scenarioName))
                throw new ArgumentException("Scenario name cannot be null or empty", nameof(scenarioName));

            var key = GetScenarioKey(featureName, scenarioName);
            return _scenarios.TryGetValue(key, out var descriptor) ? descriptor : null;
        }

        public async Task ExecuteScenarioAsync(string featureName, string scenarioName)
        {
            var descriptor = FindScenario(featureName, scenarioName);
            if (descriptor == null)
            {
                throw new InvalidOperationException($"Scenario '{scenarioName}' not found in feature '{featureName}'");
            }

            if (descriptor.ExecuteAsync == null)
            {
                throw new InvalidOperationException($"Scenario '{scenarioName}' in feature '{featureName}' does not have an execution delegate");
            }

            await descriptor.ExecuteAsync();
        }

        private string GetScenarioKey(string featureName, string scenarioName)
        {
            return $"{featureName}::{scenarioName}";
        }
    }
}