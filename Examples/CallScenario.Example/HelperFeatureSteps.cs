using Reqnroll;
using Reqnroll.CallScenario;

namespace CallScenario.Example
{
    [Binding]
    public class HelperFeatureSteps : CallableStepsBase
    {
        private static bool _isLoggedIn;
        private static bool _testDataExists;

        public HelperFeatureSteps(IScenarioRegistry scenarioRegistry) : base(scenarioRegistry)
        {
            // Register scenarios when the class is instantiated
            RegisterScenario("Helper Feature", "User logs in with valid credentials", UserLogsInWithValidCredentials);
            RegisterScenario("Helper Feature", "User logs out", UserLogsOut);
            RegisterScenario("Helper Feature", "Setup Test Data", SetupTestData);
            RegisterScenario("Helper Feature", "Cleanup Test Data", CleanupTestData);
        }

        [Given(@"the user is on the login page")]
        public void GivenTheUserIsOnTheLoginPage()
        {
            Console.WriteLine("✓ User is on the login page");
        }

        [When(@"the user enters valid username and password")]
        public void WhenTheUserEntersValidUsernameAndPassword()
        {
            Console.WriteLine("✓ User enters valid credentials");
        }

        [Then(@"the user should be logged in successfully")]
        public void ThenTheUserShouldBeLoggedInSuccessfully()
        {
            _isLoggedIn = true;
            Console.WriteLine("✓ User is logged in successfully");
        }

        [Given(@"the user is logged in")]
        public void GivenTheUserIsLoggedIn()
        {
            if (!_isLoggedIn)
            {
                Console.WriteLine("⚠ User needs to be logged in first");
                _isLoggedIn = true;
            }
            Console.WriteLine("✓ User is logged in");
        }

        [When(@"the user clicks logout")]
        public void WhenTheUserClicksLogout()
        {
            Console.WriteLine("✓ User clicks logout");
        }

        [Then(@"the user should be logged out successfully")]
        public void ThenTheUserShouldBeLoggedOutSuccessfully()
        {
            _isLoggedIn = false;
            Console.WriteLine("✓ User is logged out successfully");
        }

        [Given(@"the database is clean")]
        public void GivenTheDatabaseIsClean()
        {
            Console.WriteLine("✓ Database is clean");
        }

        [When(@"I create test users")]
        public void WhenICreateTestUsers()
        {
            Console.WriteLine("✓ Creating test users");
        }

        [When(@"I create test products")]
        public void WhenICreateTestProducts()
        {
            Console.WriteLine("✓ Creating test products");
        }

        [Then(@"the test data should be available")]
        public void ThenTheTestDataShouldBeAvailable()
        {
            _testDataExists = true;
            Console.WriteLine("✓ Test data is available");
        }

        [Given(@"test data exists")]
        public void GivenTestDataExists()
        {
            if (!_testDataExists)
            {
                Console.WriteLine("⚠ Test data doesn't exist, creating it");
                _testDataExists = true;
            }
            Console.WriteLine("✓ Test data exists");
        }

        [When(@"I remove test users")]
        public void WhenIRemoveTestUsers()
        {
            Console.WriteLine("✓ Removing test users");
        }

        [When(@"I remove test products")]
        public void WhenIRemoveTestProducts()
        {
            Console.WriteLine("✓ Removing test products");
        }

        [Then(@"the test data should be cleaned up")]
        public void ThenTheTestDataShouldBeCleanedUp()
        {
            _testDataExists = false;
            Console.WriteLine("✓ Test data is cleaned up");
        }

        // Callable scenario methods
        public void UserLogsInWithValidCredentials()
        {
            GivenTheUserIsOnTheLoginPage();
            WhenTheUserEntersValidUsernameAndPassword();
            ThenTheUserShouldBeLoggedInSuccessfully();
        }

        public void UserLogsOut()
        {
            GivenTheUserIsLoggedIn();
            WhenTheUserClicksLogout();
            ThenTheUserShouldBeLoggedOutSuccessfully();
        }

        public void SetupTestData()
        {
            GivenTheDatabaseIsClean();
            WhenICreateTestUsers();
            WhenICreateTestProducts();
            ThenTheTestDataShouldBeAvailable();
        }

        public void CleanupTestData()
        {
            GivenTestDataExists();
            WhenIRemoveTestUsers();
            WhenIRemoveTestProducts();
            ThenTheTestDataShouldBeCleanedUp();
        }
    }
}