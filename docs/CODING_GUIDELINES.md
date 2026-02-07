# MassifCentral - Coding Guidelines & Standards

## Version Control
- **Version:** 1.0.0
- **Last Updated:** 2026-02-07
- **Change Summary:** Initial coding standards and guidelines for C# development in MassifCentral

---

## Overview

This document establishes the coding standards and guidelines for the MassifCentral project. All developers must follow these guidelines to maintain code quality, consistency, and maintainability across the codebase.

## Documentation Standards

### XML Documentation Comments

All public types, methods, and properties must be documented with XML comments that will be compiled into documentation.

#### Required Documentation

- ✅ All public classes
- ✅ All public methods
- ✅ All public properties
- ✅ All public interfaces
- ✅ Method parameters (`<param>`)
- ✅ Return values (`<returns>`)
- ✅ Exceptions (`<exception>`)

#### Example: Complete XML Documentation

```csharp
/// <summary>
/// Provides simple logging functionality for the application.
/// This class contains static methods for logging messages at different severity levels.
/// </summary>
/// <remarks>
/// Current implementation writes to console output. Future versions may support
/// multiple output targets and configurable log levels.
/// </remarks>
public static class Logger
{
    /// <summary>
    /// Logs an informational message with a UTC timestamp.
    /// </summary>
    /// <param name="message">The message to log. Cannot be null or empty.</param>
    /// <exception cref="ArgumentException">Thrown when message is null or empty.</exception>
    /// <remarks>
    /// The timestamp is always in UTC format (yyyy-MM-dd HH:mm:ss) for consistency
    /// across distributed systems.
    /// </remarks>
    public static void LogInfo(string message)
    {
        if (string.IsNullOrEmpty(message))
            throw new ArgumentException("Message cannot be null or empty.", nameof(message));
        
        Console.WriteLine($"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs an error message with exception details and stack trace.
    /// </summary>
    /// <param name="message">The error description.</param>
    /// <param name="exception">The exception that caused the error.</param>
    /// <exception cref="ArgumentNullException">Thrown when exception is null.</exception>
    public static void LogError(string message, Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));
        
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }
}
```

#### XML Documentation Best Practices

- **Be Concise:** Keep summaries brief (one sentence preferred)
- **Be Clear:** Use simple language, avoid jargon
- **Be Complete:** Document all parameters and return values
- **Include Examples:** Complex methods should include code examples
- **Document Exceptions:** List all exceptions that can be thrown
- **Use Remarks:** Explain implementation details and gotchas
- **Link Related Items:** Use `<see cref="..."/>` for cross-references

## Code Style & Formatting

### Naming Conventions

#### Type Names (Classes, Interfaces, Records, Enums)
- **Style:** PascalCase
- **Pattern:** `[Adjective|Noun]+Entity|Service|Manager|Factory`
- **Examples:**
  - `BaseEntity` - Base class for entities
  - `Logger` - Static utility class
  - `IRepository` - Interface for repository pattern
  - `UserService` - Service class

#### Method Names
- **Style:** PascalCase
- **Pattern:** Verb + Noun describing action
- **Examples:**
  - `LogInfo()` - Log information
  - `GetUser()` - Retrieve a user
  - `SaveEntity()` - Persist entity
  - `ValidateInput()` - Validate input data

#### Property Names
- **Style:** PascalCase
- **Pattern:** Noun describing what the property holds
- **Examples:**
  - `Id` - Identifier
  - `CreatedAt` - Creation timestamp
  - `IsActive` - Active status flag
  - `FullName` - Full name of person

#### Parameter & Local Variable Names
- **Style:** camelCase
- **Pattern:** Descriptive short names
- **Examples:**
  - `entity` - Domain entity
  - `userId` - User identifier
  - `isValid` - Boolean flag
  - `maxRetries` - Maximum retry count

#### Constant Names
- **Style:** UPPER_SNAKE_CASE (in constants class)
- **Pattern:** All caps with underscores
- **Examples:**
  - `DEFAULT_TIMEOUT_MS` - Default timeout milliss
  - `MAX_ATTEMPTS` - Maximum attempts
  - `API_VERSION` - API version string

#### Interface Names
- **Style:** PascalCase with "I" prefix
- **Pattern:** `I[Noun|Adjective]+[Purpose]`
- **Examples:**
  - `ILogger` - Logging interface
  - `IRepository` - Data repository interface
  - `IValidator` - Validation interface

#### Generic Type Parameters
- **Style:** PascalCase with "T" prefix
- **Pattern:** `T[Constraint|Purpose]`
- **Examples:**
  - `T` - Single generic parameter
  - `TEntity` - Generic entity type
  - `TResult` - Generic result type

### File Organization Rules

#### One Entity Per File

**CRITICAL RULE:** Each C# source file must contain exactly one public entity (class, interface, enum, record, struct, or delegate).

**Rationale:**
- Improves code organization and discoverability
- Makes file structure predictable and easy to navigate
- Simplifies version control and blame history
- Reduces cognitive load when reading code
- Follows .NET community conventions (StyleCop, Microsoft guidelines)

#### File Naming Convention

**Rule:** File name must exactly match the entity name with `.cs` extension.

**Pattern:** `[EntityName].cs`

**Examples:**

| Entity Type | Entity Name | File Name |
|------------|------------|-----------|
| Public Class | `Logger` | `Logger.cs` |
| Public Interface | `IRepository` | `IRepository.cs` |
| Public Enum | `LogLevel` | `LogLevel.cs` |
| Public Record | `UserData` | `UserData.cs` |
| Public Struct | `Point` | `Point.cs` |

**Correct Structure:**

```
src/
├── MassifCentral.Lib/
│   ├── Models/
│   │   ├── BaseEntity.cs      (contains: public abstract class BaseEntity)
│   │   ├── User.cs            (contains: public class User : BaseEntity)
│   │   └── LogLevel.cs        (contains: public enum LogLevel)
│   ├── Contracts/
│   │   ├── IRepository.cs     (contains: public interface IRepository<T>)
│   │   └── ILogger.cs         (contains: public interface ILogger)
│   ├── Services/
│   │   ├── Logger.cs          (contains: public static class Logger)
│   │   └── UserService.cs     (contains: public class UserService)
│   └── Constants.cs           (contains: public static class Constants)
```

**Incorrect Structure (violates one-entity-per-file rule):**

```csharp
// ❌ BAD: Multiple entities in one file
// File: Models.cs
public class User { }
public class Product { }
public enum Status { }
```

#### Private / Helper Classes Exception

**Exception:** Helper classes, nested types, or implementations with limited scope can coexist in the same file as their primary entity:

```csharp
// ✅ ALLOWED: Helper type in main file
// File: UserRepository.cs
public class UserRepository : IRepository<User>
{
    // Implementation
    
    // Private helper class - allowed because it's internal to UserRepository
    private class UserValidator
    {
        // Validation logic
    }
}
```

**Guideline:** If a helper class or nested type could be useful elsewhere or is complex enough to understand independently, extract it to its own file.

### Formatting Rules

#### C# Style Guide
- **Brace Style:** Allman style (opening brace on new line)
- **Indentation:** 4 spaces (no tabs)
- **Line Length:** Soft limit of 120 characters
- **Spacing:** Single blank line between methods/properties

#### Correct Formatting Example

```csharp
namespace MassifCentral.Lib.Models;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the modification timestamp.</summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the entity is active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Marks the entity as inactive (soft delete).
    /// </summary>
    public virtual void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}
```

#### Line Breaking
```csharp
// Acceptable: Fits in 120 characters
var result = validator.Validate(entity, CancellationToken.None);

// Line too long - break it
var result = validator.Validate(
    entity,
    CancellationToken.None);

// Method chaining - each method on new line
var data = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.CreatedAt)
    .ToList();
```

#### Spacing

**Good spacing:**
```csharp
public string GetFormattedName(string firstName, string lastName)
{
    var trimmedFirst = firstName?.Trim() ?? string.Empty;
    var trimmedLast = lastName?.Trim() ?? string.Empty;

    return $"{trimmedFirst} {trimmedLast}".Trim();
}
```

**Bad spacing:**
```csharp
public string GetFormattedName(string firstName,string lastName){
var trimmedFirst=firstName?.Trim()??string.Empty;
var trimmedLast=lastName?.Trim()??string.Empty;
return $"{trimmedFirst} {trimmedLast}".Trim();}
```

### Editor Configuration

**File:** `.editorconfig` (to be implemented)

This file will enforce:
- Character set (UTF-8)
- Line endings (LF)
- Indentation (4 spaces)
- Insert final newline
- Trim trailing whitespace
- Brace style
- Spacing rules

### Code Analysis & Warnings

#### Required Compiler Settings

In all `.csproj` files:

```xml
<PropertyGroup>
    <!-- Nullable reference types - Catch null reference issues at compile time -->
    <Nullable>enable</Nullable>
    
    <!-- Implicit usings - Reduce boilerplate imports -->
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Latest language features -->
    <LangVersion>latest</LangVersion>
    
    <!-- Treat warnings as errors in Release builds -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
</PropertyGroup>
```

#### Nullable Reference Type Rules

**Rule:** Use nullable annotations (`?`) to document intent

**Incorrect:**
```csharp
public string GetUserName(string userId)  // Ambiguous - is return value nullable?
{
    return null;  // Warning possible null
}
```

**Correct:**
```csharp
/// <summary>Gets the user name or null if not found.</summary>
public string? GetUserName(string userId)  // Clear - return can be null
{
    return null;  // No warning
}
```

**Non-nullable assertions:**
```csharp
// When you know a value is not null despite compiler warning
var id = userId!;  // Use ! operator for suppression (document why)
```

#### Suppressing Warnings

Only suppress warnings with documented justification:

```csharp
#pragma warning disable CS8618  // Non-nullable field is uninitialized
// Justified: This is initialized by dependency injection framework
private readonly ILogger _logger;
#pragma warning restore CS8618
```

## Const Visibility Rules

### CRITICAL RULE: No `public const` in Shared Libraries

The use of `public const` creates compile-time inlining issues that break consistency when libraries are updated without recompiling consuming projects.

### Const Declaration Guidelines

| Declaration Type | Scope | Allowed | Rationale |
|-----------------|-------|---------|-----------|
| `public const` | Public API | ❌ **FORBIDDEN** | Causes compile-time inlining; values not updated in consumers |
| `public static readonly` | Public API | ✅ **REQUIRED** | Runtime reference; always current value |
| `internal const` | Internal | ✅ **ALLOWED** | Safe for internal-only use; no external consumers |
| `private const` | Class-private | ✅ **ALLOWED** | Safe for private implementation details |

### Example: Correct Constant Declaration

```csharp
namespace MassifCentral.Lib;

public static class Constants
{
    // ✅ CORRECT: Public constants use static readonly
    /// <summary>
    /// Application version. Uses static readonly to prevent compile-time inlining.
    /// All consuming projects will always reference the current value.
    /// </summary>
    public static readonly string Version = "1.0.0";

    // ✅ CORRECT: Internal constants can use const
    /// <summary>
    /// Internal default timeout not exposed to public API.
    /// </summary>
    internal const int DefaultTimeoutMs = 5000;

    // ✅ CORRECT: Private constants use const
    private const string InternalFormat = "yyyy-MM-dd";
}
```

### Why This Matters

When a constant is declared as `public const`:

1. Compiler reads the value at build time
2. Compiler copies (inlines) the literal value into each consuming assembly
3. Consuming assemblies have hardcoded values, not references
4. When the library constant changes, consuming assemblies continue using old values
5. **Result:** Inconsistent behavior, broken version reporting, failed feature toggles

**For detailed analysis, see:** [CONST_VISIBILITY_ANALYSIS.md](./assessments/CONST_VISIBILITY_ANALYSIS.md)

## Testing Guidelines

### Unit Testing Approach

- **Framework:** xUnit with Arrange-Act-Assert (AAA) pattern
- **Naming:** `ComponentName_Behavior_Expected`
- **Coverage Goal:** >= 80% of public API
- **Isolation:** Tests are independent and repeatable

### Test Naming Convention

```csharp
[Fact]
public void Logger_LogInfo_WritesToConsole()
{
    // Test name pattern: Class_Method_ExpectedBehavior
}

[Theory]
[InlineData(5, 10, 15)]
[InlineData(0, 0, 0)]
public void Calculator_Add_ReturnsCorrectSum(int a, int b, int expected)
{
    // Parameterized tests use Theory
}
```

### Test Structure: Arrange-Act-Assert

```csharp
[Fact]
public void BaseEntity_InitializesWithDefaultValues()
{
    // Arrange - Setup test data and conditions
    // (No setup needed for this test)

    // Act - Execute the code being tested
    var entity = new TestEntity();

    // Assert - Verify the results
    Assert.NotEqual(Guid.Empty, entity.Id);
    Assert.True(entity.IsActive);
    Assert.NotEqual(DateTime.MinValue, entity.CreatedAt);
}
```

### Test Best Practices

1. **One Assertion Focus:** Each test should verify one behavior
2. **Clear Names:** Test names explain what they test and why
3. **Meaningful Messages:** Include custom assertion messages
4. **No Test Data Sharing:** Each test is independent
5. **Arrange-Act-Assert:** Follow AAA pattern consistently

```csharp
[Fact]
public void Entity_Deactivate_SetsIsActiveFalse()
{
    // Arrange
    var entity = new TestEntity { IsActive = true };

    // Act
    entity.Deactivate();

    // Assert
    Assert.False(
        entity.IsActive,
        "Entity should be inactive after calling Deactivate()");
}
```

## Code Review Checklist

### Pre-Submission Checklist

Before submitting a pull request or commit:

- [ ] Code compiles without warnings
- [ ] All public types have XML documentation
- [ ] Naming conventions are followed (PascalCase, camelCase, etc.)
- [ ] No `public const` declarations in shared libraries
- [ ] All new public code has unit tests
- [ ] Existing tests still pass
- [ ] Code follows formatting rules (spacing, line length, etc.)
- [ ] Nullable reference types properly annotated
- [ ] No `#pragma` suppressions without justification

### Code Review Checklist

When reviewing code:

- [ ] Check for any `public const` declarations → Convert to `public static readonly`
- [ ] Verify `internal const` is used only for internal values
- [ ] Ensure all public classes/methods have XML documentation
- [ ] Check naming conventions are consistent
- [ ] Verify test coverage for new functionality (>= 80%)
- [ ] Check that tests follow AAA pattern
- [ ] Ensure no compiler warnings
- [ ] Verify proper use of nullable reference types

## Anti-Patterns to Avoid

### ❌ Global State

```csharp
// BAD: Static mutable state
public static class AppState
{
    public static string CurrentUser;  // Mutable, thread-unsafe
}
```

### ❌ Magic Numbers

```csharp
// BAD: Unexplained magic number
if (user.LoginAttempts > 5)  // What does 5 mean?
{
    LockAccount();
}

// GOOD: Use named constant
internal const int MaxLoginAttempts = 5;
if (user.LoginAttempts > MaxLoginAttempts)
{
    LockAccount();
}
```

### ❌ Null Deference

```csharp
// BAD: No null checking
public void ProcessUser(User user)
{
    var name = user.Name.ToUpper();  // Exception if Name is null
}

// GOOD: Proper null handling
public void ProcessUser(User? user)
{
    if (user?.Name == null)
        return;
    
    var name = user.Name.ToUpper();  // Safe
}
```

### ❌ Catching Generic Exception

```csharp
// BAD: Too broad
try
{
    DoSomething();
}
catch (Exception ex)  // Catches everything including system failures
{
    Logger.LogError("Error", ex);
}

// GOOD: Catch specific exceptions
try
{
    DoSomething();
}
catch (ArgumentException ex)
{
    Logger.LogError("Invalid argument provided", ex);
}
catch (InvalidOperationException ex)
{
    Logger.LogError("Operation is invalid in current state", ex);
}
```

## Related Documents

- [Technologies Document](./TECHNOLOGIES.md) - Technology stack overview
- [Const Visibility Analysis](./assessments/CONST_VISIBILITY_ANALYSIS.md) - Detailed const inlining risks
- [Requirements Document](./REQUIREMENTS.md) - Project requirements
- [Design Document](./DESIGN.md) - Architecture and design patterns
