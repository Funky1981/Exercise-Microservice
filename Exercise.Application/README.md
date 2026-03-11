# Exercise.Application

`Exercise.Application` contains the use-case layer for the service. It owns CQRS handlers, DTOs, validators, mapping profiles, repository contracts, and application exceptions.

## Responsibilities

- Define commands and queries for each feature area.
- Validate requests with FluentValidation.
- Coordinate business workflows through MediatR handlers.
- Map domain entities to DTOs.
- Define repository and service abstractions consumed by handlers.

## Feature Areas

- `Analytics`
- `Auth`
- `ExerciseLogs`
- `Exercises`
- `Users`
- `WorkoutPlans`
- `Workouts`

Each feature follows the same vertical-slice pattern:

- `Commands/`
- `Queries/`
- `Dtos/`
- `Mapping/`

## Important Behaviors

- User-owned resource handlers accept the current authenticated user ID and enforce ownership in the handler.
- Successful write paths load tracked entities and persist with `IUnitOfWork.SaveChangesAsync()` instead of detached `Update()` calls.
- Missing user-owned resources return `404`; foreign resources return `403`.
- Analytics summary queries aggregate counts in the database and only project the narrow duration data needed for totals.
- Concurrency conflicts are surfaced through `ConcurrencyException`.

## Core Abstractions

Key contracts live under `Abstractions/`:

- `Repositories/` for persistence access
- `Services/` for token generation and external data providers

Current repository contracts include tracked fetch methods for command-side work, for example:

- `GetByIdForUpdateAsync(...)`
- `GetOwnedByIdForUpdateAsync(...)`
- `GetOwnedByIdWithExercisesForUpdateAsync(...)`
- `GetSummaryByUserIdAsync(...)`

## Exceptions

The application layer exposes exception types used by the API middleware:

- `NotFoundException`
- `ForbiddenException`
- `ConcurrencyException`

## Dependency Injection

`DependencyInjection.cs` registers:

- MediatR
- FluentValidation validators
- AutoMapper profiles

The application project references only `Exercise.Domain`.

## Typical Request Flow

1. API endpoint creates or binds a command/query.
2. FluentValidation validates the request.
3. MediatR dispatches to a handler.
4. The handler loads domain entities through repository abstractions.
5. Domain methods apply business rules.
6. `IUnitOfWork.SaveChangesAsync()` commits changes.
7. DTOs are returned to the API layer.

## Build

```bash
dotnet build Exercise.Application/Exercise.Application.csproj
```

## Tests

Application-layer unit tests live in [../Exercise.Application.Tests/README.md](../Exercise.Application.Tests/README.md).
