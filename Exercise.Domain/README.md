# Exercise.Domain - Domain Layer

The domain layer is the heart of the Exercise Microservice, containing the core business logic, entities, and rules. Built with Domain-Driven Design (DDD) principles and SOLID design patterns.

## ??? Architecture Philosophy

The domain layer follows these key principles:
- **Domain-Driven Design** - Rich domain models with behavior
- **Clean Architecture** - Independent of external concerns
- **SOLID Principles** - Maintainable and extensible design
- **Encapsulation** - Protected invariants and controlled state changes
- **Guard Clauses** - Defensive programming with validation

## ?? Project Structure

```
Exercise.Domain/
??? Entities/             # Core domain entities with business logic
?   ??? User.cs          # User aggregates and behavior
?   ??? Exercise.cs      # Exercise definitions and properties
?   ??? Workout.cs       # Workout sessions and management
?   ??? WorkoutPlan.cs   # Structured workout programs
?   ??? ExerciseLog.cs   # Historical workout records
?   ??? Analytics.cs     # Progress tracking and metrics
??? ValueObjects/        # Immutable value objects
?   ??? Height.cs        # Height measurements with conversions
?   ??? Weight.cs        # Weight measurements with conversions
??? Common/              # Shared domain utilities
?   ??? Guard.cs         # Validation guard clauses
??? README.md           # This file
```

## ??? Domain Entities

### User Entity
Represents a system user with authentication and profile management.

```csharp
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Name { get; private set; }
    public Height? Height { get; private set; }
    public Weight? Weight { get; private set; }
    
    // Business Methods
    public void SetPassword(string password)
    public void UpdateProfile(string? userName, Height? height, Weight? weight)
    public bool IsSocialLogin()
}
```

**Key Features:**
- Secure password handling with hashing
- Social login support (Google, Facebook)
- Profile management with validation
- Email validation with regex patterns

### Exercise Entity
Represents individual exercises with their properties and metadata.

```csharp
public class Exercise
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string BodyPart { get; private set; }
    public string TargetMuscle { get; private set; }
    public string? Equipment { get; private set; }
    
    // Business Methods
    public void UpdateDescription(string? description)
    public void UpdateGifUrl(string? gifUrl)
    public bool RequiresEquipment()
}
```

**Key Features:**
- Immutable once created (except for description/GIF updates)
- Equipment requirement detection
- Body part and muscle targeting
- Rich metadata support

### Workout Entity
Manages individual workout sessions with exercise collections and state.

```csharp
public class Workout
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyList<Exercise> Exercises { get; }
    public bool IsCompleted { get; private set; }
    
    // Business Methods
    public void AddExercise(Exercise exercise)
    public void RemoveExercise(Guid exerciseId)
    public void CompleteWorkout(TimeSpan duration)
    public void UpdateNotes(string? notes)
}
```

**Key Features:**
- Encapsulated exercise collection with controlled access
- State management (active vs completed)
- Duplicate exercise prevention
- Completion tracking with duration

### WorkoutPlan Entity
Orchestrates multiple workouts into structured programs.

```csharp
public class WorkoutPlan
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyList<Workout> Workouts { get; }
    public bool IsActive { get; private set; }
    
    // Business Methods
    public void AddWorkout(Workout workout)
    public void RemoveWorkout(Guid workoutId)
    public void Activate()
    public void Deactivate()
}
```

**Key Features:**
- Multi-workout program management
- Date range validation
- User ownership enforcement
- Activation/deactivation lifecycle

### ExerciseLog Entity
Records completed workouts with detailed performance metrics.

```csharp
public class ExerciseLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public IReadOnlyList<ExerciseLogEntry> ExercisesCompleted { get; }
    public bool IsCompleted { get; private set; }
    
    // Business Methods
    public void AddEntry(Guid exerciseId, int sets, int reps, TimeSpan? duration)
    public void CompleteLog(TimeSpan? totalDuration)
    public TimeSpan CalculateTotalDuration()
}
```

**Key Features:**
- Detailed exercise performance tracking
- Sets, reps, and duration recording
- Automatic duration calculation
- Historical workout data

### Analytics Entity
Provides structured data storage for progress tracking and reporting.

```csharp
public class Analytics
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string AnalyticsType { get; private set; }
    public Dictionary<string, object> Data { get; private set; }
    
    // Business Methods
    public void AddDataPoint(string key, object value)
    public T? GetDataPoint<T>(string key)
}
```

**Key Features:**
- Flexible data structure for various metrics
- Type-safe data retrieval
- Period-based analytics
- Extensible for future metrics

## ?? Value Objects

### Height Value Object
Immutable height representation with unit conversions.

```csharp
public class Height
{
    public decimal Centimeters { get; private set; }
    
    // Factory Methods
    public static Height FromCentimeters(decimal centimeters)
    public static Height FromFeetAndInches(int feet, decimal inches)
    public static Height FromMeters(decimal meters)
    
    // Computed Properties
    public decimal TotalInches { get; }
    public decimal Meters { get; }
    public int Feet { get; }
}
```

### Weight Value Object
Immutable weight representation with unit conversions.

```csharp
public class Weight
{
    public decimal Kilograms { get; private set; }
    
    // Factory Methods
    public static Weight FromKilograms(decimal kilograms)
    public static Weight FromPounds(decimal pounds)
    public static Weight FromStonesAndPounds(int stones, decimal pounds)
    
    // Computed Properties
    public decimal Pounds { get; }
    public decimal Stones { get; }
}
```

**Value Object Benefits:**
- **Immutability** - Cannot be changed after creation
- **Equality** - Value-based equality comparison
- **Unit Safety** - Prevents unit conversion errors
- **International Support** - Multiple measurement systems

## ??? Guard Pattern

The domain uses a centralized Guard pattern for consistent validation:

```csharp
public static class Guard
{
    public static void AgainstEmptyGuid(Guid value, string paramName)
    public static void AgainstNullOrWhiteSpace(string value, string paramName)
    public static void AgainstNull<T>(T value, string paramName) where T : class
    public static void AgainstNegativeOrZero(int value, string paramName)
    public static void AgainstNegativeOrZero(decimal value, string paramName)
    public static void AgainstInvalidDateRange(DateTime start, DateTime? end, string paramName)
}
```

**Guard Benefits:**
- **Consistency** - Same validation approach across all entities
- **Maintainability** - Single place to update validation rules
- **Readability** - Clear intent with descriptive method names
- **Reusability** - Shared validation logic
- **Performance** - Static methods with minimal overhead

## ? Business Rules Enforced

### User Rules
- Email must be in valid format
- Passwords must be at least 8 characters
- Social login users cannot set passwords
- Profile updates maintain data integrity

### Workout Rules
- Cannot modify completed workouts
- No duplicate exercises in a workout
- Duration must be positive when completing
- User ownership is strictly enforced

### WorkoutPlan Rules
- End date must be after start date
- Cannot add workouts for different users
- Only one plan can be active at a time
- Date ranges must be valid

### ExerciseLog Rules
- Cannot modify completed logs
- Sets and reps must be positive
- Duration calculations are automatic
- Entry validation prevents invalid data

## ?? SOLID Principles Applied

### Single Responsibility Principle (SRP)
- Each entity manages only its own concerns
- Guard class handles only validation
- Value objects represent single concepts

### Open/Closed Principle (OCP)
- Entities are open for extension via methods
- Closed for modification with private setters
- Guard pattern allows new validation rules

### Liskov Substitution Principle (LSP)
- Value objects maintain equality contracts
- Entity interfaces can be safely substituted
- Polymorphic behavior is consistent

### Interface Segregation Principle (ISP)
- Focused interfaces for specific needs
- No forced dependencies on unused methods
- Client-specific abstractions

### Dependency Inversion Principle (DIP)
- Domain doesn't depend on infrastructure
- Abstractions define contracts
- Dependencies point inward

## ?? Domain Testing

### Unit Testing Approach
```csharp
[Test]
public void User_SetPassword_Should_ThrowException_When_UserIsSocialLogin()
{
    // Arrange
    var user = new User(Guid.NewGuid(), "John Doe", "john@example.com", "Google", "123");
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => user.SetPassword("password123"));
}
```

### Testing Categories
- **Entity Behavior** - Business method testing
- **Validation Rules** - Guard clause testing
- **State Transitions** - Lifecycle testing
- **Business Rules** - Domain logic testing

## ?? Domain Events (Future)

Planned domain events for event-driven architecture:

```csharp
// Future implementation
public class WorkoutCompletedEvent : IDomainEvent
{
    public Guid WorkoutId { get; }
    public Guid UserId { get; }
    public TimeSpan Duration { get; }
    public DateTime CompletedAt { get; }
}
```

## ?? Performance Considerations

### Memory Efficiency
- **Readonly collections** prevent unnecessary copies
- **Private setters** minimize object mutation
- **Static Guards** reduce allocation overhead

### Computation Efficiency
- **Lazy calculations** where appropriate
- **Cached regex patterns** for validation
- **Efficient LINQ operations** in collections

## ?? Future Enhancements

### Domain Services
- `WorkoutRecommendationService` - AI-powered workout suggestions
- `ProgressCalculationService` - Advanced analytics calculations
- `ExerciseCompatibilityService` - Exercise combination validation

### Advanced Features
- **Domain Events** - Event-driven architecture
- **Specification Pattern** - Complex query logic
- **Repository Patterns** - Data access abstractions

## ?? Related Resources

- [Domain-Driven Design by Eric Evans](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Guard Clause Pattern](https://deviq.com/design-patterns/guard-clause)