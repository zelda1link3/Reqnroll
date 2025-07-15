using System.Threading.Tasks;
using Reqnroll.Bindings;

namespace Reqnroll.Infrastructure
{
    public interface ITestExecutionEngine
    {
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }
        ITestThreadContext TestThreadContext { get; }

        Task OnTestRunStartAsync();
        Task OnTestRunEndAsync();

        Task OnFeatureStartAsync(FeatureInfo featureInfo);
        Task OnFeatureEndAsync();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo, RuleInfo ruleInfo);
        Task OnScenarioStartAsync();
        Task OnAfterLastStepAsync();
        Task OnScenarioEndAsync();

        Task OnScenarioSkippedAsync();

        Task StepAsync(StepDefinitionKeyword stepDefinitionKeyword, string keyword, string text, string multilineTextArg, Table tableArg);

        Task CallScenarioAsync(string featureName, string scenarioName);

        void Pending();
    }
}
