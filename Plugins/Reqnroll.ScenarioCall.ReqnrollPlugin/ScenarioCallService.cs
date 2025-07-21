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

        public ScenarioCallService(ITestExecutionEngine testExecutionEngine, IScenarioDiscoveryService discoveryService = null)
        {
            _testExecutionEngine = testExecutionEngine;
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
                // Extract steps from the generated method body and execute them through Reqnroll infrastructure
                var steps = ExtractStepsFromMethod(scenarioDefinition.TestMethod);
                
                if (steps.Any())
                {
                    // Execute the extracted steps
                    foreach (var step in steps)
                    {
                        await ExecuteStepAsync(step);
                    }
                }
                else
                {
                    throw new ReqnrollException($"No steps found in scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}'.");
                }
            }
            catch (Exception ex)
            {
                // Unwrap target invocation exceptions if needed
                var actualException = ex.InnerException ?? ex;
                throw new ReqnrollException($"Error executing scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}': {actualException.Message}", actualException);
            }
        }

        private async Task ExecuteStepAsync(StepDefinition step)
        {
            var stepKeyword = GetStepDefinitionKeyword(step.Keyword);
            
            await _testExecutionEngine.StepAsync(stepKeyword, step.Keyword, step.Text, step.MultilineText, step.Table);
        }

        private StepDefinition[] ExtractStepsFromMethod(MethodInfo method)
        {
            var steps = new List<StepDefinition>();
            
            try
            {
                // Get the method body
                var methodBody = method.GetMethodBody();
                if (methodBody == null)
                    return steps.ToArray();

                // Get the IL bytes
                var il = methodBody.GetILAsByteArray();
                if (il == null || il.Length == 0)
                    return steps.ToArray();

                // This is a simplified IL parsing approach
                // Look for string literals that match step patterns
                var strings = ExtractStringLiteralsFromIL(method);
                
                foreach (var str in strings)
                {
                    // Check if this string looks like a step
                    if (IsStepText(str))
                    {
                        var stepKeyword = InferStepKeyword(str, steps.Count);
                        steps.Add(new StepDefinition
                        {
                            Keyword = stepKeyword,
                            Text = str,
                            Table = null,
                            MultilineText = null
                        });
                    }
                }
            }
            catch (Exception)
            {
                // If IL parsing fails, return empty array
                // This will fall back to manual registration
            }

            return steps.ToArray();
        }

        private List<string> ExtractStringLiteralsFromIL(MethodInfo method)
        {
            var strings = new List<string>();
            
            try
            {
                // Use reflection to get string literals from the method
                // This is a simplified approach - we'll look at the method's module's metadata
                var module = method.Module;
                var methodToken = method.MetadataToken;
                
                // Get all string literals used in the method
                // This is a basic approach that may not catch all cases
                var methodBody = method.GetMethodBody();
                if (methodBody != null)
                {
                    var il = methodBody.GetILAsByteArray();
                    
                    // Parse IL looking for ldstr instructions (load string)
                    for (int i = 0; i < il.Length - 4; i++)
                    {
                        // ldstr opcode is 0x72
                        if (il[i] == 0x72)
                        {
                            // Get the token (4 bytes little endian)
                            int token = BitConverter.ToInt32(il, i + 1);
                            
                            try
                            {
                                // Resolve the string token
                                var str = module.ResolveString(token);
                                if (!string.IsNullOrEmpty(str))
                                {
                                    strings.Add(str);
                                }
                            }
                            catch (Exception)
                            {
                                // Skip invalid tokens
                                continue;
                            }
                            
                            i += 4; // Skip the token bytes
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If anything fails, return empty list
            }
            
            return strings;
        }

        private bool IsStepText(string text)
        {
            // Check if the string looks like a Gherkin step
            // Filter out common non-step strings
            if (string.IsNullOrEmpty(text))
                return false;
                
            // Skip very short strings
            if (text.Length < 3)
                return false;
                
            // Skip common test framework strings
            var skipPatterns = new[]
            {
                "Given ", "When ", "Then ", "And ", "But ",  // These are step keywords, but we want the full text
                "TestMethod", "Description", "Feature", "Scenario",
                "#line", "hidden"
            };
            
            // Check if it's likely a step by looking for common patterns
            var stepPatterns = new[]
            {
                @"^(the|a|an)\s+\w+",
                @"\w+\s+(is|are|should|can|will|has|have)",
                @"(I|user|system)\s+\w+",
                @"\w+\s+(with|for|in|on|at|by)\s+",
                @"(login|log in|register|click|enter|submit|navigate)",
                @"(successful|failed|valid|invalid|correct|incorrect)"
            };
            
            // Must be a reasonable length for a step
            if (text.Length > 200)
                return false;
                
            // Should contain letters
            if (!Regex.IsMatch(text, @"[a-zA-Z]"))
                return false;
                
            // Check if it matches step patterns
            foreach (var pattern in stepPatterns)
            {
                if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
                    return true;
            }
            
            // If it contains quotes, it might be a step with parameters
            if (text.Contains("\"") && text.Count(c => c == '"') >= 2)
                return true;
                
            return false;
        }

        private string InferStepKeyword(string stepText, int stepIndex)
        {
            // Try to infer the step keyword based on content and position
            var lowerText = stepText.ToLowerInvariant();
            
            // Check for explicit keywords in the text
            if (lowerText.Contains("given") || lowerText.Contains("there is") || lowerText.Contains("there are"))
                return "Given";
            if (lowerText.Contains("when") || lowerText.Contains("the user") || lowerText.Contains("i "))
                return "When";
            if (lowerText.Contains("then") || lowerText.Contains("should") || lowerText.Contains("must"))
                return "Then";
                
            // Use position-based inference
            if (stepIndex == 0)
                return "Given";
            if (stepIndex == 1)
                return "When";
            if (stepIndex >= 2)
                return "Then";
                
            return "And";
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