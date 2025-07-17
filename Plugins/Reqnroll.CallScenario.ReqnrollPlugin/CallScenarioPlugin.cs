using System;
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
            runtimePluginEvents.CustomizeGlobalDependencies += OnCustomizeGlobalDependencies;
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