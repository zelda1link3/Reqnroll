using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Reqnroll.CallScenario;

namespace Reqnroll.CallScenario.Tests
{
    public class ScenarioRegistryTests
    {
        [Fact]
        public void RegisterScenario_ValidScenario_ShouldRegisterSuccessfully()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var executed = false;

            // Act
            registry.RegisterScenario("Test Feature", "Test Scenario", () => executed = true);

            // Assert
            var foundScenario = registry.FindScenario("Test Feature", "Test Scenario");
            foundScenario.Should().NotBeNull();
            foundScenario!.FeatureName.Should().Be("Test Feature");
            foundScenario.ScenarioName.Should().Be("Test Scenario");
            
            // Verify the scenario can be executed
            foundScenario.ExecuteAsync().Wait();
            executed.Should().BeTrue();
        }

        [Fact]
        public void RegisterScenario_AsyncScenario_ShouldRegisterSuccessfully()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var executed = false;

            // Act
            registry.RegisterScenario("Test Feature", "Test Scenario", async () => 
            {
                await Task.Delay(1);
                executed = true;
            });

            // Assert
            var foundScenario = registry.FindScenario("Test Feature", "Test Scenario");
            foundScenario.Should().NotBeNull();
            foundScenario!.FeatureName.Should().Be("Test Feature");
            foundScenario.ScenarioName.Should().Be("Test Scenario");
            
            // Verify the scenario can be executed
            foundScenario.ExecuteAsync().Wait();
            executed.Should().BeTrue();
        }

        [Fact]
        public void RegisterScenario_EmptyFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                registry.RegisterScenario("", "Test Scenario", () => { }));
            exception.ParamName.Should().Be("featureName");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public void RegisterScenario_EmptyScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                registry.RegisterScenario("Test Feature", "", () => { }));
            exception.ParamName.Should().Be("scenarioName");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public void RegisterScenario_NullExecute_ShouldThrowArgumentNullException()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                registry.RegisterScenario("Test Feature", "Test Scenario", (Action)null!));
            exception.ParamName.Should().Be("execute");
        }

        [Fact]
        public void RegisterScenario_NullExecuteAsync_ShouldThrowArgumentNullException()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                registry.RegisterScenario("Test Feature", "Test Scenario", (Func<Task>)null!));
            exception.ParamName.Should().Be("executeAsync");
        }

        [Fact]
        public void FindScenario_ExistingScenario_ShouldReturnScenario()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            registry.RegisterScenario("Test Feature", "Test Scenario", () => { });

            // Act
            var foundScenario = registry.FindScenario("Test Feature", "Test Scenario");

            // Assert
            foundScenario.Should().NotBeNull();
            foundScenario!.FeatureName.Should().Be("Test Feature");
            foundScenario.ScenarioName.Should().Be("Test Scenario");
        }

        [Fact]
        public void FindScenario_NonExistingScenario_ShouldReturnNull()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act
            var foundScenario = registry.FindScenario("Non-existing Feature", "Non-existing Scenario");

            // Assert
            foundScenario.Should().BeNull();
        }

        [Fact]
        public void FindScenario_EmptyFeatureName_ShouldReturnNull()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act
            var foundScenario = registry.FindScenario("", "Some Scenario");

            // Assert
            foundScenario.Should().BeNull();
        }

        [Fact]
        public void FindScenario_EmptyScenarioName_ShouldReturnNull()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act
            var foundScenario = registry.FindScenario("Some Feature", "");

            // Assert
            foundScenario.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteScenarioAsync_ValidScenario_ShouldExecuteSuccessfully()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var executed = false;
            registry.RegisterScenario("Test Feature", "Test Scenario", () => executed = true);

            // Act
            var result = await registry.ExecuteScenarioAsync("Test Feature", "Test Scenario");

            // Assert
            result.Should().BeTrue();
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteScenarioAsync_ValidAsyncScenario_ShouldExecuteSuccessfully()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var executed = false;
            registry.RegisterScenario("Test Feature", "Test Scenario", async () => 
            {
                await Task.Delay(1);
                executed = true;
            });

            // Act
            var result = await registry.ExecuteScenarioAsync("Test Feature", "Test Scenario");

            // Assert
            result.Should().BeTrue();
            executed.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteScenarioAsync_NonExistingScenario_ShouldReturnFalse()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act
            var result = await registry.ExecuteScenarioAsync("Non-existing Feature", "Non-existing Scenario");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteScenarioAsync_ExceptionInScenario_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            registry.RegisterScenario("Test Feature", "Test Scenario", () => 
            {
                throw new InvalidOperationException("Test exception");
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                registry.ExecuteScenarioAsync("Test Feature", "Test Scenario"));

            exception.Message.Should().Contain("Failed to execute scenario 'Test Scenario' from feature 'Test Feature'");
            exception.InnerException.Should().NotBeNull();
            exception.InnerException!.Message.Should().Be("Test exception");
        }

        [Fact]
        public async Task ExecuteScenarioAsync_ExceptionInAsyncScenario_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            registry.RegisterScenario("Test Feature", "Test Scenario", async () => 
            {
                await Task.Delay(1);
                throw new InvalidOperationException("Test exception");
            });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                registry.ExecuteScenarioAsync("Test Feature", "Test Scenario"));

            exception.Message.Should().Contain("Failed to execute scenario 'Test Scenario' from feature 'Test Feature'");
            exception.InnerException.Should().NotBeNull();
            exception.InnerException!.Message.Should().Be("Test exception");
        }
    }
}