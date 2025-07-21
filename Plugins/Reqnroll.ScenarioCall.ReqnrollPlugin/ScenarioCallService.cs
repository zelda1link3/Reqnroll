using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioCallService : IScenarioCallService
    {
        private readonly ITestExecutionEngine _testExecutionEngine;
        private readonly IScenarioDiscoveryService _discoveryService;
        private readonly ITestRunner _testRunner;
        private readonly ScenarioContext _scenarioContext;

        public ScenarioCallService(ITestExecutionEngine testExecutionEngine, ITestRunner testRunner, ScenarioContext scenarioContext, IScenarioDiscoveryService discoveryService = null)
        {
            _testExecutionEngine = testExecutionEngine;
            _testRunner = testRunner;
            _scenarioContext = scenarioContext;
            _discoveryService = discoveryService ?? new ScenarioDiscoveryService();
        }

        public async Task CallScenarioAsync(string scenarioName, string featureName)
        {
            // First try to find using discovery service
            var scenarioDefinition = _discoveryService.FindScenario(scenarioName, featureName);
            
            // Fallback to global registry for manually registered scenarios
            if (scenarioDefinition == null)
            {
                scenarioDefinition = GlobalScenarioRegistry.Find(scenarioName, featureName);
            }
            
            if (scenarioDefinition == null)
            {
                throw new ReqnrollException($"Scenario '{scenarioName}' from feature '{featureName}' not found. The scenario should be automatically discovered from generated Reqnroll code or manually registered using ScenarioRegistry.Register().");
            }

            // If we have a TestMethod from discovery, invoke it directly
            if (scenarioDefinition.TestMethod != null && scenarioDefinition.TestClass != null)
            {
                await InvokeTestMethodDirectly(scenarioDefinition);
            }
            else
            {
                // Fallback to executing individual steps for manually registered scenarios
                foreach (var step in scenarioDefinition.Steps)
                {
                    await ExecuteStepAsync(step);
                }
            }
        }

        private async Task InvokeTestMethodDirectly(ScenarioDefinition scenarioDefinition)
        {
            try
            {
                // The issue is that directly calling test methods bypasses Reqnroll's context management
                // Let's try to parse the feature file and extract the actual steps instead
                var featureFileSteps = TryParseFeatureFileForScenario(scenarioDefinition.Name, scenarioDefinition.FeatureName);
                
                if (featureFileSteps != null && featureFileSteps.Any())
                {
                    // Execute the steps we found in the feature file
                    foreach (var step in featureFileSteps)
                    {
                        await ExecuteStepAsync(step);
                    }
                    return;
                }
                
                // Fallback: if we can't find the feature file, try direct method invocation with better context handling
                await InvokeTestMethodWithContextManagement(scenarioDefinition);
            }
            catch (Exception ex)
            {
                // Unwrap target invocation exceptions if needed
                var actualException = ex.InnerException ?? ex;
                throw new ReqnrollException($"Error executing scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}': {actualException.Message}", actualException);
            }
        }
        
        private List<StepDefinition> TryParseFeatureFileForScenario(string scenarioName, string featureName)
        {
            try
            {
                // Look for feature files in common locations
                var possiblePaths = new[]
                {
                    "Features",
                    ".",
                    "**/*.feature"
                };
                
                // Try to find feature files
                var currentDirectory = Environment.CurrentDirectory;
                var featureFiles = new List<string>();
                
                // Search for .feature files
                if (System.IO.Directory.Exists(System.IO.Path.Combine(currentDirectory, "Features")))
                {
                    featureFiles.AddRange(System.IO.Directory.GetFiles(
                        System.IO.Path.Combine(currentDirectory, "Features"), 
                        "*.feature", 
                        System.IO.SearchOption.AllDirectories));
                }
                
                // Also search in current directory and subdirectories
                featureFiles.AddRange(System.IO.Directory.GetFiles(
                    currentDirectory, 
                    "*.feature", 
                    System.IO.SearchOption.AllDirectories));
                
                foreach (var featureFile in featureFiles.Distinct())
                {
                    try
                    {
                        var content = System.IO.File.ReadAllText(featureFile);
                        var steps = ParseScenarioFromFeatureContent(content, scenarioName, featureName);
                        if (steps != null && steps.Any())
                        {
                            return steps;
                        }
                    }
                    catch
                    {
                        // Continue to next file
                    }
                }
            }
            catch
            {
                // If we can't parse feature files, we'll fall back to direct method invocation
            }
            
            return null;
        }
        
        private List<StepDefinition> ParseScenarioFromFeatureContent(string content, string scenarioName, string featureName)
        {
            var lines = content.Split('\n');
            var steps = new List<StepDefinition>();
            bool inTargetScenario = false;
            bool inFeature = false;
            string currentFeatureName = null;
            
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Check for feature declaration
                if (trimmedLine.StartsWith("Feature:"))
                {
                    currentFeatureName = trimmedLine.Substring(8).Trim();
                    inFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                
                // Skip if we're not in the right feature
                if (!inFeature)
                    continue;
                
                // Check for scenario declaration
                if (trimmedLine.StartsWith("Scenario:"))
                {
                    var scenarioTitle = trimmedLine.Substring("Scenario:".Length).Trim();
                    inTargetScenario = string.Equals(scenarioTitle, scenarioName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                
                // If we hit another scenario or feature, stop
                if ((trimmedLine.StartsWith("Scenario:") || trimmedLine.StartsWith("Feature:")) && inTargetScenario)
                {
                    break;
                }
                
                // Parse steps within the target scenario
                if (inTargetScenario)
                {
                    if (trimmedLine.StartsWith("Given ") || trimmedLine.StartsWith("When ") ||
                        trimmedLine.StartsWith("Then ") || trimmedLine.StartsWith("And ") ||
                        trimmedLine.StartsWith("But "))
                    {
                        var parts = trimmedLine.Split(new char[] { ' ' }, 2);
                        if (parts.Length >= 2)
                        {
                            steps.Add(new StepDefinition
                            {
                                Keyword = parts[0],
                                Text = parts[1]
                            });
                        }
                    }
                }
            }
            
            return steps.Any() ? steps : null;
        }
        
        private async Task InvokeTestMethodWithContextManagement(ScenarioDefinition scenarioDefinition)
        {
            // This is the last resort - try to call the method directly but with better error handling
            // The issue is likely that we need to ensure proper test context initialization
            
            // Create an instance of the target feature class
            var targetFeatureInstance = Activator.CreateInstance(scenarioDefinition.TestClass);
            
            // Try to set the testRunner field with our current testRunner
            var testRunnerField = scenarioDefinition.TestClass.GetField("testRunner", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (testRunnerField != null)
            {
                testRunnerField.SetValue(targetFeatureInstance, _testRunner);
            }
            
            // Try to set the _testContext field if it exists
            var testContextField = scenarioDefinition.TestClass.GetField("_testContext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (testContextField != null)
            {
                var currentTestContext = GetCurrentTestContext();
                if (currentTestContext != null)
                {
                    testContextField.SetValue(targetFeatureInstance, currentTestContext);
                }
            }
            
            // Check if the test method is async
            var task = scenarioDefinition.TestMethod.Invoke(targetFeatureInstance, null);
            if (task is Task asyncTask)
            {
                await asyncTask;
            }
        }
        
        private object GetCurrentTestContext()
        {
            try
            {
                // Try to get the TestContext from ScenarioContext
                if (_scenarioContext?.ScenarioContainer != null)
                {
                    // Try to find any registered TestContext-like object
                    // This is a generic approach that should work with different test frameworks
                    
                    // First try to find a type with "TestContext" in the name
                    var allRegistrations = _scenarioContext.ScenarioContainer.GetType()
                        .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(f => f.Name.Contains("registrations") || f.Name.Contains("objects"))
                        .FirstOrDefault();
                        
                    if (allRegistrations != null)
                    {
                        var registrationsValue = allRegistrations.GetValue(_scenarioContext.ScenarioContainer);
                        // This is getting complex - let's just try the common pattern
                    }
                    
                    // Try common test context type names
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.FullName.Contains("Microsoft.VisualStudio.TestTools.UnitTesting"));
                        
                    if (assembly != null)
                    {
                        var testContextType = assembly.GetType("Microsoft.VisualStudio.TestTools.UnitTesting.TestContext");
                        if (testContextType != null)
                        {
                            try
                            {
                                return _scenarioContext.ScenarioContainer.Resolve(testContextType);
                            }
                            catch
                            {
                                // Not registered
                            }
                        }
                    }
                }
            }
            catch
            {
                // If we can't get the test context, that's OK - the scenario might still work
            }
            
            return null;
        }

        private async Task ExecuteStepAsync(StepDefinition step)
        {
            var stepKeyword = GetStepDefinitionKeyword(step.Keyword);
            
            await _testExecutionEngine.StepAsync(stepKeyword, step.Keyword, step.Text, step.MultilineText, step.Table);
        }

        private StepDefinitionKeyword GetStepDefinitionKeyword(string keyword)
        {
            return keyword.ToLowerInvariant() switch
            {
                "given" => StepDefinitionKeyword.Given,
                "when" => StepDefinitionKeyword.When,
                "then" => StepDefinitionKeyword.Then,
                "and" => StepDefinitionKeyword.And,
                "but" => StepDefinitionKeyword.But,
                _ => StepDefinitionKeyword.Given // Default fallback
            };
        }
    }
}