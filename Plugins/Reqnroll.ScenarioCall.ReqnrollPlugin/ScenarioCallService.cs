using System;
using System.Threading.Tasks;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioCallService : IScenarioCallService
    {
        private readonly ITestExecutionEngine _testExecutionEngine;

        public ScenarioCallService(ITestExecutionEngine testExecutionEngine)
        {
            _testExecutionEngine = testExecutionEngine;
        }

        public async Task CallScenarioAsync(string scenarioName, string featureName)
        {
            // First try to find in global registry
            var scenarioDefinition = GlobalScenarioRegistry.Find(scenarioName, featureName);
            
            if (scenarioDefinition == null)
            {
                throw new ReqnrollException($"Scenario '{scenarioName}' from feature '{featureName}' not found. Make sure to register the scenario using ScenarioDiscoveryService.RegisterScenarioGlobally() in your test setup.");
            }

            // Execute each step of the found scenario
            foreach (var step in scenarioDefinition.Steps)
            {
                await ExecuteStepAsync(step);
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