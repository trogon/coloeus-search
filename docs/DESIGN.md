# MassifCentral - Design Document

## Version Control
- **Version:** 1.2.1
- **Last Updated:** 2026-02-07
- **Change Summary:** Added packaging and distribution details for NuGet library and dotnet tool.

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
│   │   ├── Program.cs                    (Entry point with Serilog initialization)
│   │   └── MassifCentral.Console.csproj
│   │
│   └── MassifCentral.Lib/
│       ├── Constants.cs                  (Global constants)
│       ├── Models/
│       │   └── BaseEntity.cs             (Base domain entity class)
│       ├── Utilities/
│       │   └── Logger.cs                 (ILogger interface and Logger class, deprecated)
│       ├── Logging/ (NEW in v1.2.0)
│       │   ├── SerilogConfiguration.cs   (Environment-specific Serilog setup)
│       │   ├── CorrelationIdEnricher.cs  (Distributed tracing support)
│       │   └── SerilogLoggerAdapter.cs   (ILogger adapter using Serilog)
│       ├── ServiceCollectionExtensions.cs
│       └── MassifCentral.Lib.csproj
│
├── tests/
│   └── MassifCentral.Tests/
│       ├── LibraryTests.cs               (Library unit tests)
│       ├── LoggerTests.cs                (Logger interface tests)
│       ├── SerilogIntegrationTests.cs    (NEW in v1.2.0, Serilog integration)
│       ├── Mocks/
│       │   └── MockLogger.cs             (Enhanced mock logger for testing)
│       ├── xunit.runner.json
│       └── MassifCentral.Tests.csproj
│
├── docs/
│   ├── REQUIREMENTS.md                   (Project requirements)
│   ├── DESIGN.md                         (This document)
│   ├── LOGGING_LIBRARY_ANALYSIS.md       (Logging library evaluation)
│   └── SERILOG_IMPLEMENTATION_GUIDE.md   (Implementation reference)
│
├── MassifCentral.slnx                    (Solution file)
└── README.md                             (Project overview)
```

## Packaging and Distribution

MassifCentral is distributed as a NuGet library and a dotnet tool.

### Packages
- **Library:** Trogon.MassifCentral.Lib
- **Dotnet Tool:** Trogon.MassifCentral (command: tmcfind)

### Build Packages
```bash
dotnet pack MassifCentral.slnx -c Release
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

### 3. Logging Framework (Serilog) - NEW in v1.2.0

**Purpose:** Provide structured, searchable logging with environment-specific sink configurations for traceability and observability.

**Design:**
- Serilog structured logging framework implementation
- ILogger abstraction maintaining backward compatibility
- SerilogLoggerAdapter implementing ILogger interface
- Environment-specific sink strategies
- Correlation ID enrichment for distributed tracing
- JSON output format for machine readability and searching

**Configuration Modes:**

**Production Mode (Default)**
- Console Sink: Errors only
- File Sink: Rolling file capturing warnings and errors (daily rolling)
- Minimum Level: Information (debug filtered out)
- Retention: 30 days of daily log files

**Diagnostic Mode** (Set DIAGNOSTIC_MODE=true)
- Single File Sink: All log levels (Trace through Error)
- Rolling Interval: Hourly
- Retention: 6-hour window (6 files kept)
- Minimum Level: Verbose (all levels captured)
- Use Case: Troubleshooting production issues

**Development Mode** (DOTNET_ENVIRONMENT=Development)
- Console Sink: All levels with colors
- File Sink: Rolling file with all levels
- Minimum Level: Debug
- Retention: 7 days of daily files

**Components:**
- `SerilogConfiguration.cs` - Static class with environment-specific configs
- `CorrelationIdEnricher.cs` - Adds operation tracking IDs to logs
- `SerilogLoggerAdapter.cs` - Adapts Serilog to ILogger interface
- `ILogger` interface - Expanded with Debug, Trace, structured logging

**Enrichers:**
- FromLogContext - Context-based properties
- WithEnvironmentUserName - Operating system user
- WithMachineName - Machine hostname
- WithProcessId - Process ID
- WithThreadId - Thread ID
- CorrelationIdEnricher - Operation correlation ID (custom)

**Structured Logging Example:**
```csharp
logger.LogInfo("User {UserId} created order {OrderId} for {Amount:C}", 
    userId: 123, 
    orderId: "ORD-456", 
    amount: 99.99);
```
Output (JSON):
```json
{
  "@t": "2026-02-07T14:23:45Z",
  "@mt": "User {UserId} created order {OrderId} for {Amount:C}",
  "UserId": 123,
  "OrderId": "ORD-456",
  "Amount": 99.99,
  "MachineName": "WORKSTATION-01",
  "ProcessId": 1234
}
```

**Methods:**
- `LogTrace(string message)` - Detailed diagnostic tracing
- `LogDebug(string message)` - Debug-level messages
- `LogInfo(string message)` - Informational messages
- `LogWarning(string message)` - Warnings
- `LogError(string message)` - Errors
- `LogError(string message, Exception exception)` - Errors with exceptions
- Structured variants with template and properties for all levels

**Backward Compatibility:**
- Old Logger class (deprecated) remains functional
- SerilogLoggerAdapter implements ILogger interface
- Program.cs registers adapter for dependency injection
- Existing code using ILogger continues to work unchanged

### 4. Correlation ID Enricher (NEW in v1.2.0)

**Purpose:** Track operations across service boundaries for distributed tracing.

**Design:**
- Implements ILogEventEnricher interface
- Uses LogContext for context-aware property storage
- Automatic generation of unique operation IDs
- Thread-safe context propagation

**Methods:**
- `SetCorrelationId(string correlationId)` - Explicitly set ID
- `GetOrCreateCorrelationId()` - Get existing or generate new
- `Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)` - Enrichment implementation

**Use Case:**
```csharp
// Entry point
var correlationId = CorrelationIdEnricher.GetOrCreateCorrelationId();
GetService1();  // All logs automatically include correlationId
GetService2();  // Tracing across multiple services
```

### 5. ILogger Interface (EXPANDED in v1.2.0)

**Purpose:** Define logging contract for dependency injection and structured logging.

**Design:**
- Abstraction layer for logging implementation
- Overloaded methods supporting structured templates
- Support for all severity levels
- Exception detail capture

**Methods (v1.2.0):**
- `LogTrace(string msg)` / `LogTrace(template, params)`
- `LogDebug(string msg)` / `LogDebug(template, params)`
- `LogInfo(string msg)` / `LogInfo(template, params)`
- `LogWarning(string msg)` / `LogWarning(template, params)`
- `LogError(string msg)` / `LogError(msg, exception)` / `LogError(template, exception, params)`

### 6. Console Application (MassifCentral.Console)

**Purpose:** Entry point and orchestration of application startup and shutdown with Serilog logging initialization.

**Design (v1.2.0):**
- Serilog initialization before host creation
- Host builder with UseSerilog()
- Dependency injection configuration for logging
- Structured error handling with logging
- Graceful shutdown with Log.CloseAndFlush()

**Responsibilities:**
- Initialize Serilog based on environment
- Create Host with DI container
- Register core services including SerilogLoggerAdapter
- Register CorrelationIdEnricher for distributed tracing
- Execute business logic
- Log startup, initialization, and completion
- Handle exceptions with full logging
- Cleanup logging resources on shutdown

**Startup Flow (v1.2.0):**
1. **Serilog Log.Logger initialization** (App startup, before everything)
2. **Host.CreateDefaultBuilder()** - Create host with logging
3. **UseSerilog()** - Integrate Serilog with host logging
4. **ConfigureServices()** - Register MassifCentral services
5. **Register SerilogLoggerAdapter** - Implement ILogger interface
6. **Resolve logger from DI** - GetRequiredService<ILogger>()
7. **Structured logging** - Use ILogger with templated messages and properties
8. **Execute application logic**
9. **Log.CloseAndFlush()** - Ensure all logs written before exit
10. **Return appropriate exit code** (0 for success, 1 for error)

**Extension Strategy:**
- Add command-line argument parsing
- Add configuration file loading
- Add diagnostic mode toggle
- Add application-specific service registration

## SOLID Principles Application

### Single Responsibility Principle (SRP)
- **Constants** class - Only manages global constants
- **SerilogConfiguration** - Only handles environment-specific Serilog setup (NEW)
- **CorrelationIdEnricher** - Only manages operation correlation tracking (NEW)
- **SerilogLoggerAdapter** - Only adapts Serilog ILogger to application ILogger (NEW)
- **ILogger interface** - Only defines logging contract
- **BaseEntity** class - Only provides base entity functionality
- **Program.cs** - Only orchestrates application startup/shutdown

### Open/Closed Principle (OCP)
- **BaseEntity** - Open for extension (inheritance), closed for modification
- **SerilogConfiguration** - Open for new environment configurations (methods)
- **ILogger** - Open for new implementations (SerilogLoggerAdapter, Logger), closed for modification
- **Models namespace** - New entities can be added without modifying existing code

### Liskov Substitution Principle (LSP)
- **BaseEntity subclasses** - Must maintain the contract of a valid entity
- **SerilogLoggerAdapter** - Can substitute for any ILogger expectation
- Can be used anywhere ILogger is expected without breaking behavior

### Interface Segregation Principle (ISP)
- **ILogger interface** - Minimum required logging methods (no unused methods)
- **ILogEventEnricher interface** - Single Enrich method (Serilog requirement)
- Clients only depend on methods they use

### Dependency Inversion Principle (DIP)
- **Console application** depends on ILogger abstraction (not concrete Logger)
- **Program.cs** depends on ILogger interface registered in DI container
- **Services** depend on ILogger interface, not implementation
- Low-level logging module (Serilog) abstracted via ILogger interface

## Testing Strategy

### Unit Test Scope
- Individual class functionality
- Library constants verification
- BaseEntity initialization and properties
- ILogger interface and adapter functionality
- MockLogger capture and assertion methods

### Integration Test Scope (NEW in v1.2.0)
- Serilog configuration creation
- Sink file creation and content validation
- Production mode sink configuration (console errors, file warnings/errors)
- Diagnostic mode sink configuration (6-hour rolling, all levels)
- Development mode sink configuration (console all, file backup)
- Structured logging property inclusion
- Exception logging with stack traces
- Correlation ID enrichment
- Environment variable-based configuration selection

### Test Classes (v1.2.0)
- **LoggerTests** - ILogger interface and Logger class tests
- **LibraryTests** - Library constants and BaseEntity tests
- **SerilogIntegrationTests** (NEW) - Serilog configuration and sink behavior tests
- **MockLogger** - Capture mechanism with helper methods

### Test Framework
- **Framework:** xUnit v3
- **Configuration:** Configured in xunit.runner.json
- **Namespace:** MassifCentral.Tests

### Test Structure
- **Test Classes:** One per component (LoggerTests, LibraryTests, SerilogIntegrationTests)
- **Test Methods:** Named with Arrange-Act-Assert pattern
- **Coverage Goal:** >= 80% for all public APIs
- **Cleanup:** IDisposable for test artifact cleanup (log files)

## Data Flow

### Application Startup Flow (v1.2.0 with Serilog)

1. **Program.cs entry point executes**
2. **Serilog Log.Logger initialization** - Creates static logger based on environment
3. **Host.CreateDefaultBuilder()** - Builds DI container
4. **.UseSerilog()** - Integrates Serilog with framework logging
5. **.ConfigureServices()** - Registers application services
6. **Register SerilogLoggerAdapter** - Maps ILogger interface to Serilog
7. **Try block begins** - Application execution
8. **Resolve ILogger** from dependency injection container
9. **LogInfo()** - Structured logging: "Starting {ApplicationName} v{Version}"
   - Serilog Log.Logger outputs as JSON to configured sinks
   - SerilogConfiguration determines sink targets (console, file, or diagnostic)
10. **LogInfo()** - "Application initialized successfully"
11. **Business logic executes**
12. **LogInfo()** - "Application completed successfully"
13. **Catch block** - Exception caught at application level
14. **Log.Fatal()** - Serilog fatal error with exception details
15. **Return exit code** - 1 for error, 0 for success
16. **Finally block** - Log.CloseAndFlush() ensures all logs written
17. **Application exits** - Clean termination with flushed logs

### Environment-Based Sink Selection

```
Environment Variable Check
    │
    ├─ DIAGNOSTIC_MODE=true? 
    │  └─ Use Diagnostic Configuration
    │     └─ Single file, 6-hour window, all levels
    │
    └─ Check DOTNET_ENVIRONMENT
       │
       ├─ "Development"
       │  └─ Use Development Configuration
       │     └─ Console (all levels) + File
       │
       └─ "Production" or "Staging" (Default)
          └─ Use Production Configuration
             └─ Console (errors only) + File (warnings/errors)
```

### Structured Logging Data Flow

```
LogInfo("User {UserId} logged in", 123)
    │
    ├─ String template parsing: "User {UserId} logged in"
    ├─ Property binding: UserId = 123
    │
    ├─ Enrichment chain:
    │  ├─ FromLogContext (Correlation ID)
    │  ├─ WithEnvironmentUserName (OS user)
    │  ├─ WithMachineName (Hostname)
    │  ├─ WithProcessId (Process ID)
    │  └─ WithThreadId (Thread ID)
    │
    ├─ Sink dispatch:
    │  ├─ Console sink (if configured)
    │  └─ File sink (if configured)
    │
    └─ Output (JSON format):
       {
         "@t": "2026-02-07T14:23:45Z",
         "@mt": "User {UserId} logged in",
         "UserId": 123,
         "MachineName": "WORKSTATION-01",
         "ProcessId": 1234,
         "CorrelationId": "550e8400-..."
       }
```

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
