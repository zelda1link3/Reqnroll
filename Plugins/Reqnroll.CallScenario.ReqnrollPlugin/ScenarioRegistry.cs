using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Reqnroll.CallScenario
{
    /// <summary>
    /// Represents a scenario that can be called from other scenarios
    /// </summary>
    public class CallableScenario
    {
        public string FeatureName { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public MethodInfo? ScenarioMethod { get; set; }
        public object? TestInstance { get; set; }
    }

    /// <summary>
    /// Interface for scenario discovery and execution
    /// </summary>
    public interface IScenarioRegistry
    {
        void RegisterScenario(CallableScenario scenario);
        CallableScenario? FindScenario(string featureName, string scenarioName);
        Task<bool> ExecuteScenarioAsync(string featureName, string scenarioName);
        void DiscoverScenarios(Assembly assembly);
    }

    /// <summary>
    /// Registry for discovering and executing scenarios by name
    /// </summary>
    public class ScenarioRegistry : IScenarioRegistry
    {
        private readonly Dictionary<string, CallableScenario> _scenarios = new Dictionary<string, CallableScenario>();

        public void RegisterScenario(CallableScenario scenario)
        {
            if (scenario == null)
                throw new ArgumentNullException(nameof(scenario));

            if (string.IsNullOrEmpty(scenario.FeatureName))
                throw new ArgumentException("Feature name cannot be null or empty", nameof(scenario));

            if (string.IsNullOrEmpty(scenario.ScenarioName))
                throw new ArgumentException("Scenario name cannot be null or empty", nameof(scenario));

            var key = GetScenarioKey(scenario.FeatureName, scenario.ScenarioName);
            _scenarios[key] = scenario;
        }

        public CallableScenario? FindScenario(string featureName, string scenarioName)
        {
            if (string.IsNullOrEmpty(featureName))
                return null;

            if (string.IsNullOrEmpty(scenarioName))
                return null;

            var key = GetScenarioKey(featureName, scenarioName);
            return _scenarios.TryGetValue(key, out var scenario) ? scenario : null;
        }

        public async Task<bool> ExecuteScenarioAsync(string featureName, string scenarioName)
        {
            var scenario = FindScenario(featureName, scenarioName);
            if (scenario == null)
                return false;

            if (scenario.ScenarioMethod == null || scenario.TestInstance == null)
                return false;

            try
            {
                var result = scenario.ScenarioMethod.Invoke(scenario.TestInstance, null);
                if (result is Task task)
                {
                    await task;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute scenario '{scenarioName}' from feature '{featureName}': {ex.Message}", ex);
            }
        }

        public void DiscoverScenarios(Assembly assembly)
        {
            // Look for types that have ReqnrollFeatureAttribute
            var featureTypes = assembly.GetTypes()
                .Where(type => type.GetCustomAttribute<ReqnrollFeatureAttribute>() != null)
                .ToList();

            foreach (var featureType in featureTypes)
            {
                var featureAttribute = featureType.GetCustomAttribute<ReqnrollFeatureAttribute>();
                if (featureAttribute == null) continue;

                var featureName = featureAttribute.Title ?? featureType.Name;

                // Find methods with ReqnrollScenarioAttribute
                var scenarioMethods = featureType.GetMethods()
                    .Where(method => method.GetCustomAttribute<ReqnrollScenarioAttribute>() != null)
                    .ToList();

                foreach (var scenarioMethod in scenarioMethods)
                {
                    var scenarioAttribute = scenarioMethod.GetCustomAttribute<ReqnrollScenarioAttribute>();
                    if (scenarioAttribute == null) continue;

                    var scenarioName = scenarioAttribute.Title ?? scenarioMethod.Name;

                    // We'll need to create test instances when needed
                    var callableScenario = new CallableScenario
                    {
                        FeatureName = featureName,
                        ScenarioName = scenarioName,
                        ScenarioMethod = scenarioMethod,
                        TestInstance = null // Will be set when needed
                    };

                    RegisterScenario(callableScenario);
                }
            }
        }

        private string GetScenarioKey(string featureName, string scenarioName)
        {
            return $"{featureName}::{scenarioName}";
        }
    }

    /// <summary>
    /// Attribute to mark feature classes for discovery
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ReqnrollFeatureAttribute : Attribute
    {
        public string? Title { get; set; }

        public ReqnrollFeatureAttribute()
        {
        }

        public ReqnrollFeatureAttribute(string title)
        {
            Title = title;
        }
    }

    /// <summary>
    /// Attribute to mark scenario methods for discovery
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ReqnrollScenarioAttribute : Attribute
    {
        public string? Title { get; set; }

        public ReqnrollScenarioAttribute()
        {
        }

        public ReqnrollScenarioAttribute(string title)
        {
            Title = title;
        }
    }
}