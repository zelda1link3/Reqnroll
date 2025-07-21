using System;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    /// <summary>
    /// Helper class for registering scenarios that can be called from other scenarios
    /// </summary>
    public static class ScenarioRegistry
    {
        /// <summary>
        /// Register a scenario that can be called from other scenarios
        /// </summary>
        /// <param name="scenarioName">The name of the scenario exactly as it appears in the feature file</param>
        /// <param name="featureName">The name of the feature exactly as it appears in the feature file</param>
        /// <param name="steps">The steps that make up the scenario</param>
        public static void Register(string scenarioName, string featureName, params StepDefinition[] steps)
        {
            GlobalScenarioRegistry.Register(scenarioName, featureName, steps);
        }

        /// <summary>
        /// Helper method to create a Given step
        /// </summary>
        public static StepDefinition Given(string text, Table table = null, string multilineText = null)
        {
            return new StepDefinition
            {
                Keyword = "Given",
                Text = text,
                Table = table,
                MultilineText = multilineText
            };
        }

        /// <summary>
        /// Helper method to create a When step
        /// </summary>
        public static StepDefinition When(string text, Table table = null, string multilineText = null)
        {
            return new StepDefinition
            {
                Keyword = "When",
                Text = text,
                Table = table,
                MultilineText = multilineText
            };
        }

        /// <summary>
        /// Helper method to create a Then step
        /// </summary>
        public static StepDefinition Then(string text, Table table = null, string multilineText = null)
        {
            return new StepDefinition
            {
                Keyword = "Then",
                Text = text,
                Table = table,
                MultilineText = multilineText
            };
        }

        /// <summary>
        /// Helper method to create an And step
        /// </summary>
        public static StepDefinition And(string text, Table table = null, string multilineText = null)
        {
            return new StepDefinition
            {
                Keyword = "And",
                Text = text,
                Table = table,
                MultilineText = multilineText
            };
        }

        /// <summary>
        /// Helper method to create a But step
        /// </summary>
        public static StepDefinition But(string text, Table table = null, string multilineText = null)
        {
            return new StepDefinition
            {
                Keyword = "But",
                Text = text,
                Table = table,
                MultilineText = multilineText
            };
        }
    }
}