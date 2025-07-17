# Reqnroll.CallScenario Plugin

## Overview

The Reqnroll.CallScenario plugin enables you to call scenarios from other features by name, allowing for better test organization and reusability. This plugin provides a clean way to reuse common test scenarios across multiple features without duplicating code.

## Features

- **Call scenarios from other features**: Use simple step definitions to call scenarios by name
- **Multiple step syntaxes**: Support for various natural language patterns
- **Automatic scenario discovery**: Scenarios are automatically discovered using attributes
- **Type-safe execution**: Compile-time validation of scenario calls
- **Comprehensive error handling**: Clear error messages for missing scenarios

## Installation

Install the NuGet package:

```bash
dotnet add package Reqnroll.CallScenario
```

Or add to your project file:

```xml
<PackageReference Include="Reqnroll.CallScenario" Version="1.0.0" />
```

## Usage

### 1. Mark Your Scenarios

First, mark your reusable scenarios with the `[ReqnrollScenario]` attribute and ensure the feature class has the `[ReqnrollFeature]` attribute:

```csharp
[ReqnrollFeature("Authentication")]
[Binding]
public class AuthenticationSteps
{
    [Given(@"the user is on the login page")]
    public void GivenTheUserIsOnTheLoginPage()
    {
        // Implementation
    }

    [When(@"the user enters valid username and password")]
    public void WhenTheUserEntersValidUsernameAndPassword()
    {
        // Implementation
    }

    [Then(@"the user should be logged in successfully")]
    public void ThenTheUserShouldBeLoggedInSuccessfully()
    {
        // Implementation
    }

    [ReqnrollScenario("User logs in with valid credentials")]
    public void UserLogsInWithValidCredentials()
    {
        GivenTheUserIsOnTheLoginPage();
        WhenTheUserEntersValidUsernameAndPassword();
        ThenTheUserShouldBeLoggedInSuccessfully();
    }
}
```

### 2. Call Scenarios from Feature Files

Use the provided step definitions to call scenarios from other features:

```gherkin
Feature: Main Feature

Scenario: Complete User Journey
    Given I call scenario "User logs in with valid credentials" from feature "Authentication"
    When I perform the main user actions
    Then the main user actions should be successful
    And I call scenario "User logs out" from feature "Authentication"
```

### 3. Available Step Patterns

The plugin supports multiple natural language patterns for calling scenarios:

```gherkin
# Primary pattern
Given I call scenario "Scenario Name" from feature "Feature Name"
When I call scenario "Scenario Name" from feature "Feature Name"
Then I call scenario "Scenario Name" from feature "Feature Name"

# Alternative patterns
Given scenario "Scenario Name" from feature "Feature Name" is called
When scenario "Scenario Name" from feature "Feature Name" is called
Then scenario "Scenario Name" from feature "Feature Name" is called

Given I invoke scenario "Scenario Name" from feature "Feature Name"
When I invoke scenario "Scenario Name" from feature "Feature Name"
Then I invoke scenario "Scenario Name" from feature "Feature Name"

Given I execute scenario "Scenario Name" from feature "Feature Name"
When I execute scenario "Scenario Name" from feature "Feature Name"
Then I execute scenario "Scenario Name" from feature "Feature Name"

Given I run scenario "Scenario Name" from feature "Feature Name"
When I run scenario "Scenario Name" from feature "Feature Name"
Then I run scenario "Scenario Name" from feature "Feature Name"

Given the scenario "Scenario Name" from feature "Feature Name" is executed
When the scenario "Scenario Name" from feature "Feature Name" is executed
Then the scenario "Scenario Name" from feature "Feature Name" is executed
```

## Example

### Helper Feature

```gherkin
Feature: Helper Feature
    This feature contains reusable scenarios

Scenario: Setup Test Data
    Given the database is clean
    When I create test users
    And I create test products
    Then the test data should be available

Scenario: Cleanup Test Data
    Given test data exists
    When I remove test users
    And I remove test products
    Then the test data should be cleaned up
```

### Helper Feature Steps

```csharp
[ReqnrollFeature("Helper Feature")]
[Binding]
public class HelperFeatureSteps
{
    [Given(@"the database is clean")]
    public void GivenTheDatabaseIsClean()
    {
        // Implementation
    }

    [When(@"I create test users")]
    public void WhenICreateTestUsers()
    {
        // Implementation
    }

    [When(@"I create test products")]
    public void WhenICreateTestProducts()
    {
        // Implementation
    }

    [Then(@"the test data should be available")]
    public void ThenTheTestDataShouldBeAvailable()
    {
        // Implementation
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
```

### Main Feature

```gherkin
Feature: Main Feature
    This feature demonstrates calling scenarios from other features

Scenario: Complete User Journey
    Given I call scenario "Setup Test Data" from feature "Helper Feature"
    When I perform the main user actions
    Then the main user actions should be successful
    And I call scenario "Cleanup Test Data" from feature "Helper Feature"
```

## Benefits

1. **Test Reusability**: Share common scenarios across multiple features
2. **Better Organization**: Keep setup and cleanup scenarios in dedicated helper features
3. **Maintainability**: Centralize common test logic in reusable scenarios
4. **Flexibility**: Build complex test flows by combining smaller scenarios
5. **Readability**: Use natural language patterns that make tests easier to understand

## Error Handling

The plugin provides clear error messages for common issues:

- **Missing scenarios**: When a referenced scenario doesn't exist
- **Invalid parameters**: When feature name or scenario name is empty
- **Execution failures**: When scenario execution fails with detailed error information

## Requirements

- .NET Standard 2.0+
- Reqnroll 3.0+

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the BSD 3-Clause License - see the LICENSE file for details.