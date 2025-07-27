using System.Reflection;
using Reqnroll.Bindings;
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
            
            // Register our custom context manager that handles nested scenario cleanup
            // We need to wrap the original context manager
            e.ObjectContainer.RegisterFactoryAs<IContextManager>(() =>
            {
                // Get the original context manager from the container
                var originalContextManager = e.ObjectContainer.Resolve<ContextManager>();
                return new ScenarioCallContextManager(originalContextManager);
            });
            
            // Note: ScenarioCallService is now instantiated directly in ScenarioCallSteps to get access to ITestRunner and ScenarioContext
        }
    }
}