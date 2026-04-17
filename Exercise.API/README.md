# Exercise.API

`Exercise.API` is the Minimal API host for the service. It wires together the application and infrastructure layers, configures authentication and middleware, and exposes the HTTP contract.

## Responsibilities

- Configure dependency injection.
- Configure JWT bearer authentication and the `Admin` authorization policy.
- Register middleware for correlation IDs, request logging, exception handling, caching, health checks, and rate limiting.
- Map versioned Minimal API endpoints.

## Middleware and Host Features

- Serilog request logging
- Correlation ID middleware
- Global exception handling with ProblemDetails responses
- JWT bearer authentication
- Authorization with an `Admin` policy
- Fixed-window rate limiting
- Output caching for the exercise catalogue
- Health checks for the EF Core DbContext
- Swagger and OpenAPI in development

## Error Semantics

- `400 Bad Request` for validation failures
- `401 Unauthorized` for invalid or missing auth
- `403 Forbidden` for authenticated access to another user's resource
- `404 Not Found` for missing resources
- `409 Conflict` for optimistic concurrency conflicts
- `500 Internal Server Error` for unexpected failures

All middleware-generated errors are returned as `application/problem+json`.

## Route Summary

### Public routes

- `POST /api/users/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /health`
- `GET /health/detail`

### Authenticated routes

- `GET /api/users/{id}`
- `PUT /api/users/{id}/profile`
- `DELETE /api/users/{id}`
- `GET /api/exercises`
- `GET /api/exercises/{id}`
- `GET /api/exercises/bodypart/{bodyPart}`
- `GET /api/workouts`
- `GET /api/workouts/{id}`
- `POST /api/workouts`
- `PUT /api/workouts/{id}`
- `POST /api/workouts/{id}/complete`
- `DELETE /api/workouts/{id}`
- `POST /api/workouts/{id}/exercises`
- `DELETE /api/workouts/{id}/exercises/{exerciseId}`
- `GET /api/workout-plans`
- `GET /api/workout-plans/{id}`
- `POST /api/workout-plans`
- `PUT /api/workout-plans/{id}`
- `POST /api/workout-plans/{id}/activate`
- `DELETE /api/workout-plans/{id}`
- `POST /api/workout-plans/{id}/workouts`
- `DELETE /api/workout-plans/{id}/workouts/{workoutId}`
- `GET /api/exercise-logs`
- `GET /api/exercise-logs/{id}`
- `POST /api/exercise-logs`
- `POST /api/exercise-logs/{id}/entries`
- `POST /api/exercise-logs/{id}/complete`
- `DELETE /api/exercise-logs/{id}`
- `GET /api/analytics/workout-summary`

### Admin-only routes

- `POST /api/exercises`
- `PUT /api/exercises/{id}`
- `DELETE /api/exercises/{id}`
- `POST /api/exercises/sync`

## Configuration

Expected configuration keys:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "ExerciseMicroservice",
    "Audience": "ExerciseMicroserviceUsers",
    "ExpiryDays": 7
  },
  "RapidApi": {
    "Host": "exercisedb.p.rapidapi.com",
    "Key": ""
  }
}
```

Secrets should be supplied through user secrets or environment variables.

The current sync path is provider-agnostic at the application layer. If you replace the current catalogue provider, the main changes are expected in the infrastructure layer:

- implement `IExerciseDataProvider`
- register the new provider in `Exercise.Infrastructure/Data/DependencyInjection.cs`
- update provider-specific config keys and `HttpClient` setup

The `POST /api/exercises/sync` endpoint and the local exercise database remain the stable contract for the rest of the system.

## Versioning

- API versioning is enabled.
- Clients can specify the version through the `api-version` header or query string.
- The current mapped version is `1.0`.

## Running the API

```bash
dotnet run --project Exercise.API
```

Development-only tools:

- Swagger UI: `/swagger`
- OpenAPI document: `/openapi/v1.json`

## Health Checks

- `/health` returns liveness status.
- `/health/detail` returns JSON with per-check status and duration.
