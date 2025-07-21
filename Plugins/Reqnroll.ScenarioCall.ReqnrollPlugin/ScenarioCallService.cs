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
                // Create an instance of the target feature class
                var targetFeatureInstance = Activator.CreateInstance(scenarioDefinition.TestClass);
                
                // Try to set the testRunner field
                var testRunnerField = scenarioDefinition.TestClass.GetField("testRunner", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (testRunnerField != null)
                {
                    testRunnerField.SetValue(targetFeatureInstance, _testRunner);
                }
                
                // Try to set the _testContext field if it exists
                var testContextField = scenarioDefinition.TestClass.GetField("_testContext", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (testContextField != null)
                {
                    // Get the current test context from ScenarioContext if available
                    var currentTestContext = GetCurrentTestContext();
                    if (currentTestContext != null)
                    {
                        testContextField.SetValue(targetFeatureInstance, currentTestContext);
                    }
                }
                
                // Call the scenario method directly
                var task = (Task)scenarioDefinition.TestMethod.Invoke(targetFeatureInstance, null);
                await task;
            }
            catch (Exception ex)
            {
                // Unwrap target invocation exceptions if needed
                var actualException = ex.InnerException ?? ex;
                throw new ReqnrollException($"Error executing scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}': {actualException.Message}", actualException);
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