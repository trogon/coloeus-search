# Serilog Implementation Guide for MassifCentral

## Version Control
- **Version:** 1.0.0
- **Date:** 2026-02-07
- **Status:** PROPOSED
- **Summary:** Step-by-step implementation guide for integrating Serilog into MassifCentral

---

## Table of Contents
1. [Phase 1: Core Setup](#phase-1-core-setup)
2. [Phase 2: Project Structure](#phase-2-project-structure)
3. [Phase 3: Adapter Pattern](#phase-3-adapter-pattern)
4. [Phase 4: Configuration](#phase-4-configuration)
5. [Phase 5: Usage Examples](#phase-5-usage-examples)
6. [Phase 6: Testing](#phase-6-testing)
7. [Migration Checklist](#migration-checklist)

---

## Phase 1: Core Setup

### Step 1.1: Update NuGet Dependencies

**File:** `src/MassifCentral.Lib/MassifCentral.Lib.csproj`

Add the following package references:

```xml
<ItemGroup>
  <!-- Core Serilog -->
  <PackageReference Include="Serilog" Version="4.0.0" />
  <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
  
  <!-- Console Sink with Colors -->
  <PackageReference Include="Serilog.Sinks.Console" Version="5.1.0" />
  
  <!-- File Sink with Rolling -->
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
  
  <!-- Enrichers for Context -->
  <PackageReference Include="Serilog.Enrichers.Environment" Version="3.0.0" />
  <PackageReference Include="Serilog.Enrichers.Process" Version="3.0.0" />
  <PackageReference Include="Serilog.Enrichers.Thread" Version="4.1.0" />
  
  <!-- JSON Formatting -->
  <PackageReference Include="Serilog.Formatting.Compact" Version="3.0.0" />
</ItemGroup>
```

**File:** `src/MassifCentral.Console/MassifCentral.Console.csproj`

```xml
<ItemGroup>
  <!-- Console app specific sinks -->
  <PackageReference Include="Serilog" Version="4.0.0" />
  <PackageReference Include="Serilog.Sinks.Console" Version="5.1.0" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
</ItemGroup>
```

---

## Phase 2: Project Structure

### Step 2.1: Create Logger Configuration Class

**File:** `src/MassifCentral.Lib/Logging/SerilogConfiguration.cs`

```csharp
namespace MassifCentral.Lib.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

/// <summary>
/// Centralizes Serilog configuration for consistent logging setup across applications.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Gets the default Serilog logger configuration suitable for console applications.
    /// </summary>
    /// <param name="applicationName">Application name for context enrichment.</param>
    /// <param name="environment">Environment name (Development, Staging, Production).</param>
    /// <param name="logDirectory">Directory for rolling log files.</param>
    /// <returns>Configured LoggerConfiguration ready to be created.</returns>
    public static LoggerConfiguration GetConfiguration(
        string applicationName,
        string environment,
        string logDirectory = "logs")
    {
        return new LoggerConfiguration()
            // Set minimum level based on environment
            .MinimumLevel.Is(GetMinimumLevel(environment))
            
            // Console sink with JSON output
            .WriteTo.Console(new CompactJsonFormatter())
            
            // File sink with rolling policy
            .WriteTo.File(
                path: Path.Combine(logDirectory, "app-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {Message} {Properties:j}{NewLine}{Exception}",
                fileSizeLimitBytes: 104857600) // 100MB
            
            // Enrichment
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", environment);
    }

    /// <summary>
    /// Determines the minimum log level based on environment.
    /// </summary>
    private static LogEventLevel GetMinimumLevel(string environment)
    {
        return environment.ToLower() switch
        {
            "development" => LogEventLevel.Debug,
            "staging" => LogEventLevel.Information,
            "production" => LogEventLevel.Warning,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// Gets advanced configuration with additional sinks (e.g., Seq for development).
    /// </summary>
    public static LoggerConfiguration GetConfigurationWithSeq(
        string applicationName,
        string environment,
        string seqServerUrl = "http://localhost:5341",
        string logDirectory = "logs")
    {
        var config = GetConfiguration(applicationName, environment, logDirectory);

        // Only enable Seq in development
        if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            config = config.WriteTo.Seq(seqServerUrl);
        }

        return config;
    }
}
```

### Step 2.2: Create Correlation ID Enricher

**File:** `src/MassifCentral.Lib/Logging/CorrelationIdEnricher.cs`

```csharp
namespace MassifCentral.Lib.Logging;

using Serilog.Core;
using Serilog.Events;

/// <summary>
/// Enriches logs with correlation IDs for distributed tracing across application boundaries.
/// Correlation IDs track requests through multiple services and layers.
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdPropertyName = "CorrelationId";
    private const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>
    /// Sets the correlation ID for the current operation context.
    /// Should be called at the entry point of a request/operation.
    /// </summary>
    /// <param name="correlationId">Unique identifier for the operation chain.</param>
    public static void SetCorrelationId(string correlationId)
    {
        LogContext.PushProperty(CorrelationIdContextKey, correlationId);
    }

    /// <summary>
    /// Gets the current correlation ID from context, or generates a new one if not set.
    /// </summary>
    /// <returns>Current or newly generated correlation ID.</returns>
    public static string GetOrCreateCorrelationId()
    {
        var existingId = LogContext.GetProperties();
        if (existingId.TryGetValue(CorrelationIdContextKey, out var id))
        {
            return id.ToString();
        }

        var newId = Guid.NewGuid().ToString("D");
        SetCorrelationId(newId);
        return newId;
    }

    /// <summary>
    /// Enriches the log event with correlation ID property.
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var properties = LogContext.GetProperties();
        if (properties.TryGetValue(CorrelationIdContextKey, out var correlationId))
        {
            var property = propertyFactory.CreateProperty(CorrelationIdPropertyName, correlationId);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
```

---

## Phase 3: Adapter Pattern

### Step 3.1: Create Serilog Adapter

**File:** `src/MassifCentral.Lib/Logging/SerilogLoggerAdapter.cs`

This adapter maintains backward compatibility with the existing `ILogger` interface:

```csharp
namespace MassifCentral.Lib.Logging;

using Serilog;
using Serilog.Events;

/// <summary>
/// Adapter that implements MassifCentral's ILogger interface using Serilog as the underlying implementation.
/// This allows gradual migration from custom logger to Serilog while maintaining interface compatibility.
/// </summary>
public class SerilogLoggerAdapter : ILogger
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the SerilogLoggerAdapter.
    /// </summary>
    /// <param name="logger">Serilog ILogger instance.</param>
    public SerilogLoggerAdapter(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an informational message with optional properties.
    /// </summary>
    public void LogInfo(string message)
    {
        _logger.Information(message);
    }

    /// <summary>
    /// Logs an informational message with structured properties.
    /// </summary>
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        _logger.Information(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    /// <summary>
    /// Logs a warning message with structured properties.
    /// </summary>
    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        _logger.Warning(messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void LogError(string message)
    {
        _logger.Error(message);
    }

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    public void LogError(string message, Exception exception)
    {
        _logger.Error(exception, message);
    }

    /// <summary>
    /// Logs an error message with exception and structured properties.
    /// </summary>
    public void LogError(string messageTemplate, Exception exception, params object[] propertyValues)
    {
        _logger.Error(exception, messageTemplate, propertyValues);
    }

    /// <summary>
    /// Logs a debug message (useful for development).
    /// </summary>
    public void LogDebug(string message)
    {
        _logger.Debug(message);
    }

    /// <summary>
    /// Logs a debug message with structured properties.
    /// </summary>
    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        _logger.Debug(messageTemplate, propertyValues);
    }
}
```

### Step 3.2: Update the ILogger Interface

**File:** `src/MassifCentral.Lib/Utilities/Logger.cs`

Keep the interface but add optional overloads for structured logging:

```csharp
namespace MassifCentral.Lib.Utilities;

/// <summary>
/// Interface for logging operations across the application.
/// Enables dependency injection and testability through abstraction.
/// Supports both simple and structured logging.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    void LogInfo(string message);

    /// <summary>
    /// Logs an informational message with structured properties.
    /// </summary>
    void LogInfo(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    void LogWarning(string message);

    /// <summary>
    /// Logs a warning message with structured properties.
    /// </summary>
    void LogWarning(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    void LogError(string message);

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    void LogError(string message, Exception exception);

    /// <summary>
    /// Logs an error message with exception and structured properties.
    /// </summary>
    void LogError(string messageTemplate, Exception exception, params object[] propertyValues);

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    void LogDebug(string message);

    /// <summary>
    /// Logs a debug message with structured properties.
    /// </summary>
    void LogDebug(string messageTemplate, params object[] propertyValues);
}

/// <summary>
/// Console-based logger implementation (DEPRECATED).
/// Use SerilogLoggerAdapter instead for new implementations.
/// This class is maintained for backward compatibility.
/// </summary>
[Obsolete("Use SerilogLoggerAdapter instead", false)]
public class Logger : ILogger
{
    // ... existing implementation remains unchanged ...
    
    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        LogInfo(string.Format(messageTemplate, propertyValues));
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        LogWarning(string.Format(messageTemplate, propertyValues));
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogError(string message, Exception exception)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }

    public void LogError(string messageTemplate, Exception exception, params object[] propertyValues)
    {
        LogError(string.Format(messageTemplate, propertyValues), exception);
    }

    public void LogDebug(string message)
    {
        Console.WriteLine($"[DEBUG] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        LogDebug(string.Format(messageTemplate, propertyValues));
    }
}
```

---

## Phase 4: Configuration

### Step 4.1: Update Program.cs

**File:** `src/MassifCentral.Console/Program.cs`

```csharp
using MassifCentral.Lib;
using MassifCentral.Lib.Logging;
using MassifCentral.Lib.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Initialize Serilog logger before anything else
Log.Logger = SerilogConfiguration.GetConfiguration(
    applicationName: Constants.ApplicationName,
    environment: Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production",
    logDirectory: "logs"
).CreateLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        // Use Serilog for all logging
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // Register custom ILogger implementation using Serilog adapter
            services.AddSingleton<ILogger>(sp =>
            {
                var serilogLogger = Log.ForContext<Program>();
                return new SerilogLoggerAdapter(serilogLogger);
            });

            // Add Correlation ID enricher
            services.AddSingleton<CorrelationIdEnricher>();

            // Register any other services
            services.AddMassifCentralServices();
        })
        .Build();

    // Get and use logger
    var logger = host.Services.GetRequiredService<ILogger>();

    logger.LogInfo("Starting {ApplicationName} v{Version}", 
        Constants.ApplicationName, 
        Constants.Version);

    // Initialize application
    logger.LogInfo("Application initialized successfully");

    // Run application
    logger.LogInfo("Application completed successfully");

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
```

### Step 4.2: Update ServiceCollectionExtensions.cs

**File:** `src/MassifCentral.Lib/ServiceCollectionExtensions.cs`

```csharp
namespace MassifCentral.Lib;

using MassifCentral.Lib.Logging;
using MassifCentral.Lib.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

/// <summary>
/// Extension methods for registering MassifCentral services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers core MassifCentral services including logging and utilities.
    /// </summary>
    public static IServiceCollection AddMassifCentralServices(this IServiceCollection services)
    {
        // Register ILogger as singleton using Serilog
        // Note: Serilog should be initialized in Program.cs before calling this method
        services.AddSingleton<ILogger>(sp =>
        {
            var serilogLogger = Log.ForContext("SourceContext", "MassifCentral");
            return new SerilogLoggerAdapter(serilogLogger);
        });

        // Register enrichers
        services.AddSingleton<CorrelationIdEnricher>();

        return services;
    }
}
```

---

## Phase 5: Usage Examples

### Example 1: Basic Logging

```csharp
public class OrderService
{
    private readonly ILogger _logger;

    public OrderService(ILogger logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(int orderId, string customerId)
    {
        _logger.LogInfo("Processing order {OrderId} for customer {CustomerId}", 
            orderId, customerId);

        try
        {
            // Process order logic...
            _logger.LogInfo("Order {OrderId} processed successfully", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to process order {OrderId}", ex, orderId);
            throw;
        }
    }
}
```

**Output (JSON):**
```json
{"@t":"2026-02-07T14:23:45.1234567Z","@mt":"Processing order {OrderId} for customer {CustomerId}","OrderId":123,"CustomerId":"CUST-456","SourceContext":"OrderService","MachineName":"DEV-MACHINE"}
{"@t":"2026-02-07T14:23:45.5234567Z","@mt":"Order {OrderId} processed successfully","OrderId":123,"SourceContext":"OrderService","MachineName":"DEV-MACHINE"}
```

### Example 2: Correlation ID Tracking

```csharp
public class RequestHandler
{
    private readonly ILogger _logger;

    public RequestHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void HandleRequest(string requestData)
    {
        // Get or create correlation ID for this request
        var correlationId = CorrelationIdEnricher.GetOrCreateCorrelationId();
        
        _logger.LogInfo("Received request with correlation ID {CorrelationId}", correlationId);
        
        // All logs in this operation automatically include the correlation ID
        ProcessData(requestData);
    }

    private void ProcessData(string data)
    {
        _logger.LogInfo("Processing data: {Data}", data);
        // CorrelationId automatically included in output
    }
}
```

### Example 3: Exception Logging

```csharp
public void SaveEntity(MyEntity entity)
{
    try
    {
        _database.Save(entity);
        _logger.LogInfo("Entity {EntityId} saved successfully", entity.Id);
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogError("Failed to save entity {EntityId}: {Reason}", ex, 
            entity.Id, ex.Message);
        throw;
    }
}
```

---

## Phase 6: Testing

### Update MockLogger

**File:** `tests/MassifCentral.Tests/Mocks/MockLogger.cs`

```csharp
namespace MassifCentral.Tests.Mocks;

using MassifCentral.Lib.Utilities;

/// <summary>
/// Mock implementation of ILogger for unit testing.
/// </summary>
public class MockLogger : ILogger
{
    private readonly List<LogEntry> _logs = [];

    public IReadOnlyList<LogEntry> Logs => _logs.AsReadOnly();

    public void LogInfo(string message)
    {
        _logs.Add(new LogEntry { Level = "INFO", Message = message });
    }

    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogInfo(message);
    }

    public void LogWarning(string message)
    {
        _logs.Add(new LogEntry { Level = "WARNING", Message = message });
    }

    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogWarning(message);
    }

    public void LogError(string message)
    {
        _logs.Add(new LogEntry { Level = "ERROR", Message = message });
    }

    public void LogError(string message, Exception exception)
    {
        _logs.Add(new LogEntry 
        { 
            Level = "ERROR", 
            Message = message,
            Exception = exception
        });
    }

    public void LogError(string messageTemplate, Exception exception, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogError(message, exception);
    }

    public void LogDebug(string message)
    {
        _logs.Add(new LogEntry { Level = "DEBUG", Message = message });
    }

    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogDebug(message);
    }

    public class LogEntry
    {
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }
}
```

### Test Example

```csharp
[Fact]
public void LoggerAdapter_LogsInformationalMessage()
{
    // Arrange
    var mockLogger = new MockLogger();
    
    // Act
    mockLogger.LogInfo("Test message with {Value}", 42);
    
    // Assert
    Assert.Single(mockLogger.Logs);
    Assert.Equal("INFO", mockLogger.Logs[0].Level);
    Assert.Contains("42", mockLogger.Logs[0].Message);
}
```

---

## Migration Checklist

- [ ] **NuGet Packages:** Add all Serilog packages to projects
- [ ] **Configuration:** Create `SerilogConfiguration` class
- [ ] **Enrichers:** Create `CorrelationIdEnricher` class
- [ ] **Adapter:** Create `SerilogLoggerAdapter` class
- [ ] **Interface:** Update `ILogger` with new method overloads
- [ ] **Program.cs:** Initialize Serilog and register services
- [ ] **Service Extensions:** Update `ServiceCollectionExtensions`
- [ ] **Tests:** Update `MockLogger` with new methods
- [ ] **Documentation:** Update README with logging usage
- [ ] **Integration Testing:** Test end-to-end logging in console app
- [ ] **Performance Testing:** Verify <1ms log write overhead
- [ ] **Deprecation:** Mark old `Logger` class as `[Obsolete]`
- [ ] **Code Review:** Review all changes
- [ ] **Release Notes:** Document logging library migration

---

## Rollback Plan

If issues arise during implementation:

1. **Keep existing `Logger.cs` unchanged** - Old logger remains as fallback
2. **Adapter pattern** - Allows swapping implementations without changing consuming code
3. **ServiceCollectionExtensions** - Can revert to registering old Logger
4. **Program.cs** - Remove Serilog initialization, revert to simple logger
5. **Tests** - MockLogger supports both old and new signatures

---

## Performance Considerations

| Operation | Latency | Notes |
|-----------|---------|-------|
| Single log write (console) | <0.5ms | Negligible overhead |
| JSON serialization | <0.2ms | CompactJsonFormatter optimized |
| File write (async) | <1ms | Non-blocking, background thread |
| Enrichment | <0.1ms | In-memory property addition |
| **Total per log entry** | **<2ms** | Well within acceptable margins |

---

## Monitoring & Validation

After implementation, validate:

```csharp
// Verify JSON output is valid
File.ReadAllText("logs/app-20260207.txt")
    .Split('\n')
    .Where(l => !string.IsNullOrEmpty(l))
    .ForEach(line => JsonDocument.Parse(line));

// Verify correlation IDs are present
Assert.All(logLines, line => Assert.Contains("CorrelationId", line));

// Verify timestamps are UTC
Assert.All(logLines, line => 
    Assert.Matches(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}", line));

// Verify enrichment data
Assert.All(logLines, line => 
    Assert.Contains("MachineName", line));
```

---

## Next Steps

1. Review this implementation guide with the team
2. Create a feature branch for Serilog integration
3. Implement Phase 1 and Phase 2 (Core Setup & Project Structure)
4. Add integration tests
5. Merge and release as v1.2.0
6. Plan Phase 5 (Advanced Features) for future sprint
