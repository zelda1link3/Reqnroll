# Reqnroll ScenarioCall Plugin

A Reqnroll plugin that allows you to call scenarios from other features within your test scenarios.

## Features

This plugin adds step definitions that allow you to call existing scenarios from within other scenarios:

- `Given I call scenario "scenario name" from feature "feature name"`
- `When I call scenario "scenario name" from feature "feature name"`  
- `Then I call scenario "scenario name" from feature "feature name"`

## Installation

1. Add a reference to `Reqnroll.ScenarioCall.ReqnrollPlugin` in your test project
2. Configure the plugin in your `reqnroll.json`:

```json
{
  "plugins": [
    {
      "name": "Reqnroll.ScenarioCall.ReqnrollPlugin"
    }
  ]
}
```

## Usage

### Step 1: Register Scenarios

Before you can call scenarios, you need to register them. This is typically done in a `[BeforeTestRun]` hook:

```csharp
[Binding]
public class ScenarioSetup
{
    [BeforeTestRun]
    public static void RegisterScenarios()
    {
        ScenarioRegistry.Register(
            scenarioName: "User logs in with valid credentials",
            featureName: "Authentication",
            ScenarioRegistry.Given("the user enters username \"testuser\""),
            ScenarioRegistry.And("the user enters password \"testpass\""),
            ScenarioRegistry.When("the user clicks login"),
            ScenarioRegistry.Then("the user should be logged in successfully")
        );
    }
}
```

### Step 2: Call Scenarios

Once registered, you can call scenarios from your feature files:

```gherkin
Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"
        When I add an item to cart
        And I proceed to checkout
        Then the order should be processed successfully
```

## API Reference

### ScenarioRegistry

The `ScenarioRegistry` class provides helper methods for registering scenarios:

- `Register(scenarioName, featureName, ...steps)` - Register a scenario
- `Given(text)` - Create a Given step
- `When(text)` - Create a When step
- `Then(text)` - Create a Then step
- `And(text)` - Create an And step
- `But(text)` - Create a But step

### Step Definitions

The plugin automatically provides these step definitions:

- `Given I call scenario "{scenarioName}" from feature "{featureName}"`
- `When I call scenario "{scenarioName}" from feature "{featureName}"`
- `Then I call scenario "{scenarioName}" from feature "{featureName}"`

## Examples

### Basic Authentication Scenario

```csharp
ScenarioRegistry.Register(
    "User logs in successfully",
    "Authentication",
    ScenarioRegistry.Given("the user is on the login page"),
    ScenarioRegistry.When("the user enters valid credentials"),
    ScenarioRegistry.And("clicks the login button"),
    ScenarioRegistry.Then("the user should be logged in")
);
```

### Calling the Scenario

```gherkin
Scenario: Admin can access admin panel
    Given I call scenario "User logs in successfully" from feature "Authentication"
    When I navigate to admin panel
    Then I should see admin options
```

## Important Notes

1. **Scenario names and feature names must match exactly** - The scenario and feature names used in the registration must match exactly with what you use in the step definitions.

2. **Registration must happen before test execution** - Use `[BeforeTestRun]` hooks to register scenarios before any tests run.

3. **Step definitions must exist** - All steps referenced in registered scenarios must have corresponding step definitions in your test project.

4. **Context sharing** - Called scenarios will execute in the same test context, so they share the same ScenarioContext and FeatureContext.

## Troubleshooting

### "Scenario not found" error

This error occurs when:
- The scenario hasn't been registered
- The scenario name or feature name doesn't match exactly
- The registration happened after the test started

Make sure to register scenarios in a `[BeforeTestRun]` hook and verify the names match exactly.

### Step execution failures

If steps within called scenarios fail, the error will propagate to the calling scenario. Ensure all step definitions exist and work correctly.