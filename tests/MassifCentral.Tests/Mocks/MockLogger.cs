using MassifCentral.Lib.Utilities;

namespace MassifCentral.Tests.Mocks;

/// <summary>
/// Mock implementation of ILogger for unit testing.
/// Captures all log messages and metadata for assertion and verification in tests.
/// Supports structured logging with categorization by level and properties.
/// </summary>
public class MockLogger : ILogger
{
    /// <summary>
    /// Represents a captured log entry with metadata.
    /// </summary>
    public class LogEntry
    {
        public required string Level { get; init; }
        public required string Message { get; init; }
        public Exception? Exception { get; init; }
    }

    /// <summary>
    /// Gets the list of all log entries captured.
    /// </summary>
    public List<LogEntry> AllLogs { get; } = new();

    /// <summary>
    /// Gets the list of informational messages that were logged.
    /// </summary>
    public List<string> InfoMessages => AllLogs
        .Where(l => l.Level == "INFO")
        .Select(l => l.Message)
        .ToList();

    /// <summary>
    /// Gets the list of debug messages that were logged.
    /// </summary>
    public List<string> DebugMessages => AllLogs
        .Where(l => l.Level == "DEBUG")
        .Select(l => l.Message)
        .ToList();

    /// <summary>
    /// Gets the list of trace messages that were logged.
    /// </summary>
    public List<string> TraceMessages => AllLogs
        .Where(l => l.Level == "TRACE")
        .Select(l => l.Message)
        .ToList();

    /// <summary>
    /// Gets the list of warning messages that were logged.
    /// </summary>
    public List<string> WarningMessages => AllLogs
        .Where(l => l.Level == "WARNING")
        .Select(l => l.Message)
        .ToList();

    /// <summary>
    /// Gets the list of error messages that were logged.
    /// </summary>
    public List<string> ErrorMessages => AllLogs
        .Where(l => l.Level == "ERROR")
        .Select(l => l.Message)
        .ToList();

    /// <summary>
    /// Gets the list of exceptions that were logged with errors.
    /// </summary>
    public List<Exception> LoggedExceptions => AllLogs
        .Where(l => l.Exception != null)
        .Select(l => l.Exception!)
        .ToList();

    /// <summary>
    /// Logs an informational message and captures it.
    /// </summary>
    public void LogInfo(string message)
    {
        AllLogs.Add(new LogEntry { Level = "INFO", Message = message });
    }

    /// <summary>
    /// Logs an informational message with structured properties.
    /// </summary>
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        // For structured logging templates like "User {UserId} logged in"
        // We'll replace {PropertyName} with the corresponding value
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            // Replace {i} (numeric placeholder) for format string compatibility
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        // Also handle named properties like {UserId} by replacing with the values
        // This is a simplified approach - just append values to show they were present
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogInfo(message);
    }

    /// <summary>
    /// Logs a debug message and captures it.
    /// </summary>
    public void LogDebug(string message)
    {
        AllLogs.Add(new LogEntry { Level = "DEBUG", Message = message });
    }

    /// <summary>
    /// Logs a debug message with structured properties.
    /// </summary>
    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogDebug(message);
    }

    /// <summary>
    /// Logs a trace message and captures it.
    /// </summary>
    public void LogTrace(string message)
    {
        AllLogs.Add(new LogEntry { Level = "TRACE", Message = message });
    }

    /// <summary>
    /// Logs a trace message with structured properties.
    /// </summary>
    public void LogTrace(string messageTemplate, params object[] propertyValues)
    {
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogTrace(message);
    }

    /// <summary>
    /// Logs a warning message and captures it.
    /// </summary>
    public void LogWarning(string message)
    {
        AllLogs.Add(new LogEntry { Level = "WARNING", Message = message });
    }

    /// <summary>
    /// Logs a warning message with structured properties.
    /// </summary>
    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogWarning(message);
    }

    /// <summary>
    /// Logs an error message and captures it.
    /// </summary>
    public void LogError(string message)
    {
        AllLogs.Add(new LogEntry { Level = "ERROR", Message = message });
    }

    /// <summary>
    /// Logs an error message with structured properties.
    /// </summary>
    public void LogError(string messageTemplate, params object[] propertyValues)
    {
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogError(message);
    }

    /// <summary>
    /// Logs an error message with an exception and captures both.
    /// </summary>
    public void LogError(string message, Exception exception)
    {
        AllLogs.Add(new LogEntry 
        { 
            Level = "ERROR", 
            Message = message,
            Exception = exception
        });
    }

    /// <summary>
    /// Logs an error message with exception and structured properties.
    /// </summary>
    public void LogError(string messageTemplate, Exception exception, params object[] propertyValues)
    {
        var message = messageTemplate;
        for (int i = 0; i < propertyValues.Length; i++)
        {
            message = message.Replace("{" + i + "}", propertyValues[i]?.ToString() ?? "null");
        }
        foreach (var value in propertyValues)
        {
            if (!message.Contains(value?.ToString() ?? "null"))
            {
                message += " " + (value?.ToString() ?? "null");
            }
        }
        LogError(message, exception);
    }

    /// <summary>
    /// Clears all captured log messages and exceptions.
    /// Useful for resetting state between test assertions.
    /// </summary>
    public void Clear()
    {
        AllLogs.Clear();
    }

    /// <summary>
    /// Checks if a log message contains the specified text.
    /// Useful for assertion in tests.
    /// </summary>
    /// <param name="text">Text to search for in log messages.</param>
    /// <returns>True if any log contains the text; otherwise, false.</returns>
    public bool ContainsMessage(string text) => 
        AllLogs.Any(l => l.Message.Contains(text, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets count of logs at a specific level.
    /// </summary>
    /// <param name="level">Log level to count (e.g., "ERROR", "WARNING", "INFO").</param>
    /// <returns>Count of logs at the specified level.</returns>
    public int GetCountByLevel(string level) => 
        AllLogs.Count(l => l.Level == level);
}

