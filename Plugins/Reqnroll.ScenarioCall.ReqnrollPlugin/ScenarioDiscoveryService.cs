using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioDiscoveryService : IScenarioDiscoveryService
    {
        private readonly Dictionary<string, ScenarioDefinition> _scenarios = new();
        private bool _hasDiscovered = false;

        public ScenarioDefinition FindScenario(string scenarioName, string featureName)
        {
            if (!_hasDiscovered)
            {
                DiscoverScenarios();
            }

            var key = CreateKey(scenarioName, featureName);
            _scenarios.TryGetValue(key, out var scenario);
            return scenario;
        }

        public IEnumerable<ScenarioDefinition> GetAllScenarios()
        {
            if (!_hasDiscovered)
            {
                DiscoverScenarios();
            }
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

        private void DiscoverScenarios()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        DiscoverScenariosInAssembly(assembly);
                    }
                    catch (Exception)
                    {
                        // Skip assemblies that can't be processed
                        continue;
                    }
                }
                _hasDiscovered = true;
            }
            catch (Exception)
            {
                // If discovery fails, we'll fall back to manual registration
                _hasDiscovered = true;
            }
        }

        private void DiscoverScenariosInAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            
            foreach (var type in types)
            {
                // Look for classes with GeneratedCodeAttribute with "Reqnroll" as the tool
                var generatedCodeAttr = type.GetCustomAttribute<GeneratedCodeAttribute>();
                if (generatedCodeAttr?.Tool?.Contains("Reqnroll") != true)
                    continue;

                try
                {
                    DiscoverScenariosInType(type);
                }
                catch (Exception)
                {
                    // Skip types that can't be processed
                    continue;
                }
            }
        }

        private void DiscoverScenariosInType(Type type)
        {
            // Find the FeatureInfo field to get the feature name
            var featureInfoField = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(f => f.FieldType.Name == "FeatureInfo");

            if (featureInfoField == null)
                return;

            string featureName = null;
            try
            {
                // Try to get the feature name from the FeatureInfo object
                var featureInfo = featureInfoField.GetValue(null);
                if (featureInfo != null)
                {
                    var titleProperty = featureInfo.GetType().GetProperty("Title");
                    featureName = titleProperty?.GetValue(featureInfo) as string;
                }
            }
            catch (Exception)
            {
                // If we can't get the feature name dynamically, try to extract from type name
                featureName = ExtractFeatureNameFromTypeName(type.Name);
            }

            if (string.IsNullOrEmpty(featureName))
                return;

            // Find test methods (scenarios) - look for methods with test-related attributes
            var testMethods = type.GetMethods()
                .Where(m => HasTestAttribute(m));

            foreach (var method in testMethods)
            {
                try
                {
                    DiscoverScenarioInMethod(method, featureName);
                }
                catch (Exception)
                {
                    // Skip methods that can't be processed
                    continue;
                }
            }
        }

        private void DiscoverScenarioInMethod(MethodInfo method, string featureName)
        {
            // Get scenario name from method attributes
            var scenarioName = GetScenarioNameFromMethod(method);

            if (string.IsNullOrEmpty(scenarioName))
                return;

            // Store the method info and class for direct invocation
            var scenario = new ScenarioDefinition
            {
                Name = scenarioName,
                FeatureName = featureName,
                Steps = new StepDefinition[0], // Keep empty for compatibility, but we'll use TestMethod instead
                Tags = new string[0], // TODO: Extract tags if needed
                TestMethod = method,
                TestClass = method.DeclaringType
            };

            var key = CreateKey(scenarioName, featureName);
            _scenarios[key] = scenario;
        }

        private string GetScenarioNameFromMethod(MethodInfo method)
        {
            // Look for TestMethodAttribute with the scenario name as constructor parameter
            // In the generated code: [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute("User logs in with valid credentials")]
            var customAttributes = method.GetCustomAttributesData();
            
            foreach (var attrData in customAttributes)
            {
                if (attrData.AttributeType.Name.Contains("TestMethod"))
                {
                    // Check if there are constructor arguments
                    if (attrData.ConstructorArguments.Count > 0)
                    {
                        var firstArg = attrData.ConstructorArguments[0];
                        if (firstArg.ArgumentType == typeof(string) && firstArg.Value is string scenarioName)
                        {
                            return scenarioName;
                        }
                    }
                }
            }
            
            // Look for Description attribute as fallback
            var attributes = method.GetCustomAttributes();
            foreach (var attr in attributes)
            {
                // Check for Description attribute
                var descriptionProp = attr.GetType().GetProperty("Description");
                if (descriptionProp != null)
                {
                    var description = descriptionProp.GetValue(attr) as string;
                    if (!string.IsNullOrEmpty(description))
                        return description;
                }
            }

            // Check for standard DescriptionAttribute
            var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
            if (descriptionAttr != null && !string.IsNullOrEmpty(descriptionAttr.Description))
            {
                return descriptionAttr.Description;
            }

            // Fall back to method name, converting from PascalCase
            return ConvertMethodNameToScenarioName(method.Name);
        }

        private bool HasTestAttribute(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes();
            return attributes.Any(attr => 
                attr.GetType().Name.Contains("Test") || 
                attr.GetType().Name.Contains("Fact") ||
                attr.GetType().Name.Contains("Scenario"));
        }



        private string ExtractFeatureNameFromTypeName(string typeName)
        {
            // Remove "Feature" suffix if present
            if (typeName.EndsWith("Feature"))
            {
                typeName = typeName.Substring(0, typeName.Length - "Feature".Length);
            }

            // Convert PascalCase to space-separated words
            return Regex.Replace(typeName, "([A-Z])", " $1").Trim();
        }

        private string ConvertMethodNameToScenarioName(string methodName)
        {
            // Convert PascalCase method name to space-separated scenario name
            return Regex.Replace(methodName, "([A-Z])", " $1").Trim();
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