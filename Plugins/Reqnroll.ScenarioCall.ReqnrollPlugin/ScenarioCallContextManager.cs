using System;
using Reqnroll.Bindings;
using Reqnroll.Infrastructure;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    /// <summary>
    /// A context manager wrapper that prevents cleanup operations during nested scenario calls.
    /// This ensures that context cleanup only happens at the top-level scenario, not for called scenarios.
    /// </summary>
    public class ScenarioCallContextManager : IContextManager
    {
        private readonly IContextManager _innerContextManager;
        private static readonly object _lockObject = new object();
        private static bool _isInNestedScenarioCall = false;
        
        public ScenarioCallContextManager(IContextManager innerContextManager)
        {
            _innerContextManager = innerContextManager ?? throw new ArgumentNullException(nameof(innerContextManager));
        }

        public TestThreadContext TestThreadContext => _innerContextManager.TestThreadContext;
        public FeatureContext FeatureContext => _innerContextManager.FeatureContext;
        public ScenarioContext ScenarioContext => _innerContextManager.ScenarioContext;
        public ScenarioStepContext StepContext => _innerContextManager.StepContext;
        public StepDefinitionType? CurrentTopLevelStepDefinitionType => _innerContextManager.CurrentTopLevelStepDefinitionType;

        public void InitializeFeatureContext(FeatureInfo featureInfo)
        {
            _innerContextManager.InitializeFeatureContext(featureInfo);
        }

        public void CleanupFeatureContext()
        {
            _innerContextManager.CleanupFeatureContext();
        }

        public void InitializeScenarioContext(ScenarioInfo scenarioInfo, RuleInfo ruleInfo)
        {
            _innerContextManager.InitializeScenarioContext(scenarioInfo, ruleInfo);
        }

        public void CleanupScenarioContext()
        {
            _innerContextManager.CleanupScenarioContext();
        }

        public void InitializeStepContext(StepInfo stepInfo)
        {
            _innerContextManager.InitializeStepContext(stepInfo);
        }

        public void CleanupStepContext()
        {
            // Only cleanup step context if we're not in a nested scenario call
            lock (_lockObject)
            {
                if (!_isInNestedScenarioCall)
                {
                    _innerContextManager.CleanupStepContext();
                }
            }
        }

        /// <summary>
        /// Marks the beginning of a nested scenario call. During this time, step context cleanup will be suppressed.
        /// </summary>
        public static void EnterNestedScenarioCall()
        {
            lock (_lockObject)
            {
                _isInNestedScenarioCall = true;
            }
        }

        /// <summary>
        /// Marks the end of a nested scenario call. Step context cleanup will be re-enabled.
        /// </summary>
        public static void ExitNestedScenarioCall()
        {
            lock (_lockObject)
            {
                _isInNestedScenarioCall = false;
            }
        }

        /// <summary>
        /// Checks if we're currently executing within a nested scenario call.
        /// </summary>
        public static bool IsInNestedScenarioCall
        {
            get
            {
                lock (_lockObject)
                {
                    return _isInNestedScenarioCall;
                }
            }
        }
    }
}