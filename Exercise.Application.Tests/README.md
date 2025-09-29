# Exercise.Application.Tests - Test Suite

Unit and integration tests for the Exercise Application layer, ensuring comprehensive coverage of CQRS handlers, validators, and AutoMapper profiles with advanced C# testing patterns.

## 🧪 Testing Philosophy

The test suite follows these key principles:

- **AAA Pattern** - Arrange-Act-Assert for clear test structure  
- **Dependency Injection** - Mock frameworks for isolated unit testing
- **Test-Driven Development** - Tests drive implementation design
- **Comprehensive Coverage** - Unit tests for handlers, validators, and mapping
- **Readable Assertions** - FluentAssertions for expressive test validation
- **Async Testing** - Proper async/await patterns with CancellationToken

## 📁 Project Structure

```plaintext
Exercise.Application.Tests/
├── Features/                    # Feature-based test organization
│   └── Exercises/              # Exercise-related test suites
│       ├── Queries/            # Query handler tests
│       │   ├── GetExercisesByBodyPart/
│       │   │   └── ✅ GetExercisesByBodyPartQueryHandlerTests.cs
│       │   ├── GetAllExercises/
│       │   │   └── ⏳ GetAllExercisesQueryHandlerTests.cs
│       │   └── GetById/
│       │       └── ⏳ GetByIdQueryHandlerTests.cs
│       └── Mapping/            # AutoMapper profile tests
│           └── ✅ ExerciseProfileTests.cs
├── TestHelpers/                # Shared test utilities
│   ├── ✅ MockFactory.cs      # Mock object factory
│   └── ✅ TestDataBuilder.cs  # Test data builders
├── Exercise.Application.Tests.csproj
└── README.md                   # This file

Legend: ✅ Completed | ⏳ Planned | 🔄 In Progress
```

## 🎯 Current Test Coverage

### ✅ **Completed Tests**

#### **1. AutoMapper Profile Tests**

- **ExerciseProfileTests** - Validates AutoMapper configuration and entity-to-DTO mapping

#### **2. Handler Unit Tests**  

- **GetExercisesByBodyPartQueryHandlerTests** - Mock-based testing with dependency injection patterns

### ⏳ **Planned Test Implementation**

#### **Handler Tests**

- GetAllExercisesQueryHandler tests
- GetByIdQueryHandler tests  
- Command handler tests (Create, Update, Delete)

#### **Integration Tests**

- Full CQRS pipeline testing
- In-memory database integration
- API endpoint integration tests

## 🔧 Testing Framework Stack

- **xUnit** - Primary testing framework
- **Moq** - Mock framework for dependency injection
- **FluentAssertions** - Readable assertion syntax
- **AutoMapper** - Configuration testing
- **Microsoft.Extensions.DependencyInjection** - Service container testing

## 📚 C# Testing Concepts Demonstrated

- **Generic Types**: `Mock<T>`, `Task<T>` patterns
- **Async Testing**: Proper async/await with CancellationToken
- **Dependency Injection**: Constructor injection in test classes
- **Interface Abstraction**: Repository pattern testing
- **Fluent Assertions**: Expressive test validation
- **Test Data Builders**: Maintainable test data creation

## 🏃 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "GetExercisesByBodyPartQueryHandlerTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```