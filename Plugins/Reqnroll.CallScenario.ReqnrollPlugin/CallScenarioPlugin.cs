using System;
using System.Linq;
using System.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Bindings.Discovery;

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
            runtimePluginEvents.CustomizeTestThreadDependencies += OnCustomizeTestThreadDependencies;
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

        private void OnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            // Instantiate all CallableStepsBase classes to ensure their constructors run and scenarios are registered
            var scenarioRegistry = e.ObjectContainer.Resolve<IScenarioRegistry>();
            var bindingRegistryBuilder = e.ObjectContainer.Resolve<IRuntimeBindingRegistryBuilder>();
            
            // Get all binding assemblies - we need to find the test assembly
            // The test assembly is typically the entry assembly or one of the loaded assemblies
            var testAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var bindingAssemblies = bindingRegistryBuilder.GetBindingAssemblies(testAssembly);
            
            // Also check all currently loaded assemblies for CallableStepsBase types
            var allLoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !a.GlobalAssemblyCache)
                .Union(bindingAssemblies)
                .Distinct()
                .ToList();
            
            // Find all types that inherit from CallableStepsBase
            foreach (var assembly in allLoadedAssemblies)
            {
                try
                {
                    var callableStepTypes = assembly.GetTypes()
                        .Where(type => typeof(CallableStepsBase).IsAssignableFrom(type) && 
                                      !type.IsAbstract && 
                                      !type.IsInterface)
                        .ToList();
                    
                    // Instantiate each CallableStepsBase class to trigger scenario registration
                    foreach (var type in callableStepTypes)
                    {
                        try
                        {
                            var constructor = type.GetConstructor(new[] { typeof(IScenarioRegistry) });
                            if (constructor != null)
                            {
                                constructor.Invoke(new object[] { scenarioRegistry });
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue processing other types
                            System.Diagnostics.Debug.WriteLine($"Failed to instantiate {type.Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but continue processing other assemblies
                    System.Diagnostics.Debug.WriteLine($"Failed to process assembly {assembly.FullName}: {ex.Message}");
                }
            }
        }
    }
}