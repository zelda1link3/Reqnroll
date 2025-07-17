using Reqnroll;
using Reqnroll.CallScenario;

namespace CallScenario.Example
{
    [ReqnrollFeature("Helper Feature")]
    [Binding]
    public class HelperFeatureSteps
    {
        private static bool _isLoggedIn;
        private static bool _testDataExists;

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

        [ReqnrollScenario("User logs in with valid credentials")]
        public void UserLogsInWithValidCredentials()
        {
            GivenTheUserIsOnTheLoginPage();
            WhenTheUserEntersValidUsernameAndPassword();
            ThenTheUserShouldBeLoggedInSuccessfully();
        }

        [ReqnrollScenario("User logs out")]
        public void UserLogsOut()
        {
            GivenTheUserIsLoggedIn();
            WhenTheUserClicksLogout();
            ThenTheUserShouldBeLoggedOutSuccessfully();
        }

        [ReqnrollScenario("Setup Test Data")]
        public void SetupTestData()
        {
            GivenTheDatabaseIsClean();
            WhenICreateTestUsers();
            WhenICreateTestProducts();
            ThenTheTestDataShouldBeAvailable();
        }

        [ReqnrollScenario("Cleanup Test Data")]
        public void CleanupTestData()
        {
            GivenTestDataExists();
            WhenIRemoveTestUsers();
            WhenIRemoveTestProducts();
            ThenTheTestDataShouldBeCleanedUp();
        }
    }
}