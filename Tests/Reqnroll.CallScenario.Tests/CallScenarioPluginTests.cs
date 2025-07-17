using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Xunit;
using Reqnroll.CallScenario;
using Reqnroll.Configuration;
using Reqnroll.Plugins;

namespace Reqnroll.CallScenario.Tests
{
    public class CallScenarioPluginTests
    {
        [Fact]
        public void Plugin_ShouldHaveCorrectAssemblyName()
        {
            // Arrange & Act
            var pluginAssemblyName = Assembly.GetAssembly(typeof(CallScenarioPlugin))!.GetName().Name;
            
            // Assert
            pluginAssemblyName.Should().Be("Reqnroll.CallScenario.ReqnrollPlugin");
        }

        [Fact]
        public void ConfigurationDefaults_ShouldAddPluginAssemblyToAdditionalStepAssemblies()
        {
            // Arrange
            var additionalStepAssemblies = new List<string>();
            var config = new ReqnrollConfiguration(
                ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.CultureInfo.InvariantCulture,
                false,
                MissingOrPendingStepsOutcome.Pending,
                false,
                false,
                TimeSpan.Zero,
                BindingSkeletons.StepDefinitionSkeletonStyle.RegexAttribute,
                additionalStepAssemblies,
                false,
                false,
                new string[0],
                ObsoleteBehavior.None,
                false
            );
            
            // Act
            var plugin = new CallScenarioPlugin();
            var pluginType = typeof(CallScenarioPlugin);
            var method = pluginType.GetMethod("OnConfigurationDefaults", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var args = new ConfigurationDefaultsEventArgs(config);
            method?.Invoke(plugin, new object[] { null!, args });
            
            // Assert
            var pluginAssemblyName = Assembly.GetAssembly(typeof(CallScenarioPlugin))!.GetName().Name;
            config.AdditionalStepAssemblies.Should().Contain(pluginAssemblyName);
        }

        [Fact]
        public void ConfigurationDefaults_ShouldNotAddDuplicateAssemblyEntries()
        {
            // Arrange
            var pluginAssemblyName = Assembly.GetAssembly(typeof(CallScenarioPlugin))!.GetName().Name;
            var additionalStepAssemblies = new List<string> { pluginAssemblyName! };
            
            var config = new ReqnrollConfiguration(
                ConfigSource.Default,
                new DependencyConfigurationCollection(),
                new DependencyConfigurationCollection(),
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.CultureInfo.InvariantCulture,
                false,
                MissingOrPendingStepsOutcome.Pending,
                false,
                false,
                TimeSpan.Zero,
                BindingSkeletons.StepDefinitionSkeletonStyle.RegexAttribute,
                additionalStepAssemblies,
                false,
                false,
                new string[0],
                ObsoleteBehavior.None,
                false
            );
            
            // Act
            var plugin = new CallScenarioPlugin();
            var pluginType = typeof(CallScenarioPlugin);
            var method = pluginType.GetMethod("OnConfigurationDefaults", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            var args = new ConfigurationDefaultsEventArgs(config);
            method?.Invoke(plugin, new object[] { null!, args });
            
            // Assert
            config.AdditionalStepAssemblies.Should().ContainSingle(pluginAssemblyName);
        }
    }
}