# Exercise.Infrastructure

Infrastructure layer for the Exercise Microservice, implementing data access and external service integrations.

## 📋 Overview

This layer contains all infrastructure concerns including:
- **Database Access** - Entity Framework Core with SQL Server
- **Repository Implementations** - Concrete implementations of domain repository interfaces
- **External Services** - Third-party API integrations
- **Persistence Configuration** - Entity mappings and database context

## 🏗️ Architecture

Following Clean Architecture principles, this layer depends on:
- ✅ `Exercise.Domain` - Core domain entities and interfaces
- ❌ No dependencies on Application or API layers

## 📁 Project Structure

```
Exercise.Infrastructure/
├── Data/
│   ├── ExerciseDbContext.cs              # EF Core DbContext
│   ├── DependencyInjection.cs            # Service registration
│   └── Configurations/
│       └── ExerciseConfiguration.cs      # Fluent API entity mappings
├── Repositories/
│   └── ExerciseRepository.cs             # (Coming in Issue #11)
└── Exercise.Infrastructure.csproj
```

## 🔧 Technologies

| Component | Technology | Purpose |
|-----------|------------|---------|
| ORM | Entity Framework Core 9.0 | Object-relational mapping |
| Database | SQL Server | Primary data store |
| Configuration | Fluent API | Entity mappings |
| DI | Microsoft.Extensions.DependencyInjection | Service registration |

## 📦 NuGet Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.10" />
```

## 🗄️ Database Context

### ExerciseDbContext

The main database context managing entity mappings and database operations.

```csharp
public class ExerciseDbContext : DbContext
{
    public DbSet<Exercise> Exercises { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applies all IEntityTypeConfiguration implementations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExerciseDbContext).Assembly);
    }
}
```

**Key Features:**
- Constructor injection of `DbContextOptions`
- `DbSet<Exercise>` for Exercises table access
- Automatic configuration discovery via assembly scanning
- Using alias pattern to avoid namespace collisions

## 🎨 Entity Configuration

### ExerciseConfiguration

Implements `IEntityTypeConfiguration<Exercise>` using Fluent API.

```csharp
public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.BodyPart).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TargetMuscle).IsRequired().HasMaxLength(100);
        
        builder.HasIndex(e => e.BodyPart); // Performance optimization
    }
}
```

**Configuration Details:**

| Property | Required | Max Length | Notes |
|----------|----------|------------|-------|
| Id | ✅ | - | Primary key (Guid) |
| Name | ✅ | 200 | Exercise name |
| BodyPart | ✅ | 100 | Indexed for queries |
| TargetMuscle | ✅ | 100 | Muscle group |
| Equipment | ❌ | 100 | Optional equipment |
| GifUrl | ❌ | 500 | Animation URL |
| Description | ❌ | 1000 | Exercise details |
| Difficulty | ❌ | 50 | Difficulty level |

**Why Fluent API?**
- ✅ Keeps domain entities clean (no attributes)
- ✅ Follows Clean Architecture (domain has zero infrastructure dependencies)
- ✅ More flexible than data annotations
- ✅ Centralized configuration in Infrastructure layer

## 🔌 Dependency Injection

### Service Registration

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddDbContext<ExerciseDbContext>(options =>
        options.UseSqlServer(
            configuration.GetConnectionString("DefaultConnection")));

    return services;
}
```

**Usage in `Program.cs`:**
```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

## 🔧 Configuration

### Connection String

Located in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-50KN1FJ;Database=ExerciseDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Connection String Components:**
- `Server` - SQL Server instance name
- `Database` - Database name
- `Trusted_Connection=true` - Windows authentication
- `MultipleActiveResultSets=true` - EF Core optimization

## 🗃️ Repositories

### ExerciseRepository (Coming in Issue #11)

Will implement `IExerciseRepository` using `ExerciseDbContext`:

```csharp
public class ExerciseRepository : IExerciseRepository
{
    private readonly ExerciseDbContext _context;
    
    public async Task<IReadOnlyList<Exercise>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Exercises
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
    
    // Additional methods...
}
```

## 🚀 Migrations

### Creating Migrations

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --project Exercise.Infrastructure --startup-project Exercise.API

# Update database
dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API

# Remove last migration
dotnet ef migrations remove --project Exercise.Infrastructure --startup-project Exercise.API

# Generate SQL script
dotnet ef migrations script --project Exercise.Infrastructure --startup-project Exercise.API
```

## 🧩 Design Patterns

### Repository Pattern
- Abstracts data access logic
- Defined in Domain, implemented in Infrastructure
- Enables easier testing with mocks

### Unit of Work
- `DbContext` acts as Unit of Work
- Tracks changes across multiple operations
- Single `SaveChanges()` commits all changes

### Dependency Inversion
- Infrastructure depends on Domain abstractions
- Domain defines `IExerciseRepository` interface
- Infrastructure provides concrete implementation

## 🎯 Current Status

| Feature | Status | Notes |
|---------|--------|-------|
| DbContext Setup | ✅ Complete | Issue #10 |
| Entity Configuration | ✅ Complete | Fluent API mappings |
| DI Registration | ✅ Complete | Service collection extensions |
| Connection String | ✅ Complete | appsettings.json |
| Repository Implementation | ⏳ Planned | Issue #11 |
| Database Migrations | ⏳ Planned | Issue #12 |
| Seeding Data | ⏳ Planned | Future sprint |

## 🔗 Related Documentation

- [Domain Layer](../Exercise.Domain/README.md)
- [Application Layer](../Exercise.Application/README.md)
- [API Layer](../Exercise.API/README.md)
- [Main README](../README.md)

## 📚 Learning Resources

- [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Fluent API Configuration](https://learn.microsoft.com/en-us/ef/core/modeling/)
- [DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Contributing

When adding infrastructure code:
1. Follow the repository pattern
2. Use Fluent API for entity configuration
3. Keep domain entities clean (no EF attributes)
4. Register services in `DependencyInjection.cs`
5. Add comprehensive tests for repositories
