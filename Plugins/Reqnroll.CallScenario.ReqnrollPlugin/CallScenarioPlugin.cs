using System;
using System.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Reqnroll.CallScenario.CallScenarioPlugin))]

namespace Reqnroll.CallScenario
{
    /// <summary>
    /// Main plugin class for CallScenario functionality
    /// </summary>
    public class CallScenarioPlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.ConfigurationDefaults += OnConfigurationDefaults;
            runtimePluginEvents.CustomizeGlobalDependencies += OnCustomizeGlobalDependencies;
        }

        private void OnConfigurationDefaults(object sender, ConfigurationDefaultsEventArgs e)
        {
            // Add the plugin assembly to the additional step assemblies so that CallScenarioSteps can be discovered
            var pluginAssembly = Assembly.GetExecutingAssembly();
            var assemblyName = pluginAssembly.GetName().Name;
            
            if (!e.ReqnrollConfiguration.AdditionalStepAssemblies.Contains(assemblyName))
            {
                e.ReqnrollConfiguration.AdditionalStepAssemblies.Add(assemblyName);
            }
        }

        private void OnCustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
            // Register the scenario registry as a singleton
            if (!e.ObjectContainer.IsRegistered<IScenarioRegistry>())
            {
                e.ObjectContainer.RegisterTypeAs<ScenarioRegistry, IScenarioRegistry>();
            }
        }
    }
}