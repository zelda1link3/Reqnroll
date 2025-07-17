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
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = "Test Scenario",
                ScenarioMethod = typeof(ScenarioRegistryTests).GetMethod(nameof(RegisterScenario_ValidScenario_ShouldRegisterSuccessfully)),
                TestInstance = this
            };

            // Act
            registry.RegisterScenario(scenario);

            // Assert
            var foundScenario = registry.FindScenario("Test Feature", "Test Scenario");
            foundScenario.Should().NotBeNull();
            foundScenario!.FeatureName.Should().Be("Test Feature");
            foundScenario.ScenarioName.Should().Be("Test Scenario");
        }

        [Fact]
        public void RegisterScenario_NullScenario_ShouldThrowArgumentNullException()
        {
            // Arrange
            var registry = new ScenarioRegistry();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => registry.RegisterScenario(null!));
            exception.ParamName.Should().Be("scenario");
        }

        [Fact]
        public void RegisterScenario_EmptyFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var scenario = new CallableScenario
            {
                FeatureName = "",
                ScenarioName = "Test Scenario"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => registry.RegisterScenario(scenario));
            exception.ParamName.Should().Be("scenario");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public void RegisterScenario_EmptyScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = ""
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => registry.RegisterScenario(scenario));
            exception.ParamName.Should().Be("scenario");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public void FindScenario_ExistingScenario_ShouldReturnScenario()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = "Test Scenario"
            };
            registry.RegisterScenario(scenario);

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
            var testInstance = new TestScenarioClass();
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = "Test Scenario",
                ScenarioMethod = typeof(TestScenarioClass).GetMethod(nameof(TestScenarioClass.TestScenarioMethod)),
                TestInstance = testInstance
            };
            registry.RegisterScenario(scenario);

            // Act
            var result = await registry.ExecuteScenarioAsync("Test Feature", "Test Scenario");

            // Assert
            result.Should().BeTrue();
            testInstance.WasExecuted.Should().BeTrue();
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
        public async Task ExecuteScenarioAsync_ScenarioWithoutMethod_ShouldReturnFalse()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = "Test Scenario",
                ScenarioMethod = null,
                TestInstance = this
            };
            registry.RegisterScenario(scenario);

            // Act
            var result = await registry.ExecuteScenarioAsync("Test Feature", "Test Scenario");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteScenarioAsync_ScenarioWithoutInstance_ShouldReturnFalse()
        {
            // Arrange
            var registry = new ScenarioRegistry();
            var scenario = new CallableScenario
            {
                FeatureName = "Test Feature",
                ScenarioName = "Test Scenario",
                ScenarioMethod = typeof(TestScenarioClass).GetMethod(nameof(TestScenarioClass.TestScenarioMethod)),
                TestInstance = null
            };
            registry.RegisterScenario(scenario);

            // Act
            var result = await registry.ExecuteScenarioAsync("Test Feature", "Test Scenario");

            // Assert
            result.Should().BeFalse();
        }

        public class TestScenarioClass
        {
            public bool WasExecuted { get; set; }

            public void TestScenarioMethod()
            {
                WasExecuted = true;
            }
        }
    }
}