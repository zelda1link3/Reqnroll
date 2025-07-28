using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.Parser;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(Reqnroll.ScenarioCall.Generator.ReqnrollPlugin.GeneratorPlugin))]

namespace Reqnroll.ScenarioCall.Generator.ReqnrollPlugin
{
    public class GeneratorPlugin : IGeneratorPlugin
    {
        public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            generatorPluginEvents.RegisterDependencies += (sender, args) =>
            {
                args.ObjectContainer.RegisterTypeAs<ScenarioCallFeatureGeneratorProvider, Reqnroll.Generator.UnitTestConverter.IFeatureGeneratorProvider>();
            };
        }
    }
}