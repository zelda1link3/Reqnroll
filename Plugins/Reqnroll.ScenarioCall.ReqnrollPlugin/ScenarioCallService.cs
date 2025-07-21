using System;
using System.Threading.Tasks;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class ScenarioCallService : IScenarioCallService
    {
        private readonly ITestExecutionEngine _testExecutionEngine;
        private readonly IScenarioDiscoveryService _scenarioDiscoveryService;

        public ScenarioCallService(ITestExecutionEngine testExecutionEngine, IScenarioDiscoveryService scenarioDiscoveryService)
        {
            _testExecutionEngine = testExecutionEngine;
            _scenarioDiscoveryService = scenarioDiscoveryService;
        }

        public async Task CallScenarioAsync(string scenarioName, string featureName)
        {
            var scenarioDefinition = _scenarioDiscoveryService.FindScenario(scenarioName, featureName);
            
            if (scenarioDefinition == null)
            {
                throw new ReqnrollException($"Scenario '{scenarioName}' from feature '{featureName}' not found in the current assembly.");
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