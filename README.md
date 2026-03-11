# Exercise Microservice

Exercise Microservice is a .NET 9 fitness-tracking backend built with Clean Architecture, CQRS, EF Core, Minimal APIs, JWT authentication, and SQL Server persistence.

## What It Does

- Manages users, authentication, refresh tokens, workouts, workout plans, exercise logs, and exercise catalogue data.
- Syncs exercises from the configured external provider.
- Exposes analytics summary data for the authenticated user.
- Applies soft delete, optimistic concurrency, rate limiting, health checks, output caching, and structured logging.

## Solution Layout

```text
Exercise.API/                  Minimal API host and composition root
Exercise.Application/          CQRS handlers, DTOs, validators, repository contracts
Exercise.Domain/               Entities, value objects, guards, business rules
Exercise.Infrastructure/       EF Core, repositories, external provider integration
Exercise.Application.Tests/    Application-layer unit tests
Exercise.API.IntegrationTests/ API and persistence integration tests
```

## Architecture

- `Exercise.Domain` has no project dependencies.
- `Exercise.Application` depends on `Exercise.Domain`.
- `Exercise.Infrastructure` depends on `Exercise.Domain` and `Exercise.Application`.
- `Exercise.API` references `Exercise.Application` and `Exercise.Infrastructure`.

## Current API Surface

- Auth
  - `POST /api/users/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- Users
  - `GET /api/users/{id}`
  - `PUT /api/users/{id}/profile`
  - `DELETE /api/users/{id}`
- Exercises
  - `GET /api/exercises`
  - `GET /api/exercises/{id}`
  - `GET /api/exercises/bodypart/{bodyPart}`
  - `POST /api/exercises` (Admin)
  - `PUT /api/exercises/{id}` (Admin)
  - `DELETE /api/exercises/{id}` (Admin)
  - `POST /api/exercises/sync` (Admin)
- Workouts
  - `GET /api/workouts`
  - `GET /api/workouts/{id}`
  - `POST /api/workouts`
  - `PUT /api/workouts/{id}`
  - `POST /api/workouts/{id}/complete`
  - `DELETE /api/workouts/{id}`
  - `POST /api/workouts/{id}/exercises`
  - `DELETE /api/workouts/{id}/exercises/{exerciseId}`
- Workout plans
  - `GET /api/workout-plans`
  - `GET /api/workout-plans/{id}`
  - `POST /api/workout-plans`
  - `PUT /api/workout-plans/{id}`
  - `POST /api/workout-plans/{id}/activate`
  - `DELETE /api/workout-plans/{id}`
  - `POST /api/workout-plans/{id}/workouts`
  - `DELETE /api/workout-plans/{id}/workouts/{workoutId}`
- Exercise logs
  - `GET /api/exercise-logs`
  - `GET /api/exercise-logs/{id}`
  - `POST /api/exercise-logs`
  - `POST /api/exercise-logs/{id}/entries`
  - `POST /api/exercise-logs/{id}/complete`
  - `DELETE /api/exercise-logs/{id}`
- Analytics
  - `GET /api/analytics/workout-summary`
- Health
  - `GET /health`
  - `GET /health/detail`

## Operational Notes

- Auth routes are rate limited.
- API routes are rate limited.
- Exercise catalogue reads use output caching.
- Most entities use soft delete with a query filter.
- Mutable aggregates use optimistic concurrency. Conflicting writes are returned as `409 Conflict` with `application/problem+json`.
- User-owned resources preserve `403` for foreign resources and `404` for missing resources.

## Local Development

Prerequisites:

- .NET 9 SDK
- SQL Server or LocalDB
- `dotnet ef` CLI tools

Configuration:

- `Exercise.API/appsettings.json` contains non-secret defaults.
- Secrets such as `ConnectionStrings:DefaultConnection`, `Jwt:Key`, and `RapidApi:Key` are expected through user secrets or environment variables.

Useful commands:

```bash
dotnet restore
dotnet build Exercise-Microservice.sln
dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API
dotnet run --project Exercise.API
dotnet test Exercise-Microservice.sln
```

Swagger is enabled in development at `/swagger`.

## Database and Migrations

The API does not auto-apply migrations on startup. Apply them explicitly:

```bash
dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API
```

The latest migration is `AddConcurrencyTokensAndIndexes`, which brings the schema in line with the current model and adds concurrency/indexing changes needed for the hardened write paths.

## Test Status

Current verified command:

```bash
dotnet test Exercise-Microservice.sln
```

Current result: `152` tests passing across unit and integration suites.

## Layer Docs

- [API](./Exercise.API/README.md)
- [Application](./Exercise.Application/README.md)
- [Domain](./Exercise.Domain/README.md)
- [Infrastructure](./Exercise.Infrastructure/README.md)
- [Application Tests](./Exercise.Application.Tests/README.md)
