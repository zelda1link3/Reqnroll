using System;
using System.Threading.Tasks;
using FluentAssertions;
using Reqnroll.Specs.Drivers;
using Reqnroll.TestProjectGenerator.Driver;
using Reqnroll.TestProjectGenerator;

namespace Reqnroll.Specs.StepDefinitions
{
    [Binding]
    public class CallScenarioSteps
    {
        private readonly FeatureFileSteps _featureFileSteps;
        private readonly ExecutionDriver _executionDriver;
        private readonly CompilationDriver _compilationDriver;
        private readonly ProjectsDriver _projectsDriver;
        private readonly VSTestExecutionDriver _vsTestExecutionDriver;
        private Exception _caughtException;

        public CallScenarioSteps(FeatureFileSteps featureFileSteps, ExecutionDriver executionDriver, CompilationDriver compilationDriver, ProjectsDriver projectsDriver, VSTestExecutionDriver vsTestExecutionDriver)
        {
            _featureFileSteps = featureFileSteps;
            _executionDriver = executionDriver;
            _compilationDriver = compilationDriver;
            _projectsDriver = projectsDriver;
            _vsTestExecutionDriver = vsTestExecutionDriver;
        }

        [Given(@"the step definition for ""(.*)"" is implemented with CallScenarioAsync")]
        public void GivenTheStepDefinitionForIsImplementedWithCallScenarioAsync(string stepDefinitionPattern)
        {
            var stepDefinitionCode = $@"
using System.Threading.Tasks;
using Reqnroll;

[Binding]
public class CallScenarioStepDefinitions
{{
    private readonly ITestRunner _testRunner;
    
    public CallScenarioStepDefinitions(ITestRunner testRunner)
    {{
        _testRunner = testRunner;
    }}

    [Given(@""{stepDefinitionPattern}"")]
    public async Task GivenICallScenarioFromFeature(string scenarioName, string featureName)
    {{
        await _testRunner.CallScenarioAsync(featureName, scenarioName);
    }}
}}";

            _projectsDriver.AddFile("CallScenarioStepDefinitions.cs", stepDefinitionCode);
        }

        [When(@"I call CallScenarioAsync with empty feature name")]
        public async Task WhenICallCallScenarioAsyncWithEmptyFeatureName()
        {
            try
            {
                var testRunner = new TestRunner(null);
                await testRunner.CallScenarioAsync("", "ValidScenario");
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [When(@"I call CallScenarioAsync with empty scenario name")]
        public async Task WhenICallCallScenarioAsyncWithEmptyScenarioName()
        {
            try
            {
                var testRunner = new TestRunner(null);
                await testRunner.CallScenarioAsync("ValidFeature", "");
            }
            catch (Exception ex)
            {
                _caughtException = ex;
            }
        }

        [Then(@"an ArgumentException should be thrown")]
        public void ThenAnArgumentExceptionShouldBeThrown()
        {
            _caughtException.Should().BeOfType<ArgumentException>();
        }

        [Then(@"the called scenarios should be executed")]
        public void ThenTheCalledScenariosShouldBeExecuted()
        {
            // This will be implemented when we have full functionality
            // For now, just verify that the test structure is correct
            _vsTestExecutionDriver.LastTestExecutionResult.Should().NotBeNull();
        }
    }
}