Feature: Main Feature
    This feature demonstrates calling scenarios from other features

Scenario: Complete User Journey
    Given I call scenario "Setup Test Data" from feature "Helper Feature"
    When I call scenario "User logs in with valid credentials" from feature "Helper Feature"
    And I perform the main user actions
    Then the main user actions should be successful
    And I call scenario "User logs out" from feature "Helper Feature"
    And I call scenario "Cleanup Test Data" from feature "Helper Feature"

Scenario: Another Example
    Given I call scenario "User logs in with valid credentials" from feature "Helper Feature"
    When I check the user dashboard
    Then the dashboard should display correctly
    And I call scenario "User logs out" from feature "Helper Feature"