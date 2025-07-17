# Reqnroll.CallScenario

A Reqnroll plugin that enables calling scenarios from other features by name, allowing for better test organization and reusability.

## Features

- **Call scenarios from other features**: Use simple step definitions to call scenarios by name
- **Multiple step syntaxes**: Support for various natural language patterns
- **Simple registration**: Register scenarios in your step definition classes
- **Type-safe execution**: Compile-time validation of scenario calls
- **Comprehensive error handling**: Clear error messages for missing scenarios

## Quick Start

1. Install the package:
   ```bash
   dotnet add package Reqnroll.CallScenario
   ```

2. Create a step definition class inheriting from `CallableStepsBase`:
   ```csharp
   [Binding]
   public class AuthenticationSteps : CallableStepsBase
   {
       public AuthenticationSteps(IScenarioRegistry scenarioRegistry) : base(scenarioRegistry)
       {
           RegisterScenario("Authentication", "User logs in", UserLogsIn);
       }

       public void UserLogsIn()
       {
           // Implementation
       }
   }
   ```

3. Call scenarios from feature files:
   ```gherkin
   Feature: Main Feature
   Scenario: Complete Journey
       Given I call scenario "User logs in" from feature "Authentication"
       When I perform main actions
       Then the result should be successful
   ```

## Documentation

Full documentation and examples are available in the [GitHub repository](https://github.com/reqnroll/Reqnroll).