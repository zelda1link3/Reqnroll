Feature: Call Scenario Functionality
    In order to reuse scenarios across features
    As a test developer
    I want to call scenarios from other features by name

Background:
    Given there is a feature file "HelperFeature.feature" in the project as
        """
        Feature: Helper Feature
        
        Scenario: Setup Test Data
            Given I have test data
            When I initialize the system
            Then the system should be ready
        
        Scenario: Cleanup Test Data
            Given the system is ready
            When I cleanup the test data
            Then the system should be clean
        """
    
    Given there is a feature file "MainFeature.feature" in the project as
        """
        Feature: Main Feature
        
        Scenario: Main Test Scenario
            Given I call scenario "Setup Test Data" from feature "Helper Feature"
            When I perform the main test
            Then I call scenario "Cleanup Test Data" from feature "Helper Feature"
        """

Scenario: Should be able to call a scenario from another feature
    Given the step definition for "I call scenario \"(.*)\" from feature \"(.*)\"" is implemented with CallScenarioAsync
    And all other steps are bound and pass
    When I execute the tests
    Then the execution should succeed
    And the called scenarios should be executed

Scenario: Should validate feature name parameter
    Given the step definition for "I call scenario \"(.*)\" from feature \"(.*)\"" is implemented with CallScenarioAsync  
    When I call CallScenarioAsync with empty feature name
    Then an ArgumentException should be thrown

Scenario: Should validate scenario name parameter
    Given the step definition for "I call scenario \"(.*)\" from feature \"(.*)\"" is implemented with CallScenarioAsync
    When I call CallScenarioAsync with empty scenario name
    Then an ArgumentException should be thrown