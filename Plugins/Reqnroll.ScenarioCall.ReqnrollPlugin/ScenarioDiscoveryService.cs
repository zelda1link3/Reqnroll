using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Reqnroll.Bindings.Discovery;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioDiscoveryService : IScenarioDiscoveryService
    {
        private readonly IBindingSourceProcessor _bindingSourceProcessor;
        private readonly Assembly _testAssembly;
        private List<ScenarioDefinition> _cachedScenarios;

        public ScenarioDiscoveryService(IBindingSourceProcessor bindingSourceProcessor)
        {
            _bindingSourceProcessor = bindingSourceProcessor;
            _testAssembly = Assembly.GetCallingAssembly();
        }

        public ScenarioDefinition FindScenario(string scenarioName, string featureName)
        {
            var allScenarios = GetAllScenarios();
            
            return allScenarios.FirstOrDefault(s => 
                string.Equals(s.Name, scenarioName, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(s.FeatureName, featureName, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<ScenarioDefinition> GetAllScenarios()
        {
            if (_cachedScenarios != null)
                return _cachedScenarios;

            _cachedScenarios = new List<ScenarioDefinition>();
            
            // Try to find feature files in the test assembly
            try
            {
                var assemblyLocation = _testAssembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation);
                
                if (!string.IsNullOrEmpty(assemblyDir))
                {
                    DiscoverScenariosFromDirectory(assemblyDir);
                }
            }
            catch (Exception)
            {
                // If we can't find feature files, return empty collection
            }

            return _cachedScenarios;
        }

        private void DiscoverScenariosFromDirectory(string directory)
        {
            // Look for .feature files
            var featureFiles = Directory.GetFiles(directory, "*.feature", SearchOption.AllDirectories);
            
            foreach (var featureFile in featureFiles)
            {
                try
                {
                    ParseFeatureFile(featureFile);
                }
                catch
                {
                    // Skip files that can't be parsed
                }
            }
        }

        private void ParseFeatureFile(string featureFilePath)
        {
            var content = File.ReadAllText(featureFilePath);
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            string currentFeatureName = null;
            ScenarioDefinition currentScenario = null;
            var currentSteps = new List<StepDefinition>();
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                if (trimmedLine.StartsWith("Feature:"))
                {
                    currentFeatureName = trimmedLine.Substring("Feature:".Length).Trim();
                }
                else if (trimmedLine.StartsWith("Scenario:"))
                {
                    // Save previous scenario if exists
                    if (currentScenario != null)
                    {
                        currentScenario.Steps = currentSteps.ToArray();
                        _cachedScenarios.Add(currentScenario);
                    }
                    
                    // Start new scenario
                    currentScenario = new ScenarioDefinition
                    {
                        Name = trimmedLine.Substring("Scenario:".Length).Trim(),
                        FeatureName = currentFeatureName,
                        Tags = new string[0]
                    };
                    currentSteps = new List<StepDefinition>();
                }
                else if (currentScenario != null && IsStepLine(trimmedLine))
                {
                    var stepDef = ParseStepLine(trimmedLine);
                    if (stepDef != null)
                    {
                        currentSteps.Add(stepDef);
                    }
                }
            }
            
            // Add the last scenario
            if (currentScenario != null)
            {
                currentScenario.Steps = currentSteps.ToArray();
                _cachedScenarios.Add(currentScenario);
            }
        }

        private bool IsStepLine(string line)
        {
            return line.StartsWith("Given ") || line.StartsWith("When ") || 
                   line.StartsWith("Then ") || line.StartsWith("And ") || 
                   line.StartsWith("But ");
        }

        private StepDefinition ParseStepLine(string line)
        {
            var parts = line.Split(new[] { ' ' }, 2);
            if (parts.Length < 2) return null;
            
            return new StepDefinition
            {
                Keyword = parts[0],
                Text = parts[1]
            };
        }
    }
}