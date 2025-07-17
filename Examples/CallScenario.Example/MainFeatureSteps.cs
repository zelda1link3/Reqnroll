using Reqnroll;

namespace CallScenario.Example
{
    [Binding]
    public class MainFeatureSteps
    {
        [When(@"I perform the main user actions")]
        public void WhenIPerformTheMainUserActions()
        {
            Console.WriteLine("✓ Performing main user actions");
        }

        [Then(@"the main user actions should be successful")]
        public void ThenTheMainUserActionsShouldBeSuccessful()
        {
            Console.WriteLine("✓ Main user actions were successful");
        }

        [When(@"I check the user dashboard")]
        public void WhenICheckTheUserDashboard()
        {
            Console.WriteLine("✓ Checking user dashboard");
        }

        [Then(@"the dashboard should display correctly")]
        public void ThenTheDashboardShouldDisplayCorrectly()
        {
            Console.WriteLine("✓ Dashboard displays correctly");
        }
    }
}