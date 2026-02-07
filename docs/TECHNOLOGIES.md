# MassifCentral - Technology Stack & Techniques Document

## Version Control
- **Version:** 1.2.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Updated logging and DI stack details to reflect Serilog implementation

---

## Executive Summary

MassifCentral is built on a modern, enterprise-grade .NET 10 platform with a focus on maintainability, scalability, and clean architecture principles. The project utilizes contemporary development practices including unit testing, semantic versioning, and structured documentation.

## Technology Stack

### Platform & Runtime

#### .NET Runtime
- **Version:** .NET 10.0 (Preview/RC)
- **Status:** Latest preview release
- **Support:** Long-term release cycle
- **Cross-platform:** Windows, Linux, macOS
- **Use Case:** Modern, high-performance application runtime with latest language features

**Why .NET 10?**
- Latest C# language features (C# 13)
- Performance improvements over previous versions
- Enhanced async/await capabilities
- Modern API design patterns
- Extensive standard library

### Programming Language

#### C# 13
- **Language Version:** Latest (13.0)
- **Features Utilized:**
  - Nullable reference types
  - Implicit usings
  - Extension methods
  - Auto-properties
  - Expression-bodied members
  - Pattern matching (future enhancement)

**Key Language Features Used:**
```csharp
// Implicit usings - Automatic imports
using System;
using System.Collections.Generic;

// Nullable reference types
public string? OptionalString { get; set; }

// Implicit usings in project file
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

### Testing Framework

#### xUnit.net v3
- **Package:** `xunit.v3` (v3.0.0)
- **Test Runner:** xUnit.net VSTest Adapter (v3.1.3)
- **SDK:** `Microsoft.NET.Test.Sdk` (v17.14.1)
- **IDE Integration:** `xunit.runner.visualstudio` (v3.1.3)

**Why xUnit?**
- Modern, open-source testing framework
- Built on excellent design principles
- Excellent IDE integration support
- Extensible assertion library
- Active community and good documentation

**Test Configuration:**
- Configuration file: `xunit.runner.json`
- Default test discovery enabled
- Support for Microsoft.Testing.Platform (optional)

### Project Structure & Build System

#### MSBuild
- **Version:** Included with .NET SDK
- **Configuration:** SDK-style project format
- **Features:**
  - Target framework: `net10.0`
  - Implicit usings enabled
  - Nullable reference types enabled
  - Automatic NuGet restore

#### Solution Organization
- **Format:** Visual Studio 2022 compatible (.slnx)
- **Projects:** 3 (.csproj files)
  - Console application
  - Class library
  - Unit test project

## Architectural Patterns & Techniques

### SOLID Principles Implementation

#### Single Responsibility Principle (SRP)
Each class has a single, well-defined responsibility:
- `Constants` class - Application-wide constants only
- `Logger` class - Logging concerns only
- `BaseEntity` class - Entity base functionality only
- `Program.cs` - Application orchestration only

#### Open/Closed Principle (OCP)
Classes are open for extension, closed for modification:
- **BaseEntity** - Can be extended through inheritance
- **Models namespace** - New entities added without modifying existing code
- **Utilities namespace** - New utilities added without breaking existing code

#### Liskov Substitution Principle (LSP)
Derived types can substitute base types without breaking contracts:
- All `BaseEntity` subclasses maintain entity contract
- Inheritance hierarchy is substitutable
- Method overrides maintain semantics

#### Interface Segregation Principle (ISP)
Clients depend only on interfaces they use:
- `ILogger` interface provides a narrow logging surface
- Serilog adapter implements the interface for DI-friendly usage

#### Dependency Inversion Principle (DIP)
Depend on abstractions, not concretions:
- Console application depends on library abstractions
- Logging depends on `ILogger` abstraction with adapter-backed implementation

### Design Patterns

#### Static Utility Pattern
Used for `Logger` and `Constants`:
- No instance creation required
- Direct access with type names
- Appropriate for utility functions
- Logger remains for backward compatibility; DI uses `ILogger` + Serilog adapter

#### Template Method Pattern
Enabled through `BaseEntity` inheritance:
- Base class provides structure (Id, timestamps)
- Derived classes provide specific behavior
- Consistent behavior across entities

#### Audit Trail Pattern
Built into `BaseEntity`:
- `CreatedAt` - Audit creation
- `ModifiedAt` - Audit tracking
- `IsActive` - Soft delete support

#### Repository Pattern (Future)
Planned for data access layer:
- Abstract data access implementation
- Enable persistence mechanism switching
- Enable testing with mock repositories

### Layered Architecture

```
Presentation Layer
    ↓ (uses)
Business Logic Layer (Shared Library)
    ↓ (uses)
Data Access Layer (Future)
    ↓ (uses)
Database/External Systems
```

**Current Layers:**
1. **Presentation** - Console application (Program.cs)
2. **Core Library** - Domain models, utilities, constants
3. **Testing** - Unit tests ensuring code quality

**Planned Layers:**
4. **Data Access** - Repositories and entity mapping
5. **Services** - Business logic and orchestration
6. **API** - External service interfaces

### Naming Conventions

#### Namespace Organization
```csharp
// Root namespace
MassifCentral                    // Application name

// Feature-based organization
MassifCentral.Lib               // Shared library
MassifCentral.Lib.Models        // Domain models
MassifCentral.Lib.Utilities     // Helper utilities
MassifCentral.Lib.Services      // Business services (future)

MassifCentral.Console           // Console application
MassifCentral.Tests             // Unit tests
```

#### Naming Standards
- **Classes:** PascalCase (e.g., `BaseEntity`, `Logger`)
- **Methods:** PascalCase (e.g., `LogInfo()`, `LogError()`)
- **Properties:** PascalCase (e.g., `CreatedAt`, `IsActive`)
- **Local variables:** camelCase (e.g., `entity`, `timestamp`)
- **Constants:** UPPER_SNAKE_CASE in static classes (e.g., `ApplicationName`)
- **Interfaces:** Prefix with "I" (e.g., `ILogger`)
- **Abstract classes:** Base prefix or Suffix Abstract (e.g., `BaseEntity`)

## Code Quality Practices

For comprehensive coding standards and guidelines, including documentation, formatting, naming conventions, testing practices, const visibility rules, and code review checklists, see the dedicated [Coding Guidelines](./CODING_GUIDELINES.md) document.

### Key Areas Covered in Coding Guidelines

- **XML Documentation Standards** - Complete documentation requirements for public APIs
- **Naming Conventions** - PascalCase, camelCase, UPPER_SNAKE_CASE rules for all code elements
- **Code Formatting** - Indentation, spacing, line length, and brace style
- **Testing Techniques** - Unit testing with Arrange-Act-Assert pattern, test naming
- **Const Visibility Rules** - Why `public const` is forbidden and `static readonly` required
- **Code Review Checklist** - Pre-submission and review validation steps
- **Anti-Patterns** - Common pitfalls to avoid

## Dependency Management

### NuGet Packages

#### Testing Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.NET.Test.Sdk` | 17.14.1 | Test execution and discovery |
| `xunit` | 3.0.0 | Core test framework |
| `xunit.runner.visualstudio` | 3.1.3 | IDE test runner integration |

#### Runtime Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| .NET Base Class Library | Included | Core framework |
| `Serilog` | 4.0.0 | Structured logging core |
| `Serilog.Sinks.Console` | 6.0.0 | Console logging sink |
| `Serilog.Sinks.File` | 5.0.0 | Rolling file sink |
| `Serilog.Extensions.Logging` | 8.0.0 | Microsoft logging integration |
| `Serilog.Formatting.Compact` | 3.0.0 | Compact JSON formatting |
| `Serilog.Enrichers.Environment` | 3.0.0 | Environment enrichment |
| `Serilog.Enrichers.Process` | 3.0.0 | Process enrichment |
| `Serilog.Enrichers.Thread` | 4.0.0 | Thread enrichment |

#### Future Dependencies (Planned)
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection
- **Configuration:** Microsoft.Extensions.Configuration
- **Database:** Entity Framework Core
- **Validation:** FluentValidation

### Dependency Management Strategy
- **Version Control:** NuGet package versions pinned in .csproj
- **Compatibility:** All packages target .NET 10
- **Updates:** Regular updates for security and features
- **Compatibility Matrix:** Maintain list of tested versions

## Build & Compilation

### Build Configuration

#### Debug Configuration
- **Optimization:** None (faster compilation)
- **Debug Info:** Full symbols and debugging support
- **Purpose:** Development and debugging

#### Release Configuration
- **Optimization:** Enabled (-O2 equivalent)
- **Debug Info:** No embedded symbols
- **Trimming:** (Future) Enable framework trimming
- **Purpose:** Production deployment

### Compilation Settings

```xml
<!-- In .csproj files -->
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
<LangVersion>latest</LangVersion>
```

### Build Artifacts

#### Output Artifacts
- **DLL:** Compiled assemblies (e.g., `MassifCentral.Lib.dll`)
- **EXE:** Executable console app (e.g., `MassifCentral.Console.exe` on Windows)
- **PDB:** Debug symbols (in Debug builds)

#### Build Output Locations
```
bin/
├── Debug/net10.0/           (Debug build)
│   ├── *.dll
│   ├── *.pdb
│   └── *.exe (on Windows)
└── Release/net10.0/         (Release build)
    ├── *.dll
    └── *.exe (on Windows)
```

## Development Tools & Environment

### Required Tools

#### .NET SDK
- **Minimum Version:** .NET 10 SDK
- **Download:** https://dotnet.microsoft.com/download
- **Verification:** `dotnet --version`

#### Code Editors
- **Visual Studio 2022** - Full IDE for .NET development
- **Visual Studio Code** - Lightweight with C# extension
- **JetBrains Rider** - Powerful IDE alternative

#### Essential IDE Extensions
- **C# Extension** (VS Code)
- **C# LanguageServices** (Visual Studio)
- **ReSharper** (Rider or VS - optional premium)

### Development Workflow

#### Local Development
```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project src/MassifCentral.Console

# Clean build artifacts
dotnet clean
```

#### Debugging
- **Visual Studio:** Built-in debugger with breakpoints
- **VS Code:** Debugger extension with OmniSharp
- **Command Line:** `dotnet run --no-build` with VS Code debugger

### Version Control

#### Git Configuration
- **Repository:** Located in `.git/` directory
- **Remote:** GitHub (configured in `.github/` folder)
- **Ignore File:** `.gitignore` (comprehensive C# patterns)

#### Branching Strategy (Recommended)
- **main:** Production releases only
- **develop:** Integration branch for features
- **feature/** - Feature development branches
- **bugfix/** - Bug fix branches
- **hotfix/** - Production hotfixes

## Performance Considerations

### Optimization Techniques

#### Current Optimizations
- **Static Classes:** Logger uses static methods (no allocation overhead)
- **Guid Generation:** Efficient `Guid.NewGuid()` in BaseEntity
- **DateTime:** Using UTC timestamps for consistency
- **Nullable Types:** Compile-time null safety

#### Future Performance Enhancements
- **Async Operations:** Async logging and I/O operations
- **Object Pooling:** Reuse frequently allocated objects
- **Caching:** Cache frequently accessed data
- **Lazy Loading:** Defer expensive operations
- **Benchmarking:** BenchmarkDotNet for performance testing

### Memory Management
- **Garbage Collection:** Automatic (Gen2 for BaseEntity instances)
- **Memory Profiling:** dotTrace or PerfView for analysis
- **Leak Detection:** VS debugging tools for memory leaks

## Security Practices

### Current Security Measures
- ✅ Nullable reference types (null safety)
- ✅ No hardcoded secrets
- ✅ No sensitive data in logs
- ✅ UTC timestamps (timezone safety)
- ✅ No external command injection

### Future Security Enhancements
- **Input Validation:** Implement validation framework
- **Authorization:** Role-based access control (RBAC)
- **Encryption:** Data encryption for sensitive fields
- **Secrets Management:** Secure configuration with user secrets
- **Audit Logging:** Security event tracking
- **Code Analysis:** Static analysis tools (SonarAnalyzer)

### Dependency Security
- **NuGet Audits:** Check for vulnerable packages
- **Version Pinning:** Specific version requirements
- **Security Updates:** Regular updates for dependencies

## Deployment & Distribution

### Build Output
- **Standalone Executable:** Single EXE file on Windows
- **Portable Binary:** Works across different Windows versions
- **Cross-platform:** Supports .NET on Linux and macOS

### Deployment Options

#### Self-Contained Deployment
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

#### Framework-Dependent Deployment
```bash
dotnet publish -c Release
```

### Continuous Integration (Future)
- **GitHub Actions:** Automated build and test
- **Build Triggers:** On push to branches
- **Test Automation:** Automatic test execution
- **Artifact Publishing:** Release package generation

## Future Technology Roadmap

### Phase 2: Infrastructure (Q2 2026)
- ✓ Dependency Injection container
- ✓ Configuration management system
- ✓ Interface abstraction for Logger
- ✓ Entity validation framework

### Phase 3: Data Access (Q3 2026)
- ✓ Entity Framework Core
- ✓ Database context setup
- ✓ Repository pattern implementation
- ✓ Migration system

### Phase 4: Services (Q4 2026)
- ✓ Business logic layer
- ✓ Service interfaces
- ✓ Domain-driven design patterns

### Phase 5: API Layer (Q1 2027)
- ✓ REST API with ASP.NET Core
- ✓ OpenAPI/Swagger documentation
- ✓ Authentication and authorization

### Phase 6: Advanced Features (Q2 2027+)
- ✓ Event-driven architecture
- ✓ Message queue integration
- ✓ Advanced caching strategies
- ✓ Real-time communication (SignalR)

## Technology Update Schedule

### Planned Reviews
- **Quarterly:** Review new .NET/C# features
- **Bi-annual:** Evaluate new NuGet packages
- **Annual:** Major technology assessment

### .NET Upgrade Path
- **.NET 10** → .NET 11 (2026 Q4)
- **.NET 11** → .NET 12 (2027 Q4)
- Keep within 2 major versions of latest

## Related Documentation

- [Requirements Document](./REQUIREMENTS.md) - Feature specifications
- [Design Document](./DESIGN.md) - Architecture and patterns
- [Coding Guidelines](./CODING_GUIDELINES.md) - Code style, documentation, and testing standards
- [Const Visibility Analysis](./assessments/CONST_VISIBILITY_ANALYSIS.md) - Risk analysis for const inlining
- [README.md](../README.md) - Quick start guide

## Appendix: Useful Resources

### Official Documentation
- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [xUnit.net](https://xunit.net/)

### Community Resources
- [Microsoft Learn](https://learn.microsoft.com)
- [Stack Overflow C# Tag](https://stackoverflow.com/questions/tagged/c%23)
- [GitHub C# Topics](https://github.com/topics/csharp)

### Performance Tools
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [dotTrace Profiler](https://www.jetbrains.com/profiler/)
- [PerfView](https://github.com/Microsoft/perfview)

### Code Analysis
- [ReSharper](https://www.jetbrains.com/resharper/)
- [Roslyn Analyzers](https://github.com/dotnet/roslyn-analyzers)
- [SonarAnalyzer](https://github.com/SonarSource/sonar-dotnet)
