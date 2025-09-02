# Exercise.API - API Layer

The API layer provides RESTful endpoints for the Exercise Microservice, built with ASP.NET Core Minimal API for high performance and simplicity.

## ??? Architecture

The API layer follows Clean Architecture principles:
- **Controllers/Endpoints** - HTTP request handling
- **Middleware** - Cross-cutting concerns (authentication, logging, validation)
- **DTOs** - Data transfer objects for API contracts
- **Filters** - Request/response processing

## ?? Project Structure

```
Exercise.API/
??? Controllers/           # API controllers (if using controller-based approach)
??? Endpoints/            # Minimal API endpoint definitions
??? Middleware/           # Custom middleware components
??? DTOs/                 # Data Transfer Objects
??? Filters/              # Action filters and exception handlers
??? Extensions/           # Service collection extensions
??? Program.cs            # Application entry point and configuration
??? appsettings.json      # Application configuration
??? README.md            # This file
```

## ??? API Endpoints

### Exercise Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/exercises/{bodyPart}` | Get exercises by body part | No |
| `GET` | `/api/exercises/{id}` | Get specific exercise | No |
| `POST` | `/api/exercises` | Create custom exercise | Yes |
| `PUT` | `/api/exercises/{id}` | Update exercise | Yes |
| `DELETE` | `/api/exercises/{id}` | Delete exercise | Yes |

### Authentication Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register new user | No |
| `POST` | `/api/auth/login` | User login | No |
| `POST` | `/api/auth/refresh` | Refresh JWT token | No |
| `POST` | `/api/auth/logout` | User logout | Yes |
| `POST` | `/api/auth/external` | Social login | No |

### User Management Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/users/profile` | Get current user profile | Yes |
| `PUT` | `/api/users/profile` | Update user profile | Yes |
| `DELETE` | `/api/users/profile` | Delete user account | Yes |

### Workout Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/workouts` | Get user's workouts | Yes |
| `GET` | `/api/workouts/{id}` | Get specific workout | Yes |
| `POST` | `/api/workouts` | Create new workout | Yes |
| `PUT` | `/api/workouts/{id}` | Update workout | Yes |
| `PUT` | `/api/workouts/{id}/complete` | Complete workout | Yes |
| `DELETE` | `/api/workouts/{id}` | Delete workout | Yes |

### Workout Plan Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/workout-plans` | Get user's workout plans | Yes |
| `GET` | `/api/workout-plans/{id}` | Get specific workout plan | Yes |
| `POST` | `/api/workout-plans` | Create workout plan | Yes |
| `PUT` | `/api/workout-plans/{id}` | Update workout plan | Yes |
| `PUT` | `/api/workout-plans/{id}/activate` | Activate workout plan | Yes |
| `DELETE` | `/api/workout-plans/{id}` | Delete workout plan | Yes |

### Analytics Endpoints
| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `GET` | `/api/analytics/progress` | Get user progress data | Yes |
| `GET` | `/api/analytics/summary` | Get workout summary | Yes |
| `GET` | `/api/analytics/trends` | Get performance trends | Yes |

## ?? Configuration

### Required Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ExerciseDB;..."
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "ExerciseAPI",
    "Audience": "ExerciseApp",
    "ExpirationMinutes": 60
  },
  "RapidApi": {
    "Host": "exercisedb.p.rapidapi.com",
    "Key": "your-rapidapi-key"
  },
  "GoogleAuth": {
    "ClientId": "your-google-client-id",
    "ClientSecret": "your-google-client-secret"
  },
  "FacebookAuth": {
    "AppId": "your-facebook-app-id",
    "AppSecret": "your-facebook-app-secret"
  }
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` - Development/Staging/Production
- `ASPNETCORE_URLS` - URLs to bind to
- `JWT_SECRET` - JWT secret key (production)
- `RAPIDAPI_KEY` - RapidAPI key for exercise database

## ?? Security

### Authentication
- **JWT Bearer tokens** for API authentication
- **Social login** integration (Google, Facebook)
- **Refresh token** rotation for security

### Authorization
- **Role-based access control** for admin functions
- **Resource-based authorization** for user data
- **Claims-based permissions** for fine-grained control

### Validation
- **Input validation** using FluentValidation
- **Model binding validation** for request DTOs
- **Business rule validation** at domain level

## ?? Middleware Pipeline

1. **Exception Handling** - Global error handling
2. **CORS** - Cross-origin request handling
3. **Authentication** - JWT token validation
4. **Authorization** - Permission checking
5. **Request Logging** - Request/response logging
6. **Rate Limiting** - API rate limiting
7. **Response Compression** - Gzip compression

## ?? Request/Response Examples

### Create Workout
```http
POST /api/workouts
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "name": "Monday Upper Body",
  "exercises": [
    {
      "exerciseId": "550e8400-e29b-41d4-a716-446655440000",
      "sets": 3,
      "reps": 12
    }
  ]
}
```

### Response
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Monday Upper Body",
  "date": "2023-12-01T10:00:00Z",
  "isCompleted": false,
  "exercises": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Push-up",
      "bodyPart": "chest",
      "equipment": null
    }
  ]
}
```

## ?? Error Handling

### Standard Error Response
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "traceId": "0HMQ2V3N5N7QD:00000001",
  "errors": {
    "Name": ["The Name field is required."]
  }
}
```

### HTTP Status Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

## ?? Performance

### Optimizations
- **Minimal API** for reduced overhead
- **Response caching** for static data
- **Database connection pooling**
- **Async/await** patterns throughout
- **Response compression** (Gzip)

### Monitoring
- **Application Insights** integration
- **Health checks** for dependencies
- **Metrics collection** for performance
- **Structured logging** with Serilog

## ?? Testing

### Testing Strategy
```bash
# Run API integration tests
dotnet test Exercise.API.Tests

# Run specific test category
dotnet test --filter Category=Integration

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories
- **Unit Tests** - Individual endpoint logic
- **Integration Tests** - Full request/response cycle
- **Contract Tests** - API contract validation
- **Performance Tests** - Load and stress testing

## ?? Deployment

### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Exercise.API/Exercise.API.csproj", "Exercise.API/"]
RUN dotnet restore "Exercise.API/Exercise.API.csproj"
COPY . .
WORKDIR "/src/Exercise.API"
RUN dotnet build "Exercise.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Exercise.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Exercise.API.dll"]
```

### Health Checks
Available at `/health`:
- Database connectivity
- External API availability
- Memory usage
- Disk space

## ?? Related Documentation

- [Domain Layer Documentation](../Exercise.Domain/README.md)
- [Application Layer Documentation](../Exercise.Application/README.md)
- [Infrastructure Layer Documentation](../Exercise.Infrastructure/README.md)