# Exercise Microservice

A comprehensive .NET 9 microservice for fitness tracking and exercise management, built with Clean Architecture principles and Domain-Driven Design.

## 🏗️ Architecture Overview

This microservice follows Clean Architecture patterns with clear separation of concerns:

```
Exercise-Microservice/
├── Exercise.API/           # API Layer - RESTful endpoints and controllers
├── Exercise.Domain/        # Domain Layer - Core business entities and logic
├── Exercise.Application/   # Application Layer - Use cases and services
├── Exercise.Infrastructure/# Infrastructure Layer - Data access and external services
└── Exercise-Microservice.sln
```

## 🚀 Features

### Core Capabilities
- **Exercise Management** - Browse, search, and manage exercises
- **Workout Planning** - Create and manage workout plans
- **Progress Tracking** - Log workouts and track fitness progress
- **User Management** - User profiles with authentication
- **Analytics** - Workout statistics and progress reports

### Technical Features
- **Clean Architecture** - Maintainable, testable, and scalable design
- **Domain-Driven Design** - Rich domain models with business logic
- **Minimal API** - Lightweight, high-performance endpoints
- **JWT Authentication** - Secure authentication with social login support
- **External API Integration** - RapidAPI Exercise Database integration
- **Docker Support** - Containerized deployment
- **OpenAPI/Swagger** - Comprehensive API documentation

## 🛠️ Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET | 9.0 |
| API | ASP.NET Core Minimal API | 9.0 |
| Authentication | JWT Bearer + Social Auth | - |
| Database | Entity Framework Core | 9.0 |
| External API | RapidAPI Exercise Database | - |
| Container | Docker | Latest |
| Documentation | Swagger/OpenAPI | 3.0 |

## 🏃‍♂️ Quick Start

### Prerequisites
- .NET 9 SDK
- Docker (optional)
- RapidAPI key for Exercise Database

### Setup
1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Exercise-Microservice
   ```

2. **Configure API keys**
   ```bash
   dotnet user-secrets set "RapidApi:Key" "your-rapidapi-key"
   ```

3. **Run the application**
   ```bash
   dotnet run --project Exercise.API
   ```

4. **Access the API**
   - API: `https://localhost:7041`
   - Swagger UI: `https://localhost:7041/swagger`

### Docker Deployment
```bash
docker build -t exercise-microservice .
docker run -p 8080:80 exercise-microservice
```

## 📋 API Endpoints

### Exercise Management
- `GET /api/exercises/{bodyPart}` - Get exercises by body part
- `GET /api/exercises/{id}` - Get specific exercise
- `POST /api/exercises` - Create custom exercise

### User Management
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `GET /api/users/profile` - Get user profile
- `PUT /api/users/profile` - Update user profile

### Workout Management
- `POST /api/workouts` - Create workout
- `GET /api/workouts/{id}` - Get workout details
- `PUT /api/workouts/{id}/complete` - Complete workout
- `GET /api/workouts/user/{userId}` - Get user workouts

### Analytics
- `GET /api/analytics/progress/{userId}` - Get user progress
- `GET /api/analytics/summary/{userId}` - Get workout summary

## 🏛️ Domain Model

### Core Entities
- **User** - User profiles with authentication
- **Exercise** - Individual exercises with details
- **Workout** - Collections of exercises for a session
- **WorkoutPlan** - Structured workout programs
- **ExerciseLog** - Records of completed workouts
- **Analytics** - Progress tracking and statistics

### Value Objects
- **Height** - Height measurements with unit conversions
- **Weight** - Weight measurements with unit conversions

## 🔒 Security

- **JWT Authentication** - Stateless token-based auth
- **Social Login** - Google, Facebook integration
- **Input Validation** - Domain-level validation with Guard patterns
- **CORS** - Configurable cross-origin policies

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter Category=Integration

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Development Status

| Component | Status | Progress |
|-----------|--------|----------|
| Domain Model | ✅ Complete | 100% |
| API Layer | 🔄 In Progress | 60% |
| Authentication | 🔄 In Progress | 40% |
| Database Layer | ⏳ Planned | 0% |
| Integration Tests | ⏳ Planned | 0% |
| Documentation | 🔄 In Progress | 80% |

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Follow Clean Architecture principles
4. Add comprehensive tests
5. Update documentation
6. Submit a pull request

## 📚 Documentation

- [API Documentation](./Exercise.API/README.md)
- [Domain Documentation](./Exercise.Domain/README.md)
- [Technical Specification](./docs/Technical-Specification.md)

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Related Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [RapidAPI Exercise Database](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb)