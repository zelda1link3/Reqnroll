using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.CallScenario;

namespace Reqnroll.CallScenario.Tests
{
    public class CallableStepsBaseTests
    {
        private class TestCallableSteps : CallableStepsBase
        {
            public TestCallableSteps(IScenarioRegistry scenarioRegistry) : base(scenarioRegistry)
            {
            }

            public void TestRegisterScenario(string featureName, string scenarioName, Action execute)
            {
                RegisterScenario(featureName, scenarioName, execute);
            }

            public void TestRegisterScenarioAsync(string featureName, string scenarioName, Func<Task> executeAsync)
            {
                RegisterScenario(featureName, scenarioName, executeAsync);
            }
        }

        [Fact]
        public void Constructor_ValidRegistry_ShouldInitializeSuccessfully()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();

            // Act
            var steps = new TestCallableSteps(mockRegistry.Object);

            // Assert
            steps.Should().NotBeNull();
        }

        [Fact]
        public void Constructor_NullRegistry_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new TestCallableSteps(null!));
            exception.ParamName.Should().Be("scenarioRegistry");
        }

        [Fact]
        public void RegisterScenario_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new TestCallableSteps(mockRegistry.Object);
            Action execute = () => { /* test action */ };

            // Act
            steps.TestRegisterScenario("Test Feature", "Test Scenario", execute);

            // Assert
            mockRegistry.Verify(r => r.RegisterScenario("Test Feature", "Test Scenario", execute), Times.Once);
        }

        [Fact]
        public void RegisterScenarioAsync_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new TestCallableSteps(mockRegistry.Object);
            Func<Task> executeAsync = async () => 
            {
                await Task.Delay(1);
                /* test async action */
            };

            // Act
            steps.TestRegisterScenarioAsync("Test Feature", "Test Scenario", executeAsync);

            // Assert
            mockRegistry.Verify(r => r.RegisterScenario("Test Feature", "Test Scenario", executeAsync), Times.Once);
        }
    }
}