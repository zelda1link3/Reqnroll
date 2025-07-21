using System;
using System.Collections.Generic;
using System.Linq;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioDiscoveryService : IScenarioDiscoveryService
    {
        private readonly Dictionary<string, ScenarioDefinition> _scenarios = new();

        public ScenarioDefinition FindScenario(string scenarioName, string featureName)
        {
            var key = CreateKey(scenarioName, featureName);
            _scenarios.TryGetValue(key, out var scenario);
            return scenario;
        }

        public IEnumerable<ScenarioDefinition> GetAllScenarios()
        {
            return _scenarios.Values;
        }

        public void RegisterScenario(string scenarioName, string featureName, params StepDefinition[] steps)
        {
            var scenario = new ScenarioDefinition
            {
                Name = scenarioName,
                FeatureName = featureName,
                Steps = steps,
                Tags = new string[0]
            };

            var key = CreateKey(scenarioName, featureName);
            _scenarios[key] = scenario;
        }

        private string CreateKey(string scenarioName, string featureName)
        {
            return $"{featureName}::{scenarioName}";
        }

        // Static method to allow users to register scenarios from their step definitions
        public static void RegisterScenarioGlobally(string scenarioName, string featureName, params StepDefinition[] steps)
        {
            // This will be called during test setup
            GlobalScenarioRegistry.Register(scenarioName, featureName, steps);
        }
    }

    // Global registry that can be accessed from anywhere
    public static class GlobalScenarioRegistry
    {
        private static readonly Dictionary<string, ScenarioDefinition> _globalScenarios = new();

        public static void Register(string scenarioName, string featureName, params StepDefinition[] steps)
        {
            var scenario = new ScenarioDefinition
            {
                Name = scenarioName,
                FeatureName = featureName,
                Steps = steps,
                Tags = new string[0]
            };

            var key = $"{featureName}::{scenarioName}";
            _globalScenarios[key] = scenario;
        }

        public static ScenarioDefinition Find(string scenarioName, string featureName)
        {
            var key = $"{featureName}::{scenarioName}";
            _globalScenarios.TryGetValue(key, out var scenario);
            return scenario;
        }

        public static IEnumerable<ScenarioDefinition> GetAll()
        {
            return _globalScenarios.Values;
        }
    }
}