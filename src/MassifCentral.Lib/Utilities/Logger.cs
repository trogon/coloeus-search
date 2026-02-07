namespace MassifCentral.Lib.Utilities;

/// <summary>
/// Console-based logger implementation providing simple logging functionality.
/// DEPRECATED: Use SerilogLoggerAdapter instead for new implementations.
/// This class is maintained for backward compatibility only.
/// </summary>
[Obsolete("Use SerilogLoggerAdapter from MassifCentral.Lib.Logging instead", false)]
public class Logger : ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs an informational message with structured properties.
    /// </summary>
    public void LogInfo(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogInfo(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs a warning message with structured properties.
    /// </summary>
    public void LogWarning(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogWarning(message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs an error message with structured properties.
    /// </summary>
    public void LogError(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogError(message);
    }

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    public void LogError(string message, Exception exception)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }

    /// <summary>
    /// Logs an error message with exception and structured properties.
    /// </summary>
    public void LogError(string messageTemplate, Exception exception, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogError(message, exception);
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    public void LogDebug(string message)
    {
        Console.WriteLine($"[DEBUG] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs a debug message with structured properties.
    /// </summary>
    public void LogDebug(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogDebug(message);
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    public void LogTrace(string message)
    {
        Console.WriteLine($"[TRACE] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs a trace message with structured properties.
    /// </summary>
    public void LogTrace(string messageTemplate, params object[] propertyValues)
    {
        var message = string.Format(messageTemplate, propertyValues);
        LogTrace(message);
    }
}
