using System.Threading.Tasks;
using Xunit;
using Reqnroll.ScenarioCall.ReqnrollPlugin;
using Moq;
using Reqnroll.Infrastructure;
using Reqnroll.Bindings;

namespace Reqnroll.ScenarioCall.IntegrationTests
{
    public class ScenarioCallServiceTests
    {
        [Fact]
        public void ScenarioRegistry_CanRegisterAndFindScenario()
        {
            // Arrange
            var scenarioName = "Test Scenario";
            var featureName = "Test Feature";
            var steps = new[]
            {
                ScenarioRegistry.Given("step 1"),
                ScenarioRegistry.When("step 2"),
                ScenarioRegistry.Then("step 3")
            };

            // Act
            ScenarioRegistry.Register(scenarioName, featureName, steps);
            var found = GlobalScenarioRegistry.Find(scenarioName, featureName);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(scenarioName, found.Name);
            Assert.Equal(featureName, found.FeatureName);
            Assert.Equal(3, found.Steps.Length);
            Assert.Equal("Given", found.Steps[0].Keyword);
            Assert.Equal("step 1", found.Steps[0].Text);
        }

        [Fact]
        public async Task ScenarioCallService_ThrowsWhenScenarioNotFound()
        {
            // Arrange
            var mockEngine = new Mock<ITestExecutionEngine>();
            var service = new ScenarioCallService(mockEngine.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ReqnrollException>(
                () => service.CallScenarioAsync("Non-existent Scenario", "Non-existent Feature"));
            
            Assert.Contains("not found", exception.Message);
        }

        [Fact]
        public async Task ScenarioCallService_ExecutesRegisteredScenario()
        {
            // Arrange
            var mockEngine = new Mock<ITestExecutionEngine>();
            var service = new ScenarioCallService(mockEngine.Object);
            
            var scenarioName = "Execution Test Scenario";
            var featureName = "Execution Test Feature";
            
            // Register a test scenario
            ScenarioRegistry.Register(
                scenarioName, 
                featureName,
                ScenarioRegistry.Given("test step 1"),
                ScenarioRegistry.When("test step 2")
            );

            // Act
            await service.CallScenarioAsync(scenarioName, featureName);

            // Assert
            mockEngine.Verify(e => e.StepAsync(
                StepDefinitionKeyword.Given, 
                "Given", 
                "test step 1", 
                null, 
                null), Times.Once);
            
            mockEngine.Verify(e => e.StepAsync(
                StepDefinitionKeyword.When, 
                "When", 
                "test step 2", 
                null, 
                null), Times.Once);
        }
    }
}