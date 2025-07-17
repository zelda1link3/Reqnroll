using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll.CallScenario;

namespace Reqnroll.CallScenario.Tests
{
    public class CallScenarioStepsTests
    {
        [Fact]
        public async Task CallScenarioFromFeature_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.CallScenarioFromFeature("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }

        [Fact]
        public async Task CallScenarioFromFeature_EmptyFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                steps.CallScenarioFromFeature("Test Scenario", ""));

            exception.ParamName.Should().Be("featureName");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioFromFeature_EmptyScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                steps.CallScenarioFromFeature("", "Test Feature"));

            exception.ParamName.Should().Be("scenarioName");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioFromFeature_WhitespaceFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                steps.CallScenarioFromFeature("Test Scenario", "   "));

            exception.ParamName.Should().Be("featureName");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioFromFeature_WhitespaceScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                steps.CallScenarioFromFeature("   ", "Test Feature"));

            exception.ParamName.Should().Be("scenarioName");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioFromFeature_ScenarioNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(false);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                steps.CallScenarioFromFeature("Test Scenario", "Test Feature"));

            exception.Message.Should().Contain("Scenario 'Test Scenario' not found in feature 'Test Feature'");
            exception.Message.Should().Contain("could not be executed");
        }

        [Fact]
        public void Constructor_NullRegistry_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new CallScenarioSteps(null!));
            exception.ParamName.Should().Be("scenarioRegistry");
        }

        [Fact]
        public async Task ScenarioFromFeatureIsCalled_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.ScenarioFromFeatureIsCalled("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }

        [Fact]
        public async Task InvokeScenarioFromFeature_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.InvokeScenarioFromFeature("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }

        [Fact]
        public async Task ExecuteScenarioFromFeature_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.ExecuteScenarioFromFeature("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }

        [Fact]
        public async Task RunScenarioFromFeature_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.RunScenarioFromFeature("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }

        [Fact]
        public async Task TheScenarioFromFeatureIsExecuted_ValidParameters_ShouldCallRegistry()
        {
            // Arrange
            var mockRegistry = new Mock<IScenarioRegistry>();
            mockRegistry.Setup(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"))
                       .ReturnsAsync(true);

            var steps = new CallScenarioSteps(mockRegistry.Object);

            // Act
            await steps.TheScenarioFromFeatureIsExecuted("Test Scenario", "Test Feature");

            // Assert
            mockRegistry.Verify(r => r.ExecuteScenarioAsync("Test Feature", "Test Scenario"), Times.Once);
        }
    }
}