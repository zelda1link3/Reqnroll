using System.Threading.Tasks;

namespace Reqnroll.ScenarioCall.ReqnrollPlugin
{
    public interface IScenarioCallService
    {
        Task CallScenarioAsync(string scenarioName, string featureName);
    }
}