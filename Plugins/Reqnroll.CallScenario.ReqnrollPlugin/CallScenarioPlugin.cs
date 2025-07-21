using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Reqnroll.BoDi;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;
using Reqnroll.Bindings.Discovery;
using Reqnroll.Parser;

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

                // Discover scenarios from feature files
                DiscoverScenariosFromFeatureFiles(scenarioRegistry);

                // Also support manual registration for backward compatibility
                InitializeCallableStepsBaseClasses(container, scenarioRegistry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error in InitializeScenarios: {ex.Message}");
            }
        }

        private void DiscoverScenariosFromFeatureFiles(IScenarioRegistry scenarioRegistry)
        {
            try
            {
                // Find the project root directory by looking for .csproj or .sln files
                var currentDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                var projectRoot = FindProjectRoot(currentDirectory);
                
                if (projectRoot == null)
                {
                    System.Diagnostics.Debug.WriteLine("CallScenarioPlugin: Could not find project root directory");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Scanning for feature files from: {projectRoot.FullName}");

                // Find all .feature files
                var featureFiles = projectRoot.GetFiles("*.feature", SearchOption.AllDirectories);
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Found {featureFiles.Length} feature files");

                var parser = new ReqnrollGherkinParser(new ReqnrollGherkinDialectProvider("en"));

                foreach (var featureFile in featureFiles)
                {
                    try
                    {
                        using (var reader = featureFile.OpenText())
                        {
                            var document = parser.Parse(reader, new ReqnrollDocumentLocation(featureFile.FullName));
                            if (document?.ReqnrollFeature != null)
                            {
                                RegisterScenariosFromFeature(scenarioRegistry, document.ReqnrollFeature);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error parsing feature file {featureFile.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error in DiscoverScenariosFromFeatureFiles: {ex.Message}");
            }
        }

        private DirectoryInfo? FindProjectRoot(DirectoryInfo? directory)
        {
            if (directory == null) return null;

            // Look for .csproj, .vbproj, .fsproj, or .sln files
            if (directory.GetFiles("*.csproj").Any() || 
                directory.GetFiles("*.vbproj").Any() || 
                directory.GetFiles("*.fsproj").Any() ||
                directory.GetFiles("*.sln").Any())
            {
                return directory;
            }

            return FindProjectRoot(directory.Parent);
        }

        private void RegisterScenariosFromFeature(IScenarioRegistry scenarioRegistry, ReqnrollFeature feature)
        {
            var featureName = feature.Name;
            System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Processing feature: {featureName}");

            foreach (var scenarioDefinition in feature.ScenarioDefinitions)
            {
                var scenarioName = scenarioDefinition.Name;
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Found scenario: {scenarioName} in feature: {featureName}");

                // Register the scenario with a placeholder execution method
                // For now, we'll register it but provide a meaningful error message
                scenarioRegistry.RegisterScenario(featureName, scenarioName, () =>
                {
                    throw new NotImplementedException($"Scenario '{scenarioName}' from feature '{featureName}' was discovered from a .feature file but automatic execution is not yet implemented.\n\n" +
                        $"To use this scenario, create a step definition class that inherits from CallableStepsBase and manually register this scenario with the steps it should execute.\n\n" +
                        $"Example:\n" +
                        $"[Binding]\n" +
                        $"public class MySteps : CallableStepsBase\n" +
                        $"{{\n" +
                        $"    public MySteps(IScenarioRegistry registry) : base(registry)\n" +
                        $"    {{\n" +
                        $"        RegisterScenario(\"{featureName}\", \"{scenarioName}\", ExecuteMyScenario);\n" +
                        $"    }}\n\n" +
                        $"    public void ExecuteMyScenario()\n" +
                        $"    {{\n" +
                        $"        // Add your scenario steps here\n" +
                        $"    }}\n" +
                        $"}}");
                });
            }
        }

        private void InitializeCallableStepsBaseClasses(IObjectContainer container, IScenarioRegistry scenarioRegistry)
        {
            try
            {
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
                System.Diagnostics.Debug.WriteLine($"CallScenarioPlugin: Error in InitializeCallableStepsBaseClasses: {ex.Message}");
            }
        }
    }
}