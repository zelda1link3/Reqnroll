using System;
using System.Collections.Generic;
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
                System.Diagnostics.Debug.WriteLine("CallScenarioPlugin: Registered ScenarioRegistry as singleton");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("CallScenarioPlugin: IScenarioRegistry already registered");
            }
        }

        private void OnCustomizeTestThreadDependencies(object sender, CustomizeTestThreadDependenciesEventArgs e)
        {
            // Initialize scenarios by finding and instantiating CallableStepsBase classes
            InitializeScenarios(e.ObjectContainer);
        }

        private void InitializeScenarios(IObjectContainer container)
        {
            try
            {
                var scenarioRegistry = container.Resolve<IScenarioRegistry>();
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Retrieved ScenarioRegistry instance: {scenarioRegistry.GetHashCode()}");

                // Find all loaded assemblies that might contain CallableStepsBase classes
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !a.GlobalAssemblyCache && !a.GetName().Name.StartsWith("System."))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Checking {assemblies.Count} assemblies for CallableStepsBase classes");

                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var callableStepTypes = assembly.GetTypes()
                            .Where(type => typeof(CallableStepsBase).IsAssignableFrom(type) && 
                                          !type.IsAbstract && 
                                          !type.IsInterface)
                            .ToList();

                        System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Found {callableStepTypes.Count} CallableStepsBase types in {assembly.GetName().Name}");

                        foreach (var type in callableStepTypes)
                        {
                            try
                            {
                                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Instantiating {type.Name}");
                                
                                // Try to create instance using DI container first
                                object? instance = null;
                                try
                                {
                                    instance = container.Resolve(type);
                                    System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Successfully resolved {type.Name} from DI container");
                                }
                                catch
                                {
                                    // If DI resolution fails, try manual instantiation
                                    var constructor = type.GetConstructor(new[] { typeof(IScenarioRegistry) });
                                    if (constructor != null)
                                    {
                                        instance = constructor.Invoke(new object[] { scenarioRegistry });
                                        System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Successfully instantiated {type.Name} manually");
                                    }
                                }

                                if (instance != null)
                                {
                                    System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Successfully created instance of {type.Name}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Failed to create instance of {type.Name}");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error instantiating {type.Name}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error processing assembly {assembly.GetName().Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error in InitializeScenarios: {ex.Message}");
            }
        }
    }
}