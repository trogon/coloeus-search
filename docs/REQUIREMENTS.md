# MassifCentral - Project Requirements Document

## Version Control
- **Version:** 1.2.1
- **Last Updated:** 2026-02-07
- **Change Summary:** Added dotnet tool functional requirement and updated packaging-related requirements.

---

## Project Overview

MassifCentral is a .NET 10 foundational framework that enables development teams to build scalable, maintainable console and service applications. The framework provides essential infrastructure including dependency injection, logging, entity management, and configuration management, allowing teams to focus on business logic rather than boilerplate code.

---

## Functional Requirements (Business-Level)

### FR-1: Enable Rapid Application Development
**Business Value:** High  
**Goal:** Reduce time-to-market for new .NET applications by providing a pre-configured foundation with essential services.

**User Stories:**
- As a developer, I want to start a new application without building infrastructure from scratch, so I can focus on business features
- As a team lead, I want a consistent application structure across projects, so I can manage multiple applications efficiently
- As a DevOps engineer, I want standardized logging and configuration across all applications, so I can manage operations at scale

**Acceptance Criteria:**
- New applications can be created using MassifCentral in less than 5 minutes
- Developers can add custom business logic without modifying framework code
- Framework updates don't break existing applications

### FR-2: Support Application Extensibility
**Business Value:** High  
**Goal:** Enable teams to extend the framework with domain-specific functionality without modifying core code.

**User Stories:**
- As a developer, I want to add my own services to the application container, so I can integrate custom business logic
- As an architect, I want to enforce architectural patterns across applications, so I maintain consistency
- As a team, I want to share common code across projects, so I reduce duplication

**Acceptance Criteria:**
- Framework provides clear extension points for custom services
- Extension mechanism supports dependency injection patterns
- Multiple applications can reference the framework library without conflicts

### FR-3: Provide Observability Into Application Behavior
**Business Value:** Medium  
**Goal:** Enable teams to observe, diagnose, and troubleshoot application issues through comprehensive logging and monitoring.

**User Stories:**
- As an operator, I want to see structured logs from the application, so I can diagnose issues
- As a developer, I want to add logging to my code without coupling to logging implementation, so I can swap implementations later
- As a support team, I want logs with timestamps and error context, so I can quickly identify issues

**Acceptance Criteria:**
- All application events are logged with appropriate context
- Logs include timestamps for debugging time-sensitive issues
- Exception information is captured with stack traces
- Logging implementation can be changed without modifying application code

### FR-4: Provide Dotnet Tool Access
**Business Value:** Medium  
**Goal:** Allow users to install and run the console app as a global or local dotnet tool.

**User Stories:**
- As a developer, I want to install the tool via `dotnet tool`, so I can run it without cloning the repository
- As a CI engineer, I want to invoke the tool from scripts using the dotnet tool command line, so I can automate workflows

**Acceptance Criteria:**
- The console app is packaged as a dotnet tool
- The tool can be installed using `dotnet tool install` from a NuGet feed
- The tool can be executed from the command line after installation

---

## Technical Requirements (Implementation Details)

These technical requirements specify how the functional requirements are fulfilled through the framework's architecture and components.

### TR-1: Console Application Infrastructure
**Supports:** FR-1, FR-2, FR-5  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- Application must be executable from command line
- Application must initialize all services before running business logic
- Application must handle shutdown gracefully
- Application must catch and log unhandled exceptions
- Application must exit with appropriate status codes

**Implementation Components:**
- Program.cs entry point
- Host.CreateDefaultBuilder configuration
- Service collection setup

### TR-2: Shared Library with Reusable Components
**Supports:** FR-2, FR-4  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- Library must provide BaseEntity class for domain models
- Library must provide constants for application-wide values
- Library must be packagable as reusable assembly
- Library must have minimal external dependencies
- Library must be testable with unit tests

**Implementation Components:**
- MassifCentral.Lib project
- Constants.cs (application identification)
- Models/BaseEntity.cs (entity base class)

### TR-3: Abstraction-Based Logging Framework with Serilog
**Supports:** FR-3, FR-5  
**Implementation Status:** ✅ COMPLETED (v1.2.0)

**Technical Specifications:**
- Logger must implement ILogger interface with structured logging support
- Logger must support all severity levels: Trace, Debug, Info, Warning, Error
- Logger must output structured JSON format for searchability
- Logger must include UTC timestamps in all entries
- Logger must handle exceptions with full stack traces
- Logger must be registered as Singleton service
- Logger must be injectable into other services
- Logger must support context enrichment (correlation IDs, machine info, process info, thread info)
- Production Mode: Console sink outputs errors only, rolling file sink captures warnings and errors
- Diagnostic Mode: Single file sink with 6-hour retention window captures all levels (Trace through Error)
- Development Mode: Console sink with all levels, rolling file backup
- Correlation ID enricher must track operations across service boundaries
- Structured properties must enable rich log queries and filtering

**Implementation Components:**
- ILogger interface (expanded in v1.2.0 with Trace, Debug, structured methods)
- Logger implementation class (maintained for backward compatibility, marked obsolete)
- SerilogLoggerAdapter (new in v1.2.0, implements ILogger using Serilog)
- SerilogConfiguration.cs (new in v1.2.0, environment-specific configurations)
- CorrelationIdEnricher.cs (new in v1.2.0, distributed tracing support)
- ServiceCollectionExtensions registration

### TR-4: Dependency Injection Container
**Supports:** FR-1, FR-2, FR-5  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- Application must use Microsoft.Extensions.DependencyInjection
- Services must be registered in ServiceCollectionExtensions
- Lifetimes must be explicitly specified (Transient, Scoped, Singleton)
- DI container must be configured via Host.CreateDefaultBuilder
- Service resolution must fail fast with clear error messages
- All service registrations must be documented

**Implementation Components:**
- ServiceCollectionExtensions.cs
- Program.cs Host configuration
- Service registration patterns

### TR-5: Test Infrastructure with Mocks
**Supports:** FR-5, FR-3  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- MockLogger must implement ILogger interface
- Mock implementations must capture method calls
- Test framework must be xUnit
- All tests must use Arrange-Act-Assert pattern
- Tests must achieve >= 80% code coverage

**Implementation Components:**
- Mocks/MockLogger.cs
- LoggerTests.cs
- LibraryTests.cs

### TR-6: Reusable Component Library
**Supports:** FR-2, FR-3  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- Library must provide base classes for domain entities (BaseEntity)
- Library must expose application constants
- Library must be packagable as reusable NuGet assembly
- Library must maintain backward compatibility across versions
- Library dependencies must be documented and limited to approved NuGet packages

**Implementation Components:**
- MassifCentral.Lib project
- Models/BaseEntity.cs (domain entity base class)
- Constants.cs (application constants)

### TR-7: Interface-Based Architecture with Dependency Injection
**Supports:** FR-1, FR-2, FR-3  
**Implementation Status:** ✅ COMPLETED (v1.1.0)

**Technical Specifications:**
- All service interfaces must be defined to enable dependency injection
- All service implementations must be registered in ServiceCollectionExtensions
- All public dependencies must be injected via constructor
- Service lifetimes must be explicitly documented (Transient, Scoped, Singleton)
- Loose coupling must enable easy substitution of implementations (e.g., for testing)

**Implementation Components:**
- ILogger interface and Logger implementation
- ServiceCollectionExtensions service registration
- DI container configuration in Program.cs

---

## Non-Functional Requirements

### Performance
- Application startup time must be under 2 seconds
- Service resolution from DI container must be < 1ms per instance
- Logging operations must not block application execution
- Framework must support at least 1,000 concurrent operations in memory

### Scalability
- Framework must support 10+ independent applications built on it
- DI container must handle 100+ registered services without degradation
- Shared library must evolve without breaking dependent applications
- Architecture must scale from single-application to multi-application deployments

### Maintainability
- All public code must have XML documentation comments
- Code must follow SOLID principles
- Project structure must follow standard .NET conventions
- All public API changes must be tracked in CHANGELOG

### Reliability
- All unhandled exceptions must be logged before application exit
- All library components must have >= 80% unit test coverage
- All tests must pass before accepting code changes
- DI configuration errors must provide clear diagnostic messages

### Security
- Sensitive information must not be logged
- Exception messages must not expose internal system details
- User input validation must be enforced (future requirement)
- Secrets must not be stored in code (future requirement)

---

## Requirement Traceability

| Functional Requirement | Technical Requirements | Primary Components |
|------------------------|------------------------|-------------------|
| FR-1: Rapid Development | TR-1, TR-4, TR-7 | Program.cs, ServiceCollectionExtensions, DI setup |
| FR-2: Extensibility | TR-1, TR-4, TR-6, TR-7 | DI Container, Shared Library, Extension points |
| FR-3: Observability | TR-3, TR-5, TR-7 | ILogger, Logger, MockLogger, Test infrastructure |

---

## Requirement Status Summary

| Category | Total | Status |
|----------|-------|--------|
| **Functional Requirements (Business-Level)** | 3 | ✅ All satisfied |
| **Technical Requirements (Implementation)** | 7 | ✅ All implemented |
| **Non-Functional Requirements** | 5 | ✅ 4/5 met, 1 deferred |

**Note:** Security requirement for user input validation deferred to Phase 2

---

## Future Enhancements (Phase 2+)

These enhancements align with and support the functional requirements:

- Configuration management system (supports FR-1: Rapid Development)
- Database persistence layer (supports FR-2: Extensibility, TR-6: Reusable Components)
- API/Web service layer (supports FR-1: Rapid Development, FR-2: Extensibility)
- Advanced logging with file output (supports FR-3: Observability)
- Entity validation framework (supports TR-6: Reusable Components)
- Repository pattern implementation (supports FR-2: Extensibility, TR-7: Interface-Based Architecture)
- Event-driven architecture with messaging (supports FR-2: Extensibility)
- User input validation framework (supports Security NFR)

---

## Related Documents

- [Design Document](./DESIGN.md) - Technical architecture and patterns
- [Dependency Injection Guide](./DEPENDENCY_INJECTION.md) - DI implementation details
- [Implementation Summary v1.1.0](./IMPLEMENTATION_SUMMARY_v1.1.0.md) - Status of implementation
- [Architecture Analysis](./ARCHITECTURE_ANALYSIS.md) - Architecture assessment
- [CHANGELOG.md](../CHANGELOG.md) - Version history and release notes
