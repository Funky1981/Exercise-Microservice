# Exercise.Application.Tests

`Exercise.Application.Tests` contains unit tests for the application layer.

## Scope

The suite covers:

- Command handlers
- Query handlers
- FluentValidation validators
- AutoMapper configuration

## Test Style

- xUnit for the test runner
- Moq for doubles
- FluentAssertions for assertions
- Shared test builders and mock factories under `TestHelpers/`

## Helpers

- `MockFactory.cs` centralizes repository, mapper, token-service, and unit-of-work mocks.
- `TestDataBuilder.cs` creates consistent domain test data.

## Coverage Focus

The current suite verifies:

- Auth flows
- User profile and deletion flows
- Exercise CRUD handlers
- Workout CRUD and exercise-assignment handlers
- Workout plan CRUD and activation handlers
- Exercise log handlers
- Analytics summary handlers
- Validator behavior for request contracts

## Run the Tests

```bash
dotnet test Exercise.Application.Tests/Exercise.Application.Tests.csproj
```

The full solution test command also runs the API integration suite:

```bash
dotnet test Exercise-Microservice.sln
```
