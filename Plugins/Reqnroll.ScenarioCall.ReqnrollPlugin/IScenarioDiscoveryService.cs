using System.Collections.Generic;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public interface IScenarioDiscoveryService
    {
        ScenarioDefinition FindScenario(string scenarioName, string featureName);
        IEnumerable<ScenarioDefinition> GetAllScenarios();
    }

    public class ScenarioDefinition
    {
        public string Name { get; set; }
        public string FeatureName { get; set; }
        public string[] Tags { get; set; }
        public StepDefinition[] Steps { get; set; }
    }

    public class StepDefinition
    {
        public string Keyword { get; set; }
        public string Text { get; set; }
        public Table Table { get; set; }
        public string MultilineText { get; set; }
    }
}