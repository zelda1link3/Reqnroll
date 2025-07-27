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
            // Mark that we're entering a nested scenario call
            ScenarioCallContextManager.EnterNestedScenarioCall();
            
            try
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
            finally
            {
                // Always ensure we exit the nested scenario call, even if there's an exception
                ScenarioCallContextManager.ExitNestedScenarioCall();
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
                
                // Fallback: Log that we couldn't parse the feature file and skip execution
                // This is better than crashing with StepContext null reference
                throw new ReqnrollException($"Cannot execute scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}': Feature file not found or could not be parsed. The scenario was discovered but its steps could not be determined.");
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

        
        private object GetCurrentTestContext()
        {
            // This method was causing issues with context management
            // For now, return null to avoid complications
            return null;
        }

        private async Task ExecuteStepAsync(StepDefinition step)
        {
            // Instead of trying to manually manage StepContext, let's execute the step 
            // through the TestExecutionEngine directly but catch and handle the StepContext issue
            var stepKeyword = GetStepDefinitionKeyword(step.Keyword);
            
            try
            {
                await _testExecutionEngine.StepAsync(stepKeyword, step.Keyword, step.Text, step.MultilineText, step.Table);
            }
            catch (NullReferenceException ex) when (ex.StackTrace?.Contains("UpdateStatusOnStepFailure") == true)
            {
                // This is the StepContext null issue - let's wrap and provide a better error message
                throw new ReqnrollException($"Error executing step '{step.Keyword} {step.Text}' in called scenario. This may be caused by missing step definitions or context issues. Inner error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // For other exceptions, wrap them with context
                throw new ReqnrollException($"Error executing step '{step.Keyword} {step.Text}': {ex.Message}", ex);
            }
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