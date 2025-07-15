using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Reqnroll;
using Reqnroll.Infrastructure;

namespace Reqnroll.Tests.CallScenario
{
    public class CallScenarioTests
    {
        [Fact]
        public async Task CallScenarioAsync_WithValidParameters_ShouldThrowNotImplementedException()
        {
            // Arrange
            var mockExecutionEngine = new Mock<ITestExecutionEngine>();
            mockExecutionEngine.Setup(x => x.CallScenarioAsync("TestFeature", "TestScenario"))
                .Throws(new NotImplementedException("CallScenario functionality is not fully implemented yet"));
            
            var testRunner = new TestRunner(mockExecutionEngine.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotImplementedException>(() => 
                testRunner.CallScenarioAsync("TestFeature", "TestScenario"));
            
            exception.Message.Should().Contain("CallScenario functionality is not fully implemented yet");
        }

        [Fact]
        public async Task CallScenarioAsync_WithEmptyFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockExecutionEngine = new Mock<ITestExecutionEngine>();
            mockExecutionEngine.Setup(x => x.CallScenarioAsync("", "TestScenario"))
                .Throws(new ArgumentException("Feature name cannot be null or empty", "featureName"));
            
            var testRunner = new TestRunner(mockExecutionEngine.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                testRunner.CallScenarioAsync("", "TestScenario"));
            
            exception.ParamName.Should().Be("featureName");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioAsync_WithNullFeatureName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockExecutionEngine = new Mock<ITestExecutionEngine>();
            mockExecutionEngine.Setup(x => x.CallScenarioAsync(null, "TestScenario"))
                .Throws(new ArgumentException("Feature name cannot be null or empty", "featureName"));
            
            var testRunner = new TestRunner(mockExecutionEngine.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                testRunner.CallScenarioAsync(null, "TestScenario"));
            
            exception.ParamName.Should().Be("featureName");
            exception.Message.Should().Contain("Feature name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioAsync_WithEmptyScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockExecutionEngine = new Mock<ITestExecutionEngine>();
            mockExecutionEngine.Setup(x => x.CallScenarioAsync("TestFeature", ""))
                .Throws(new ArgumentException("Scenario name cannot be null or empty", "scenarioName"));
            
            var testRunner = new TestRunner(mockExecutionEngine.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                testRunner.CallScenarioAsync("TestFeature", ""));
            
            exception.ParamName.Should().Be("scenarioName");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public async Task CallScenarioAsync_WithNullScenarioName_ShouldThrowArgumentException()
        {
            // Arrange
            var mockExecutionEngine = new Mock<ITestExecutionEngine>();
            mockExecutionEngine.Setup(x => x.CallScenarioAsync("TestFeature", null))
                .Throws(new ArgumentException("Scenario name cannot be null or empty", "scenarioName"));
            
            var testRunner = new TestRunner(mockExecutionEngine.Object);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                testRunner.CallScenarioAsync("TestFeature", null));
            
            exception.ParamName.Should().Be("scenarioName");
            exception.Message.Should().Contain("Scenario name cannot be null or empty");
        }

        [Fact]
        public void CallScenarioAsync_ShouldBeAvailableOnITestRunner()
        {
            // Act & Assert
            typeof(ITestRunner).Should().HaveMethod("CallScenarioAsync", new[] { typeof(string), typeof(string) });
        }
    }
}