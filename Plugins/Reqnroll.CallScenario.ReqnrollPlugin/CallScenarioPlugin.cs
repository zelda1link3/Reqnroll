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
            runtimePluginEvents.CustomizeGlobalDependencies += OnCustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeTestThreadDependencies += OnCustomizeTestThreadDependencies;
        }

        private void OnCustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
            // Register the scenario registry as a singleton
            if (!e.ObjectContainer.IsRegistered<IScenarioRegistry>())
            {
                e.ObjectContainer.RegisterTypeAs<ScenarioRegistry, IScenarioRegistry>();
            }
        }

        private void OnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            // Discover scenarios from the test assembly
            var scenarioRegistry = e.ObjectContainer.Resolve<IScenarioRegistry>();
            
            // Get the test assembly - this should be the assembly containing the test scenarios
            var testAssembly = GetTestAssembly();
            if (testAssembly != null)
            {
                scenarioRegistry.DiscoverScenarios(testAssembly);
            }
        }

        private Assembly? GetTestAssembly()
        {
            try
            {
                // Get the calling assembly which should be the test assembly
                var callingAssembly = Assembly.GetCallingAssembly();
                if (callingAssembly != null && !callingAssembly.GetName().Name?.StartsWith("Reqnroll") == true)
                {
                    return callingAssembly;
                }

                // Fallback: try to get the entry assembly
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null && !entryAssembly.GetName().Name?.StartsWith("Reqnroll") == true)
                {
                    return entryAssembly;
                }

                // If all else fails, look through loaded assemblies for one that contains test scenarios
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in loadedAssemblies)
                {
                    var assemblyName = assembly.GetName().Name;
                    if (assemblyName != null && 
                        !assemblyName.StartsWith("System") && 
                        !assemblyName.StartsWith("Microsoft") &&
                        !assemblyName.StartsWith("Reqnroll") &&
                        !assemblyName.StartsWith("netstandard") &&
                        !assemblyName.StartsWith("mscorlib"))
                    {
                        // Check if this assembly has any types with ReqnrollFeatureAttribute
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            if (type.GetCustomAttribute<ReqnrollFeatureAttribute>() != null)
                            {
                                return assembly;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}