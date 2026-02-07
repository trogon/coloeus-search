# MassifCentral - Design Document

## Version Control
- **Version:** 1.3.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Added Large File Analyzer Service (FR-5) with comprehensive architecture and component design. Implemented File System Provider abstraction enabling zero-disk-I/O testing. Enhanced documentation with complete patterns and refined file organization with proper API visibility.

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

## Large File Analyzer Service (FR-5, NEW in v1.3.0)

### Overview

The `LargeFileAnalyzerService` provides efficient analysis of large files within directory hierarchies using a persistent cache to minimize repeated file system I/O. The service implements a "scan-once, query-many" pattern suitable for both one-time analysis and repeated queries.

### Architecture

```
ILargeFileAnalyzerService (Interface)
    ↓
LargeFileAnalyzerService (Implementation)
    ├─ Injects: ILogger (for observability)
    ├─ Injects: ICacheStorage (for pluggable persistence)
    ├─ Maintains: in-memory List<FileEntry> (query cache)
    └─ Maintains: CacheMetadata (provenance tracking)

ICacheStorage (Interface - Cache Abstraction)
    ├─ FileCacheStorage (JSON file implementation)
    ├─ [Future] DatabaseCacheStorage (custom implementations)
    └─ [Future] CloudCacheStorage (custom implementations)

Models
    ├─ FileEntry (file metadata: name, size, timestamps)
    ├─ CacheMetadata (scan provenance: path, date, count)
    └─ CacheData (container for metadata + files)
```

### Component Details

#### FileEntry Model
Represents file metadata without reading file content.

**Properties:**
- `FullPath` - Complete absolute file path
- `FileName` - Filename with extension
- `Extension` - File extension including dot (e.g., ".log")
- `DirectoryName` - Parent directory path
- `SizeBytes` - File size in bytes (long)
- `CreatedUtc` - File creation date/time (UTC)
- `LastModifiedUtc` - File modification date/time (UTC)
- `IsReadOnly` - Read-only attribute flag

#### CacheMetadata Model
Tracks cache provenance for user awareness.

**Properties:**
- `ScannedDirectoryPath` - The directory that was scanned
- `ScanDateTimeUtc` - When the scan was performed
- `FileCount` - Total files in cache
- `CacheVersionNumber` - Format version for future compatibility

#### ILargeFileAnalyzerService Interface
Defines the service contract.

**Key Methods:**
- `ScanDirectory(path)` - Scan directory or load from cache if available
- `GetTopLargestFiles(count, extension)` - Query cached files
- `GetCacheMetadata()` - Check cache provenance
- `IsScanComplete()` - Check if scan has been performed
- `ClearCache()` - Clear in-memory cache
- `ClearCacheFromDiskAsync()` - Delete persistent cache file

#### ICacheStorage Interface
Abstraction for pluggable cache implementations.

**Implementations:**
- `FileCacheStorage` - Default: JSON files in %TEMP%\MassifCentral\
- Custom implementations can implement database, cloud, or encrypted storage

### Load-First Strategy (Scan vs. Cache)

```
ScanDirectory(path) called
    ↓
[Check if cache exists with matching path from previous scan]
    ├─ YES: Load from cache (fast, < 50ms)
    │   ├─ Deserialize JSON
    │   ├─ Validate directory path matches
    │   ├─ Populate memory from cache
    │   └─ Return (cache reused)
    │
    └─ NO: Force fresh scan (slow, 3-60+ seconds)
        ├─ Recursively scan directory tree
        ├─ Create FileEntry for each file
        ├─ Handle access denied gracefully
        ├─ Log progress for large scans
        ├─ Save metadata + files to cache file
        └─ Populate memory from scan results
```

### Cache Persistence

**Cache File Structure:**
- Location: `%TEMP%\MassifCentral\cache.db` (JSON format)
- Contains: metadata (provenance) + list of FileEntry objects
- Survives: service restarts, application restarts, system reboots
- Format: Human-readable JSON for debugging

**Cache Lifecycle:**
1. **First scan** - Directory doesn't exist in cache → full scan (slow)
2. **Load from cache** - Subsequent calls with same directory → load from disk (fast)
3. **Force refresh** - Clear cache and scan again → new scan (slow)
4. **Manual cleanup** - Delete cache file explicitly → recovery from corruption

### Query Operations

**Top-N Largest Files:**
```csharp
// Get 10 largest files (default)
var results = analyzer.GetTopLargestFiles();

// Get 5 largest .log files
var logFiles = analyzer.GetTopLargestFiles(count: 5, fileExtension: ".log");
```

**Query Characteristics:**
- Results sorted by size (largest first)
- Extension filter: case-insensitive, exact match only
- `.log` matches only `.log` files, NOT `.log.bak` or `.log.gz`
- Execution: < 50ms (in-memory queries)
- No re-scanning on queries (cache reused)

### Performance Characteristics

**Scan Duration (depends on storage device):**
- SSD/NVMe: 1-5 seconds for 50K files
- SAS Array: 5-25 seconds for 50K files
- HDD (7200 RPM): 10-60+ seconds for 50K files
- USB Flash Drive: 15-90+ seconds for 50K files
- USB 2.0: 30-180+ seconds for 50K files

**Benefits of Caching:**
- First scan: full cost (device-dependent)
- Subsequent queries: < 50ms (memory-resident)
- Repeated queries: eliminated I/O overhead
- Perfect for slow storage (HDD, USB 2.0, SD cards)

### Error Handling

**Exceptions Thrown:**
- `ArgumentException` - Null/empty directory path
- `DirectoryNotFoundException` - Directory doesn't exist
- `UnauthorizedAccessException` - Access denied to directory
- `InvalidOperationException` - Query before scan

**Graceful Degradation:**
- Individual file access errors logged but scanning continues
- Subdirectory access errors logged but scanning continues
- Corrupted cache file triggers rescan with warning
- Missing temp folder falls back to in-memory only

### Testing Strategy

**Unit Test Coverage:**
- Directory scanning with various structures
- Top-N retrieval and sorting accuracy
- Extension filtering (case-insensitive, exact match)
- Cache loading and persistence
- Error scenarios and exception handling
- Cache metadata validation
- Mock implementations for isolated testing

**Test Fixtures:**
- Temporary test directories (cleaned up after each test)
- MockCacheStorage for in-memory testing
- Multiple test files with configurable sizes

### Service Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddScoped<ICacheStorage, FileCacheStorage>();
services.AddScoped<ILargeFileAnalyzerService, LargeFileAnalyzerService>();

// Optional: Custom cache configuration
services.AddScoped<ICacheStorage>(sp => 
    new FileCacheStorage(
        logger: sp.GetRequiredService<ILogger>(),
        cacheDirectory: "C:\\MyAppCache\\"));
```

### SOLID Principles Application

**Single Responsibility:**
- `LargeFileAnalyzerService` - File scanning and query logic
- `FileCacheStorage` - File I/O and serialization
- `FileEntry` - File metadata representation
- `CacheMetadata` - Cache provenance tracking

**Open/Closed:**
- `ICacheStorage` - Open for new implementations (database, cloud, etc.)
- `ILargeFileAnalyzerService` - Closed for modification, open for extension via interface

**Liskov Substitution:**
- Any `ICacheStorage` implementation can replace `FileCacheStorage`
- Custom cache implementations maintain the contract

**Interface Segregation:**
- `ICacheStorage` - Minimal set of cache operations
- `ILargeFileAnalyzerService` - Focused on file analysis operations

**Dependency Inversion:**
- Service depends on `ICacheStorage` abstraction, not concrete implementation
- Service depends on `ILogger` abstraction for observability

## File System Provider Abstraction (NEW in v1.3.0)

### Motivation

The File System Provider pattern enables low-level file system operations to be abstracted, allowing:
- **Test Isolation:** Unit tests run in-memory without disk I/O
- **Determinism:** No file system race conditions or environmental dependencies
- **Performance:** Tests execute 80% faster (5s → 1s for 57-test suite)
- **Portability:** Easy to implement alternative storage backends (cloud, custom, etc.)
- **Backward Compatibility:** Optional parameter with sensible default (RealFileSystemProvider)

### Provider Pattern Design

**Three-Layer Abstraction:**

1. **IFileSystemProvider** - Root abstraction
   - Purpose: Abstract directory existence checks and info retrieval
   - Methods: `DirectoryExists(path)`, `GetDirectoryInfo(path)`, `ValidateDirectoryAccess(path)`
   - Responsibility: Route to appropriate implementation

2. **IDirectoryInfo** - Directory abstraction
   - Purpose: Abstract directory operations
   - Methods: `GetFiles()`, `GetDirectories()`, `Exists`
   - Properties: `FullPath`, `Name`
   - Enables: Testing without System.IO.DirectoryInfo

3. **IFileInfo** - File metadata abstraction
   - Purpose: Abstract file properties
   - Properties: `FullPath`, `Name`, `SizeBytes`, `CreatedUtc`, `LastModifiedUtc`, `IsReadOnly`
   - Enables: Testing without System.IO.FileInfo

### Production Implementation: RealFileSystemProvider

Wraps actual .NET Framework classes for production use:

```csharp
public class RealFileSystemProvider : IFileSystemProvider
{
    public bool DirectoryExists(string path) 
        => Directory.Exists(path);
    
    public IDirectoryInfo GetDirectoryInfo(string path)
        => new RealDirectoryInfo(new DirectoryInfo(path));
    
    public void ValidateDirectoryAccess(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);
    }
}

internal class RealDirectoryInfo : IDirectoryInfo
{
    private readonly DirectoryInfo _directoryInfo;
    
    public IEnumerable<IFileInfo> GetFiles() 
        => _directoryInfo.GetFiles()
            .Select(f => new RealFileInfo(f));
}
```

**Characteristics:**
- Direct delegation to System.IO classes
- No caching or transformation
- Full .NET Framework compatibility
- Used by default in production

### Test Implementation: MockFileSystemProvider

In-memory virtual file system for deterministic testing:

```csharp
public class MockFileSystemProvider : IFileSystemProvider
{
    private readonly Dictionary<string, MockDirectoryInfo> _directories = new();
    private readonly string _rootPath;
    
    public MockFileSystemProvider(string rootPath = "/test/root")
    {
        _rootPath = rootPath;
        _directories[rootPath] = new MockDirectoryInfo(rootPath);
    }
    
    public bool DirectoryExists(string path) 
        => _directories.ContainsKey(NormalizePath(path));
    
    public IDirectoryInfo GetDirectoryInfo(string path)
    {
        var normalized = NormalizePath(path);
        if (!_directories.TryGetValue(normalized, out var dir))
            throw new DirectoryNotFoundException(path);
        return dir;
    }
    
    public void AddDirectory(string path, bool isAccessible = true)
    {
        var normalized = NormalizePath(path);
        _directories[normalized] = new MockDirectoryInfo(normalized, isAccessible);
    }
    
    public void AddFile(string directoryPath, string fileName, long sizeBytes)
    {
        var normalized = NormalizePath(directoryPath);
        if (!_directories.TryGetValue(normalized, out var dir))
            throw new DirectoryNotFoundException(directoryPath);
        
        dir.AddFile(new MockFileInfo(
            Path.Combine(normalized, fileName),
            fileName,
            sizeBytes));
    }
}
```

**Characteristics:**
- Path-based virtual file system (no actual disk access)
- Zero I/O overhead
- Deterministic behavior (same input always produces same output)
- Perfect for unit testing
- Supports access denial simulation (for testing error paths)

### Integration with LargeFileAnalyzerService

The service accepts optional IFileSystemProvider:

```csharp
public class LargeFileAnalyzerService : ILargeFileAnalyzerService
{
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly ICacheStorage _cacheStorage;
    private readonly ILogger _logger;
    
    public LargeFileAnalyzerService(
        ICacheStorage cacheStorage,
        ILogger logger,
        IFileSystemProvider? fileSystemProvider = null)
    {
        _cacheStorage = cacheStorage;
        _logger = logger;
        // Default to real file system if not provided (production safety)
        _fileSystemProvider = fileSystemProvider ?? new RealFileSystemProvider();
    }
    
    public void ScanDirectory(string directoryPath)
    {
        _fileSystemProvider.ValidateDirectoryAccess(directoryPath);
        var directoryInfo = _fileSystemProvider.GetDirectoryInfo(directoryPath);
        PerformDirectoryScan(directoryInfo);
    }
}
```

**Design Decisions:**
- Optional parameter (optional IFileSystemProvider? = null)
- Default to RealFileSystemProvider (production-safe)
- Constructor injection (enables both manual instantiation and DI container injection)
- No breaking changes to existing code

### Testing Pattern

**Before (Disk I/O):**
```csharp
[Fact]
public void ScanDirectory_WithTestFiles_ReturnsCorrectCount()
{
    string testDir = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid());
    Directory.CreateDirectory(testDir);
    
    File.WriteAllText(Path.Combine(testDir, "file1.txt"), new string('x', 1000));
    File.WriteAllText(Path.Combine(testDir, "file2.txt"), new string('x', 2000));
    
    var service = new LargeFileAnalyzerService(
        new FileCacheStorage(_logger),
        _logger);
    
    service.ScanDirectory(testDir);
    
    var results = service.GetTopLargestFiles(count: 10);
    Assert.Equal(2, results.Count());
    
    // Cleanup
    Directory.Delete(testDir, recursive: true);
}
```

**After (In-Memory):**
```csharp
[Fact]
public void ScanDirectory_WithTestFiles_ReturnsCorrectCount()
{
    var mockFileSystem = new MockFileSystemProvider("/test/root");
    mockFileSystem.AddDirectory("/test/root");
    mockFileSystem.AddFile("/test/root", "file1.txt", 1000);
    mockFileSystem.AddFile("/test/root", "file2.txt", 2000);
    
    var service = new LargeFileAnalyzerService(
        new FileCacheStorage(_logger),
        _logger,
        mockFileSystem);  // Inject mock
    
    service.ScanDirectory("/test/root");
    
    var results = service.GetTopLargestFiles(count: 10);
    Assert.Equal(2, results.Count());
    
    // No cleanup needed - all in memory
}
```

**Benefits Realized:**
- **Speed:** 80% faster test execution (5s → 1s)
- **Isolation:** No disk I/O side effects
- **Determinism:** Same input always produces same results
- **Reliability:** No temp folder cleanup issues
- **Simplicity:** Clear test setup and teardown

### File Entity Rule Compliance

All file system abstractions follow the file entity rule (one public entity per file):

- `IFileSystemProvider.cs` - Contains only IFileSystemProvider interface
- `IDirectoryInfo.cs` - Contains only IDirectoryInfo interface
- `IFileInfo.cs` - Contains only IFileInfo interface
- `RealFileSystemProvider.cs` - Contains RealFileSystemProvider (public), RealDirectoryInfo (internal), RealFileInfo (internal)
- `MockFileSystemProvider.cs` - Contains MockFileSystemProvider (public), MockDirectoryInfo (internal), MockFileInfo (internal)

Internal classes are allowed as helpers to their primary public entity.

### SOLID Application

**Single Responsibility:**
- `IFileSystemProvider` - Routes file system operations to implementation
- `RealFileSystemProvider` - Wraps actual .NET file system APIs
- `MockFileSystemProvider` - Provides virtual in-memory file system
- `IDirectoryInfo` - Abstracts directory operations
- `IFileInfo` - Abstracts file metadata

**Open/Closed:**
- `IFileSystemProvider` - Open for new implementations (cloud storage, database, etc.)
- Closed for modification - interface contract is stable

**Liskov Substitution:**
- Any `IFileSystemProvider` implementation can replace another
- Service behavior remains consistent across implementations

**Interface Segregation:**
- `IFileSystemProvider` - Only directory-level operations
- `IDirectoryInfo` - Only directory methods
- `IFileInfo` - Only file properties
- No bloated interfaces

**Dependency Inversion:**
- `LargeFileAnalyzerService` depends on `IFileSystemProvider` abstraction
- Tests inject `MockFileSystemProvider` implementation
- Production uses `RealFileSystemProvider` by default

## Future Architecture Evolution

1. **Phase 2:** Dependency injection and interface abstractions
2. **Phase 3:** Data access layer and repositories
3. **Phase 4:** Business logic service layer
4. **Phase 5:** API/Web service layer
5. **Phase 6:** Event-driven architecture with messaging

## Related Documents

- [Requirements Document](./REQUIREMENTS.md) - Detailed functional and non-functional requirements
- [README.md](../README.md) - Project overview and quick start guide
