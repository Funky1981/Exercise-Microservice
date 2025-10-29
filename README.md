# Exercise Microservice

[![.NET Version](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF_Core-9.0-512BD4)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

A production-ready .NET 9 microservice for fitness tracking and exercise management, built with **Clean Architecture** principles and **Domain-Driven Design**. This project demonstrates modern software engineering practices including CQRS, repository pattern, dependency injection, and comprehensive unit testing.

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Development Status](#-development-status)
- [Documentation](#-documentation)
- [Contributing](#-contributing)

---

## ✨ Features

### Core Capabilities

- **Exercise Management** - Browse, search, and manage exercise database
- **Workout Planning** - Create and manage personalized workout plans
- **Progress Tracking** - Log workouts and track fitness progress over time
- **User Management** - User profiles with secure authentication
- **Analytics Dashboard** - Comprehensive workout statistics and progress reports

### Technical Highlights

- ✅ **Clean Architecture** - Maintainable, testable, and scalable design
- ✅ **CQRS Pattern** - Command Query Responsibility Segregation with MediatR
- ✅ **Domain-Driven Design** - Rich domain models with encapsulated business logic
- ✅ **Entity Framework Core** - Code-first database approach with SQL Server
- ✅ **Minimal API** - Lightweight, high-performance ASP.NET Core endpoints
- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Comprehensive Testing** - Unit tests with xUnit, Moq, and FluentAssertions
- ✅ **Docker Support** - Containerized deployment ready

---

## 🏗️ Architecture

This microservice follows **Clean Architecture** (Onion Architecture) with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                      Exercise.API                        │
│            (Controllers, Minimal APIs, DI)               │
│                 ↓ depends on ↓                          │
├─────────────────────────────────────────────────────────┤
│              Exercise.Infrastructure                     │
│        (EF Core, Repositories, External APIs)            │
│                 ↓ depends on ↓                          │
├─────────────────────────────────────────────────────────┤
│               Exercise.Application                       │
│         (CQRS Handlers, DTOs, Interfaces)                │
│                 ↓ depends on ↓                          │
├─────────────────────────────────────────────────────────┤
│                 Exercise.Domain                          │
│         (Entities, Value Objects, Guards)                │
│              (NO DEPENDENCIES)                           │
└─────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **Domain** | Core business entities and logic | None |
| **Application** | Use cases, CQRS handlers, business workflows | Domain |
| **Infrastructure** | Data access, external services, persistence | Domain, Application |
| **API** | HTTP endpoints, authentication, DI configuration | All layers |

---

## �️ Technology Stack

### Backend

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Framework** | .NET 9.0 | Modern, high-performance runtime |
| **API** | ASP.NET Core Minimal API | Lightweight RESTful endpoints |
| **ORM** | Entity Framework Core 9.0 | Database access and migrations |
| **Database** | SQL Server | Relational data storage |
| **CQRS** | MediatR | Command/Query separation |
| **Mapping** | AutoMapper | Object-to-object mapping |
| **Authentication** | JWT Bearer | Secure token-based auth |
| **Validation** | FluentValidation | Domain validation rules |

### Testing

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Test Framework** | xUnit 2.9.2 | Unit test runner |
| **Mocking** | Moq 4.20.72 | Dependency mocking |
| **Assertions** | FluentAssertions 8.6.0 | Readable test assertions |

### External Services

- **RapidAPI Exercise Database** - Comprehensive exercise data API

---

## 🚀 Getting Started

### Prerequisites

Before running the application, ensure you have:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or LocalDB)
- [Git](https://git-scm.com/) for version control
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop) for containerized deployment
- (Optional) [RapidAPI Key](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb) for exercise database integration

### Installation

1. **Clone the repository**

   ```bash
   git clone https://github.com/Funky1981/Exercise-Microservice.git
   cd Exercise-Microservice
   ```

2. **Configure database connection**

   Update `Exercise.API/appsettings.json` with your SQL Server instance:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=ExerciseDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Run database migrations** (Coming in Issue #12)

   ```bash
   dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API
   ```

4. **Configure API keys** (Optional - for RapidAPI integration)

   ```bash
   dotnet user-secrets set "RapidApi:Key" "your-rapidapi-key" --project Exercise.API
   ```

5. **Build and run**

   ```bash
   dotnet build
   dotnet run --project Exercise.API
   ```

6. **Access the application**

   - API: `https://localhost:7041`
   - Swagger UI: `https://localhost:7041/swagger` (when implemented)

### Running with Docker

```bash
# Build the Docker image
docker build -t exercise-microservice .

# Run the container
docker run -p 8080:80 -p 8081:443 exercise-microservice
```

### Running Tests

```bash
# Run all unit tests
dotnet test

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test Exercise.Application.Tests/Exercise.Application.Tests.csproj
```

---

## � Project Structure

```
Exercise-Microservice/
│
├── Exercise.API/                           # 🌐 API Layer
│   ├── Program.cs                          # Application entry point & DI configuration
│   ├── MapEndpoints.cs                     # Minimal API endpoint mappings
│   ├── appsettings.json                    # Configuration & connection strings
│   └── Properties/launchSettings.json      # Development settings
│
├── Exercise.Application/                   # 💼 Application Layer
│   ├── DependencyInjection.cs              # Application service registration
│   ├── Features/                           # Feature-based organization (Vertical Slices)
│   │   └── Exercises/
│   │       ├── Queries/                    # CQRS Queries
│   │       │   ├── GetAllExercises/
│   │       │   ├── GetExercisesById/
│   │       │   └── GetExercisesByBodyPart/
│   │       ├── Commands/                   # CQRS Commands (planned)
│   │       ├── Dtos/                       # Data Transfer Objects
│   │       └── Mapping/                    # AutoMapper profiles
│   ├── Abstractions/                       # Interfaces
│   │   └── Repositories/                   # Repository contracts
│   └── Common/                             # Shared application logic
│       └── Behaviors/                      # MediatR pipeline behaviors
│
├── Exercise.Application.Tests/             # 🧪 Unit Tests
│   ├── Features/                           # Test organization mirrors application
│   │   └── Exercises/
│   │       ├── Queries/                    # Query handler tests
│   │       └── Mapping/                    # AutoMapper configuration tests
│   └── TestHelpers/                        # Test utilities
│       ├── MockFactory.cs                  # Mock object factory
│       └── TestDataBuilder.cs              # Test data builders
│
├── Exercise.Domain/                        # 🏛️ Domain Layer
│   ├── Entities/                           # Domain entities
│   │   ├── Exercise.cs                     # Core exercise entity
│   │   ├── User.cs                         # User aggregate root
│   │   ├── Workout.cs                      # Workout aggregate
│   │   ├── WorkoutPlan.cs                  # Workout plan entity
│   │   ├── ExerciseLog.cs                  # Exercise log entity
│   │   └── Analytics.cs                    # Analytics entity
│   └── Common/                             # Shared domain logic
│       └── Guard.cs                        # Domain validation guards
│
├── Exercise.Infrastructure/                # 🔧 Infrastructure Layer
│   ├── Data/                               # Data access
│   │   ├── ExerciseDbContext.cs            # EF Core DbContext
│   │   ├── DependencyInjection.cs          # Infrastructure service registration
│   │   └── Configurations/                 # Fluent API entity configurations
│   │       └── ExerciseConfiguration.cs
│   └── Repositories/                       # Repository implementations (planned)
│       └── ExerciseRepository.cs
│
└── Exercise-Microservice.sln               # Solution file
```

---

## 📊 Development Status

### Implementation Progress

| Layer | Status | Progress | Description |
|-------|--------|----------|-------------|
| **Domain** | ✅ Complete | 100% | Entities, value objects, domain logic |
| **Application** | ✅ Complete | 100% | CQRS handlers, DTOs, AutoMapper |
| **Unit Tests** | ✅ Complete | 95% | xUnit, Moq, FluentAssertions |
| **Infrastructure** | 🔄 In Progress | 50% | DbContext, configurations |
| **Database** | 🔄 In Progress | 35% | Migrations pending |
| **API** | 🔄 In Progress | 60% | Endpoints partially implemented |
| **Authentication** | 🔄 In Progress | 40% | JWT configuration in progress |
| **Integration Tests** | ⏳ Planned | 0% | Coming in Sprint 7 |
| **Documentation** | ✅ Complete | 95% | Comprehensive READMEs |

### Sprint Progress

#### ✅ Completed Sprints

- **Sprint 1** - Domain & Application Layer Foundation
  - Domain entities with encapsulation
  - CQRS query handlers with MediatR
  - AutoMapper configuration
  - Repository interfaces

- **Sprint 2** - Unit Test Infrastructure
  - xUnit test framework setup
  - Moq for mocking dependencies
  - FluentAssertions for readable tests
  - Query handler test coverage

- **Sprint 3** - Infrastructure Layer (Partial)
  - ✅ Issue #10: DbContext and EF Core configuration
  - 🔄 Issue #11: Repository implementation (in progress)
  - ⏳ Issue #12: Database migrations (pending)

#### 🔄 Current Sprint

**Sprint 3** - Infrastructure Layer (Continuing)
- Implementing concrete `ExerciseRepository`
- Creating initial database migrations
- Testing database connectivity

#### ⏳ Upcoming Sprints

- **Sprint 4** - API Layer endpoints
- **Sprint 5** - Authentication & Authorization
- **Sprint 6** - External API integration
- **Sprint 7** - Integration testing
- **Sprint 8** - CQRS command handlers
- **Sprint 9** - Performance & observability
- **Sprint 10** - DevOps & deployment

---

## 🏗️ How It Was Built

### Design Principles

This project was built following industry best practices and modern software engineering principles:

#### 1. **Clean Architecture**

The codebase is organized into concentric layers with strict dependency rules:

- **Domain Layer** (Core) - Contains business entities and rules with zero external dependencies
- **Application Layer** - Orchestrates business workflows using CQRS pattern
- **Infrastructure Layer** - Implements data access and external service integrations
- **API Layer** - Exposes HTTP endpoints and handles cross-cutting concerns

**Key Benefit**: High testability, maintainability, and independence from frameworks and databases.

#### 2. **Domain-Driven Design (DDD)**

- **Rich Domain Models** - Entities encapsulate business logic with private setters
- **Value Objects** - Immutable objects like `Height` and `Weight` with built-in validation
- **Aggregate Roots** - Entities like `User` and `Workout` maintain consistency boundaries
- **Domain Guards** - Centralized validation using the Guard pattern

#### 3. **CQRS (Command Query Responsibility Segregation)**

Separates read and write operations for better scalability and clarity:

```csharp
// Query Example (Read)
public class GetExercisesByBodyPartQuery : IRequest<IReadOnlyList<ExerciseDto>>
{
    public string BodyPart { get; set; }
}

// Handler processes the query
public class GetExercisesByBodyPartQueryHandler 
    : IRequestHandler<GetExercisesByBodyPartQuery, IReadOnlyList<ExerciseDto>>
{
    // Uses repository to fetch data
    // Uses AutoMapper to map to DTOs
}
```

#### 4. **Repository Pattern**

Abstracts data access behind interfaces defined in the Domain layer:

```csharp
// Interface in Domain
public interface IExerciseRepository
{
    Task<IReadOnlyList<Exercise>> GetAllAsync(CancellationToken cancellationToken);
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

// Implementation in Infrastructure
public class ExerciseRepository : IExerciseRepository
{
    private readonly ExerciseDbContext _context;
    // EF Core implementation
}
```

#### 5. **Dependency Injection**

All dependencies are constructor-injected and registered in a centralized configuration:

- MediatR for CQRS handlers
- AutoMapper for object mapping
- DbContext for data access
- Repositories for domain abstractions

#### 6. **Test-Driven Development**

Comprehensive unit test coverage using the AAA (Arrange-Act-Assert) pattern:

```csharp
[Fact]
public async Task Handle_ShouldReturnExercises_WhenBodyPartExists()
{
    // Arrange - Set up test data and mocks
    var mockRepository = new Mock<IExerciseRepository>();
    mockRepository.Setup(r => r.GetByBodyPartAsync("chest", default))
                  .ReturnsAsync(TestExercises);

    // Act - Execute the handler
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert - Verify the outcome
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
}
```

### Technology Decisions

#### Why Entity Framework Core?

- **Code-First Approach** - Database schema generated from domain entities
- **LINQ Support** - Type-safe queries with strong IntelliSense
- **Migration System** - Version-controlled database schema changes
- **Performance** - Excellent performance with async operations

#### Why MediatR?

- **Decoupling** - Controllers don't directly depend on handlers
- **Cross-Cutting Concerns** - Pipeline behaviors for logging, validation, etc.
- **Testability** - Easy to test handlers in isolation

#### Why AutoMapper?

- **Separation** - DTOs separate from domain entities
- **Convention-Based** - Reduces boilerplate mapping code
- **Type Safety** - Compile-time validation of mappings

---

## 📚 Documentation

Comprehensive documentation is available for each layer:

- **[API Layer](./Exercise.API/README.md)** - HTTP endpoints, middleware, configuration
- **[Domain Layer](./Exercise.Domain/README.md)** - Entities, value objects, business rules
- **[Application Layer](./Exercise.Application/README.md)** - CQRS handlers, DTOs, interfaces
- **[Infrastructure Layer](./Exercise.Infrastructure/README.md)** - EF Core, repositories, data access
- **[Testing Guide](./Exercise.Application.Tests/README.md)** - Unit testing patterns and practices

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Follow** Clean Architecture principles and existing patterns
4. **Add** comprehensive unit tests for new features
5. **Update** documentation as needed
6. **Commit** changes with clear, descriptive messages
7. **Push** to your branch (`git push origin feature/amazing-feature`)
8. **Open** a Pull Request

### Code Standards

- Follow C# naming conventions
- Use private setters for entity properties
- Implement Guard validation for domain invariants
- Write unit tests with AAA pattern
- Document public APIs with XML comments

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🔗 Learn More

### Related Resources

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)

### External APIs

- [RapidAPI Exercise Database](https://rapidapi.com/justin-WFnsXH_t6/api/exercisedb)

---

## 👨‍💻 Author

**Chris Gibbons**

- GitHub: [@Funky1981](https://github.com/Funky1981)

---

## ⭐ Acknowledgments

- Clean Architecture concepts by Robert C. Martin (Uncle Bob)
- Domain-Driven Design principles by Eric Evans
- CQRS pattern implementation with MediatR
- Testing best practices from xUnit and FluentAssertions communities

---

<div align="center">

**Built with ❤️ using .NET 9**

[![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server)

</div>
 
 
