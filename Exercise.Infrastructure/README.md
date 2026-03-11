# Exercise.Infrastructure

`Exercise.Infrastructure` implements persistence and external integrations for the service.

## Responsibilities

- Configure `ExerciseDbContext`
- Map domain entities to SQL Server tables
- Implement repository contracts from `Exercise.Application`
- Implement `IUnitOfWork`
- Integrate with the external exercise data provider

## Data Access

### DbContext

`ExerciseDbContext` is responsible for:

- Applying all entity configurations from the assembly
- Managing `DbSet`s for the main aggregates
- Converting deletes into soft deletes when the entity supports it
- Stamping `UpdatedAt`
- Stamping a shadow `ConcurrencyToken` on added and modified entities

### Entity Configuration

Current model features include:

- Soft-delete query filters
- Shadow audit fields
- Shadow optimistic concurrency tokens
- Composite indexes for common access patterns:
  - `Workouts(UserId, Date)`
  - `WorkoutPlans(UserId, StartDate)`
  - `ExerciseLogs(UserId, Date)`
- JSON conversion for `Analytics.Data`

### Repositories

Concrete repositories currently exist for:

- `Exercise`
- `User`
- `Workout`
- `WorkoutPlan`
- `ExerciseLog`

Read-heavy methods use `AsNoTracking()` where appropriate. Command-side methods expose tracked fetches for mutation paths.

## Unit of Work and Concurrency

`UnitOfWork.SaveChangesAsync()` wraps EF Core `SaveChangesAsync()` and translates `DbUpdateConcurrencyException` into the application-layer `ConcurrencyException`.

This allows the API layer to return `409 Conflict` on stale overlapping writes.

## SQL Server Configuration

`AddInfrastructure()` configures:

- SQL Server via `UseSqlServer(...)`
- EF Core retry-on-failure for transient database errors
- Repository registrations
- `IUnitOfWork`
- Named `HttpClient` for the external exercise provider

## External Provider

The current exercise sync implementation uses:

- `IExerciseDataProvider`
- `RapidApiExerciseProvider`

## Migrations

Useful commands:

```bash
dotnet ef migrations add <Name> --project Exercise.Infrastructure --startup-project Exercise.API
dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API
dotnet ef migrations script --project Exercise.Infrastructure --startup-project Exercise.API
```

The latest migration is `AddConcurrencyTokensAndIndexes`.

## Build

```bash
dotnet build Exercise.Infrastructure/Exercise.Infrastructure.csproj
```
