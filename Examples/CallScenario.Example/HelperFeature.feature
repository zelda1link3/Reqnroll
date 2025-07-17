Feature: Helper Feature
    This feature contains reusable scenarios that can be called from other features

@AuthenticationHelpers
Scenario: User logs in with valid credentials
    Given the user is on the login page
    When the user enters valid username and password
    Then the user should be logged in successfully

@AuthenticationHelpers
Scenario: User logs out
    Given the user is logged in
    When the user clicks logout
    Then the user should be logged out successfully

@DataHelpers
Scenario: Setup Test Data
    Given the database is clean
    When I create test users
    And I create test products
    Then the test data should be available

@DataHelpers
Scenario: Cleanup Test Data
    Given test data exists
    When I remove test users
    And I remove test products
    Then the test data should be cleaned up