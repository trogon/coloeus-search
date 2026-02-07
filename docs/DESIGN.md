# MassifCentral - Design Document

## Version Control
- **Version:** 1.1.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Implemented Dependency Injection infrastructure with Microsoft.Extensions.DependencyInjection; refactored Logger to ILogger interface; added ServiceCollectionExtensions for service registration; updated Program.cs to use DI container

---

## Architecture Overview

MassifCentral follows a layered architecture pattern designed for maintainability, testability, and scalability.

### High-Level Architecture

```
┌─────────────────────────────────────┐
│   Console Applications              │
│   (MassifCentral.Console)           │
│                                     │
│   - Entry Point                     │
│   - Program.cs                      │
└─────────────┬───────────────────────┘
              │
              │ References
              │
┌─────────────▼───────────────────────┐
│   Shared Library                    │
│   (MassifCentral.Lib)               │
│                                     │
│   - Domain Models                   │
│   - Utilities                       │
│   - Constants                       │
└─────────────────────────────────────┘
              ▲
              │
              │ Tested By
              │
┌─────────────┴───────────────────────┐
│   Unit Tests                        │
│   (MassifCentral.Tests)             │
│                                     │
│   - Library Tests                   │
│   - Utility Tests                   │
└─────────────────────────────────────┘
```

## Project Structure

### Directory Layout

```
MassifCentral/
├── src/
│   ├── MassifCentral.Console/
│   │   ├── Program.cs                    (Entry point)
│   │   └── MassifCentral.Console.csproj
│   │
│   └── MassifCentral.Lib/
│       ├── Constants.cs                  (Global constants)
│       ├── Models/
│       │   └── BaseEntity.cs             (Base domain entity class)
│       ├── Utilities/
│       │   └── Logger.cs                 (Logging utility)
│       └── MassifCentral.Lib.csproj
│
├── tests/
│   └── MassifCentral.Tests/
│       ├── UnitTest1.cs                  (Library unit tests)
│       ├── xunit.runner.json
│       └── MassifCentral.Tests.csproj
│
├── docs/
│   ├── REQUIREMENTS.md                   (Project requirements)
│   └── DESIGN.md                         (This document)
│
├── MassifCentral.sln                     (Solution file)
└── README.md                             (Project overview)
```

## Component Design

### 1. Constants (MassifCentral.Lib.Constants)

**Purpose:** Centralize application-wide constants for consistency and easy maintenance.

**Design:**
- Static class with constant properties
- Immutable values initialized at compile time
- No instance creation required

**Current Constants:**
- `ApplicationName` - Identifies the application ("MassifCentral")
- `Version` - Current application version ("1.0.0")

**Extension Points:**
- Add feature flags for feature toggle functionality
- Add environment-specific configurations
- Add API endpoint URLs

### 2. Base Entity (MassifCentral.Lib.Models.BaseEntity)

**Purpose:** Provide a consistent base for all domain entities with tracking and identification.

**Design:**
- Abstract class that cannot be instantiated directly
- Inheritance-based extension for specific domain models
- Automatic Guid generation for unique identification
- UTC timestamps for cross-timezone consistency
- Active status flag for soft delete patterns

**Properties:**
- `Id` (Guid) - Unique identifier
- `CreatedAt` (DateTime) - Entity creation timestamp
- `ModifiedAt` (DateTime) - Entity modification timestamp
- `IsActive` (bool) - Active status flag

**Design Patterns:**
- Template Method Pattern - Subclasses define specific behavior
- Audit Trail Pattern - Built-in timestamp tracking

**Extension Points:**
- Add CreatedBy/ModifiedBy properties for user tracking
- Add version property for optimistic concurrency
- Add soft delete implementation in repositories

### 3. Logger Utility (MassifCentral.Lib.Utilities.Logger)

**Purpose:** Provide simple, consistent logging across the application.

**Design:**
- Static class providing static methods
- No configuration required for basic usage
- UTC timestamps for log consistency
- Console output for local execution

**Methods:**
- `LogInfo(string message)` - Informational messages
- `LogWarning(string message)` - Warning conditions
- `LogError(string message)` - Error messages
- `LogError(string message, Exception ex)` - Errors with exceptions

**Current Implementation:**
- Console.WriteLine output to STDOUT
- Timestamp format: `yyyy-MM-dd HH:mm:ss`
- Log level prefixes: `[INFO]`, `[WARNING]`, `[ERROR]`

**Future Enhancements:**
- Abstract `ILogger` interface for dependency injection
- Multiple output targets (file, syslog, cloud)
- Configurable log levels
- Structured logging support
- Log filtering and sampling

### 4. Console Application (MassifCentral.Console)

**Purpose:** Entry point and orchestration of application startup and shutdown.

**Design:**
- Simple entry point in Program.cs
- Demonstrates library component usage
- Structured exception handling
- Graceful error recovery

**Responsibilities:**
- Initialize application with logging
- Execute business logic (placeholder)
- Handle exceptions at application level
- Log shutdown status

**Extension Strategy:**
- Add command-line argument parsing
- Add configuration file loading
- Add dependency injection container setup
- Add service registration

## SOLID Principles Application

### Single Responsibility Principle (SRP)
- **Constants** class - Only manages global constants
- **Logger** class - Only handles logging concerns
- **BaseEntity** class - Only provides base entity functionality
- **Program.cs** - Only orchestrates application startup/shutdown

### Open/Closed Principle (OCP)
- **BaseEntity** - Open for extension (inheritance), closed for modification
- **Logger** - Can be extended to ILogger interface for implementation variation
- **Models namespace** - New entities can be added without modifying existing code

### Liskov Substitution Principle (LSP)
- **BaseEntity subclasses** - Must maintain the contract of a valid entity
- Can be used anywhere BaseEntity is expected without breaking behavior

### Interface Segregation Principle (ISP)
- **Future Logger interface** will be minimum interface required
- Clients only depend on methods they use

### Dependency Inversion Principle (DIP)
- **Console application** depends on Logger static API (future: ILogger abstraction)
- Low-level modules (Logger) don't depend on high-level modules directly

## Testing Strategy

### Unit Test Scope
- Individual class functionality
- Library constants verification
- BaseEntity initialization and properties
- Logger output format validation

### Test Framework
- **Framework:** xUnit v3
- **Configuration:** Configured in xunit.runner.json
- **Namespace:** MassifCentral.Tests

### Test Structure
- **Test Classes:** One per component (LibraryTests)
- **Test Methods:** Named with Arrange-Act-Assert pattern
- **Coverage Goal:** >= 80% for all public APIs

## Data Flow

### Application Startup Flow

1. **Program.cs Main() executes**
2. **Logger.LogInfo()** - Log startup message with Constants.ApplicationName
3. **Try block** - Execute application logic
4. **Logger.LogInfo()** - Log initialization success
5. **Console.WriteLine()** - Display welcome message
6. **Catch block** - Handle exceptions
7. **Logger.LogError()** - Log error details
8. **Finally block** - Log completion
9. **Environment.Exit()** - Exit with appropriate code

### Entity Creation Flow

1. **Derived class instantiation** (e.g., `new UserEntity()`)
2. **BaseEntity constructor runs** (implicit)
3. **Id property initializes** with `Guid.NewGuid()`
4. **Timestamps initialize** with `DateTime.UtcNow`
5. **IsActive initializes** to `true`
6. **Entity ready for use**

## Error Handling

### Application-Level Errors
- Caught at Main() level
- Logged with full exception details
- Application exits with code 1

### Future Error Handling Enhancement
- Specific exception types for different failures
- Retry logic for transient failures
- Error recovery strategies
- Error notification system

## Security Considerations

### Current Implementation
- No sensitive data exposure in logs
- UTC timestamps prevent timezone confusion
- No authentication/authorization (not applicable at this stage)

### Future Security Measures
- Input validation framework
- Secure configuration management
- Data encryption for sensitive fields
- Audit logging for security events
- Access control for sensitive operations

## Performance Considerations

### Optimization Areas
- Static Logger class avoids object creation overhead
- Guid.NewGuid() is efficient for unique identifiers
- DateTime.UtcNow provides minimal overhead
- Console output is appropriate for current scale

### Future Performance Enhancements
- Asynchronous logging for high-throughput scenarios
- Log aggregation and batching
- Structured logging for efficient parsing
- Caching of frequently accessed data

## Deployment Considerations

### Build Output
- Console application produces executable
- Library produces DLL for reuse
- Single-file executable publishing supported

### Runtime Requirements
- .NET 10 Runtime
- No external dependencies (core functionality)
- Cross-platform compatible (Windows, Linux, macOS)

## Dependency Injection Strategy

### Motivation for Dependency Injection

Dependency Injection enables:
- **Loose Coupling:** Components depend on abstractions, not concrete implementations
- **Testability:** Easy to inject mock implementations for unit testing
- **Flexibility:** Swap implementations without changing dependent code
- **Maintainability:** Centralized object lifecycle management
- **Scalability:** Scales well as codebase grows with multiple implementations

### Recommended Library: Microsoft.Extensions.DependencyInjection

**Selection Rationale:**
- Official Microsoft library - built into ASP.NET Core ecosystem
- Lightweight and performant - minimal overhead
- Zero external dependencies - only depends on core .NET
- Excellent console app support - works seamlessly with Microsoft.Extensions.Hosting
- Familiar patterns - aligns with industry standards and ASP.NET Core conventions
- Built-in lifetime management - Transient, Scoped, Singleton patterns
- Great documentation and community support

**Alternatives Considered:**

| Library | Pros | Cons | Recommendation |
|---------|------|------|-----------------|
| Microsoft.Extensions.DependencyInjection | Official, lightweight, no dependencies | Limited advanced features | ✅ **Recommended** |
| Autofac | Powerful, module system, diagnostics | Heavier, more complex | Consider if advanced features needed later |
| SimpleInjector | Clean API, diagnostic abilities | Smaller community | Good lightweight alternative |
| Ninject | Popular, flexible | Older, slower | Not recommended for new projects |
| Castle Windsor | Enterprise-grade, mature | Complex setup, heavier | For large enterprise systems |

### Library Acquisition

Add the following NuGet packages to MassifCentral.Console and MassifCentral.Lib projects:

**Command Line:**
```bash
dotnet add src/MassifCentral.Console package Microsoft.Extensions.DependencyInjection
dotnet add src/MassifCentral.Console package Microsoft.Extensions.Hosting
dotnet add src/MassifCentral.Lib package Microsoft.Extensions.DependencyInjection.Abstractions
```

**Or add to .csproj:**
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="10.0.0" />
  <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.0.0" />
</ItemGroup>
```

### Setup Architecture

```
┌─────────────────────────────────────┐
│   Console Application               │
│   (MassifCentral.Console)           │
│                                     │
│   ┌──────────────────────────────┐  │
│   │ ServiceCollection Setup      │  │
│   │ - Register services          │  │
│   │ - Register logging           │  │
│   │ - Create provider            │  │
│   └────────┬─────────────────────┘  │
│            │                        │
│            ▼                        │
│   ┌──────────────────────────────┐  │
│   │ IServiceProvider             │  │
│   │ - Resolve dependencies       │  │
│   │ - Manage lifetimes           │  │
│   │ - Create instances           │  │
│   └────────┬─────────────────────┘  │
└────────────┼──────────────────────────┘
             │
             ▼
         Dependent Services
         (Logger, Repositories, etc.)
```

### Implementation Pattern

**1. Create Service Registration Extension:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Register abstracted Logger
        services.AddSingleton<ILogger, Logger>();
        
        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        
        // Register use cases
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        
        return services;
    }
}
```

**2. Configure in Program.cs:**
```csharp
var services = new ServiceCollection();
services.AddApplicationServices();
var serviceProvider = services.BuildServiceProvider();

// Resolve and use services
var logger = serviceProvider.GetRequiredService<ILogger>();
var useCase = serviceProvider.GetRequiredService<ICreateUserUseCase>();
```

**3. With Microsoft.Extensions.Hosting (Recommended):**
```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationServices();
        services.AddScoped<Application>();
    })
    .Build();

var app = host.Services.GetRequiredService<Application>();
await app.RunAsync();
```

### Lifetime Management

**Transient (Stateless Services)**
- New instance created each time
- Use for: Stateless utilities, command handlers
- Example: `LoggerFormatter`

**Scoped (Request-level)**
- New instance per scope/request
- Use for: Database contexts, repositories, use cases
- Example: `UserRepository`

**Singleton (Application-level)**
- Single instance for application lifetime
- Use for: Configuration, logging, caches
- Example: `ApplicationConfiguration`, `ILogger`

### Refactoring Logger for DI

**Current Static Approach:**
```csharp
Logger.LogInfo("message");  // Static, tightly coupled
```

**DI-Ready Interface:**
```csharp
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogError(string message, Exception ex);
}

public class Logger : ILogger
{
    // Implementation moves here
}
```

**Usage with DI:**
```csharp
public class MyService
{
    private readonly ILogger _logger;
    
    public MyService(ILogger logger)
    {
        _logger = logger;  // Injected, loosely coupled
    }
    
    public void DoWork()
    {
        _logger.LogInfo("Starting work");
    }
}
```

### Testing with DI

**Mock Logger for Tests:**
```csharp
public class MockLogger : ILogger
{
    public List<string> LoggedMessages { get; } = new();
    
    public void LogInfo(string message) => LoggedMessages.Add(message);
    public void LogWarning(string message) => LoggedMessages.Add(message);
    public void LogError(string message) => LoggedMessages.Add(message);
    public void LogError(string message, Exception ex) => LoggedMessages.Add(message);
}

// In unit test
[Fact]
public void ServiceLogsInfoMessage()
{
    var mockLogger = new MockLogger();
    var service = new MyService(mockLogger);
    
    service.DoWork();
    
    Assert.Contains("Starting work", mockLogger.LoggedMessages);
}
```

### Implementation Roadmap

**Phase 1: Foundation (Current)**
- Add DI packages to projects
- Create `ILogger` interface
- Create `ServiceCollectionExtensions`
- Update `Logger` class to implement `ILogger`

**Phase 2: Refactor Console App**
- Update `Program.cs` to use DI container
- Inject `ILogger` instead of using static methods
- Test with mock logger implementation

**Phase 3: Expand Service Registration**
- Create repository interfaces in Domain layer
- Register repositories in DI container
- Implement abstraction-based service layers

**Phase 4: Advanced Patterns**
- Factory patterns for complex object creation
- Decorator patterns for cross-cutting concerns
- Service locator patterns where appropriate

### Best Practices

1. **Depend on Abstractions:** Always inject interfaces, not concrete classes
2. **Constructor Injection:** Prefer constructor injection for clarity and testability
3. **Service Registration:** Register services at application startup, not runtime
4. **Lifetime Correctness:** Choose appropriate lifetimes to avoid memory leaks
5. **Avoid Service Locator:** Use constructor injection instead of requesting from container
6. **Group Registration:** Use extension methods to organize service registration

## Future Architecture Evolution

1. **Phase 2:** Dependency injection and interface abstractions
2. **Phase 3:** Data access layer and repositories
3. **Phase 4:** Business logic service layer
4. **Phase 5:** API/Web service layer
5. **Phase 6:** Event-driven architecture with messaging

## Related Documents

- [Requirements Document](./REQUIREMENTS.md) - Detailed functional and non-functional requirements
- [README.md](../README.md) - Project overview and quick start guide
