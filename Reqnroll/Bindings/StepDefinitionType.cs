using System;

namespace Reqnroll.Bindings
{
    public enum StepDefinitionType
    {
        Given = ScenarioBlock.Given,
        When = ScenarioBlock.When,
        Then = ScenarioBlock.Then,
        Scenario = ScenarioBlock.Scenario
    }

    internal static class BindingTypeHelper
    {
        public static StepDefinitionType ToBindingType(this ScenarioBlock block)
        {
            if (block != ScenarioBlock.Given &&
                block != ScenarioBlock.When &&
                block != ScenarioBlock.Then &&
                block != ScenarioBlock.Scenario)
                throw new ArgumentException("Unable to convert block to binding type", nameof(block));

            return (StepDefinitionType)(int)block;
        }

        public static ScenarioBlock ToScenarioBlock(this StepDefinitionType stepDefinitionType)
        {
            return (ScenarioBlock)(int)stepDefinitionType;
        }

        public static StepDefinitionKeyword ToStepDefinitionKeyword(this StepDefinitionType stepDefinitionType)
        {
            return (StepDefinitionKeyword)(int)stepDefinitionType;
        }

        public static bool Equals(this ScenarioBlock block, StepDefinitionType stepDefinitionType)
        {
            return (int)block == (int)stepDefinitionType;
        }
    }
}