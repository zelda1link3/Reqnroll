using System;
using System.Collections.Generic;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Gherkin.Ast;

namespace Reqnroll.ScenarioCall.Generator.ReqnrollPlugin
{
    public class ScenarioCallFeatureGenerator : IFeatureGenerator
    {
        private readonly IFeatureGenerator _baseGenerator;
        private readonly Dictionary<string, ReqnrollFeature> _featureCache = new();

        public ScenarioCallFeatureGenerator(IFeatureGenerator baseGenerator, ReqnrollDocument document)
        {
            _baseGenerator = baseGenerator;
        }

        public CodeNamespace GenerateUnitTestFixture(ReqnrollDocument document, string testClassName, string targetNamespace, out IEnumerable<string> warnings)
        {
            // First, preprocess the document to expand scenario calls
            var expandedDocument = ExpandScenarioCalls(document);
            
            // Then use the base generator to generate the actual code
            return _baseGenerator.GenerateUnitTestFixture(expandedDocument, testClassName, targetNamespace, out warnings);
        }

        private ReqnrollDocument ExpandScenarioCalls(ReqnrollDocument document)
        {
            // For now, let's just return the original document and handle expansion 
            // via preprocessing the feature file content before parsing
            // This is simpler than trying to manipulate the AST
            var feature = document.ReqnrollFeature;
            
            // Check if any scenarios contain scenario call steps
            bool needsExpansion = false;
            foreach (var scenario in feature.ScenarioDefinitions.OfType<Scenario>())
            {
                foreach (var step in scenario.Steps)
                {
                    if (IsScenarioCallStep(step.Text))
                    {
                        needsExpansion = true;
                        break;
                    }
                }
                if (needsExpansion) break;
            }
            
            if (!needsExpansion)
            {
                return document; // No changes needed
            }
            
            // For now, return original document - we'll implement a different approach
            // that works at the text level before the Gherkin parser processes it
            return document;
        }

        private bool IsScenarioCallStep(string stepText)
        {
            return Regex.IsMatch(stepText, @"I call scenario ""([^""]+)"" from feature ""([^""]+)""", RegexOptions.IgnoreCase);
        }

        // These methods will be implemented in a future iteration
        // For now, we'll focus on getting the plugin architecture working
        private List<Step> FindAndExtractScenarioSteps(string scenarioName, string featureName)
        {
            // TODO: Implement scenario lookup and step extraction
            return new List<Step>();
        }

        private ReqnrollFeature FindReferencedFeature(string featureName)
        {
            // TODO: Implement feature file discovery and parsing
            return null;
        }
    }
}