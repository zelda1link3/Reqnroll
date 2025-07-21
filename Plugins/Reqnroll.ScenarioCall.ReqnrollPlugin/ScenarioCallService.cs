using System;
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
                // Create an instance of the test class
                var testInstance = Activator.CreateInstance(scenarioDefinition.TestClass);
                
                // Check if the method is async
                if (scenarioDefinition.TestMethod.ReturnType == typeof(Task) || 
                    (scenarioDefinition.TestMethod.ReturnType.IsGenericType && 
                     scenarioDefinition.TestMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                {
                    // Invoke async method
                    var task = (Task)scenarioDefinition.TestMethod.Invoke(testInstance, null);
                    await task;
                }
                else
                {
                    // Invoke sync method
                    scenarioDefinition.TestMethod.Invoke(testInstance, null);
                }
            }
            catch (Exception ex)
            {
                // Unwrap target invocation exceptions
                var actualException = ex.InnerException ?? ex;
                throw new ReqnrollException($"Error executing scenario '{scenarioDefinition.Name}' from feature '{scenarioDefinition.FeatureName}': {actualException.Message}", actualException);
            }
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