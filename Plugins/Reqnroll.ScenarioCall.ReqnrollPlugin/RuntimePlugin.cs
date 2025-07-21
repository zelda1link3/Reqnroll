using System.Reflection;
using Reqnroll.Infrastructure;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Reqnroll.ScenarioCall.ReqnrollPlugin.RuntimePlugin))]

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public class RuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.ConfigurationDefaults += RuntimePluginEventsOnConfigurationDefaults;
            runtimePluginEvents.CustomizeTestThreadDependencies += RuntimePluginEventsOnCustomizeTestThreadDependencies;
        }

        private void RuntimePluginEventsOnConfigurationDefaults(object sender, ConfigurationDefaultsEventArgs e)
        {
            // Add the plugin assembly to the additional step assemblies so that the step definitions in ScenarioCallSteps are discovered
            var pluginAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            if (!e.ReqnrollConfiguration.AdditionalStepAssemblies.Contains(pluginAssemblyName))
            {
                e.ReqnrollConfiguration.AdditionalStepAssemblies.Add(pluginAssemblyName);
            }
        }

        private void RuntimePluginEventsOnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<ScenarioDiscoveryService, IScenarioDiscoveryService>();
            // Note: ScenarioCallService is now instantiated directly in ScenarioCallSteps to get access to ITestRunner and ScenarioContext
        }
    }
}