# MassifCentral - Design Document

## Version Control
- **Version:** 1.0.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Initial architecture design and component specifications

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

## Future Architecture Evolution

1. **Phase 2:** Dependency injection and interface abstractions
2. **Phase 3:** Data access layer and repositories
3. **Phase 4:** Business logic service layer
4. **Phase 5:** API/Web service layer
5. **Phase 6:** Event-driven architecture with messaging

## Related Documents

- [Requirements Document](./REQUIREMENTS.md) - Detailed functional and non-functional requirements
- [README.md](../README.md) - Project overview and quick start guide
