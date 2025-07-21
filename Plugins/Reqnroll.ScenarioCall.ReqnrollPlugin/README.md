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

### Automatic Discovery (New!)

The plugin now automatically discovers scenarios from your Reqnroll-generated code! No manual registration needed.

Simply use scenarios that exist in your feature files:

```gherkin
Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"
        When I add an item to cart
        And I proceed to checkout
        Then the order should be processed successfully
```

The plugin will automatically find the "User logs in with valid credentials" scenario from the "Authentication" feature.

### Manual Registration (Fallback)

If automatic discovery doesn't work for your scenario, you can still register scenarios manually in a `[BeforeTestRun]` hook:

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

## How It Works

The plugin uses reflection to scan assemblies for classes marked with `GeneratedCodeAttribute` with "Reqnroll" as the tool name. It then:

1. Extracts feature names from `FeatureInfo` fields
2. Identifies scenario names from test method attributes
3. Makes these scenarios available for calling

> **Note**: Currently, the automatic discovery identifies scenarios but does not extract their step definitions from the generated code. Called scenarios will be found but may not execute steps unless manually registered with step definitions.

## API Reference

### ScenarioRegistry

The `ScenarioRegistry` class provides helper methods for manual scenario registration:

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

### Using Automatic Discovery

```gherkin
Feature: Authentication
    Scenario: User logs in with valid credentials
        Given there is a user registered with user name "Trillian" and password "139139"
        When the user attempts to log in with user name "Trillian" and password "139139"
        Then the login attempt should be successful
        And the user should be authenticated

Feature: Order Processing
    Scenario: Process order for logged in user
        Given I call scenario "User logs in with valid credentials" from feature "Authentication"
        When I add an item to cart
        Then the order should be processed successfully
```

### Manual Registration with Steps

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

## Important Notes

1. **Scenario names and feature names must match exactly** - The scenario and feature names used must match exactly with those defined in your feature files.

2. **Automatic discovery limitations** - Currently, automatic discovery finds scenarios but doesn't extract step definitions from generated code. For full functionality, use manual registration.

3. **Step definitions must exist** - All steps referenced in registered scenarios must have corresponding step definitions in your test project.

4. **Context sharing** - Called scenarios will execute in the same test context, so they share the same ScenarioContext and FeatureContext.

## Troubleshooting

### "Scenario not found" error

This error occurs when:
- The scenario doesn't exist in your feature files
- The scenario name or feature name doesn't match exactly
- The scenario hasn't been manually registered (if using manual registration)

Make sure scenario and feature names match exactly as they appear in your `.feature` files.

### Step execution failures

If steps within called scenarios fail, the error will propagate to the calling scenario. Ensure all step definitions exist and work correctly.