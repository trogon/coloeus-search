namespace MassifCentral.Lib.Logging;

using MassifCentral.Lib.Utilities;
using Serilog;

/// <summary>
/// Adapter that implements MassifCentral's ILogger interface using Serilog as the underlying implementation.
/// This enables gradual migration from the custom logger to Serilog while maintaining interface compatibility.
/// </summary>
public class SerilogLoggerAdapter : Utilities.ILogger
{
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the SerilogLoggerAdapter.
    /// </summary>
    /// <param name="logger">Serilog ILogger instance.</param>
    public SerilogLoggerAdapter(Serilog.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an informational message.
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
    /// Logs an error message with structured properties.
    /// </summary>
    public void LogError(string messageTemplate, params object[] propertyValues)
    {
        _logger.Error(messageTemplate, propertyValues);
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
    /// Logs a debug message (useful for development and diagnostics).
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

    /// <summary>
    /// Logs a verbose/trace message (useful for detailed diagnostics).
    /// </summary>
    public void LogTrace(string message)
    {
        _logger.Verbose(message);
    }

    /// <summary>
    /// Logs a verbose/trace message with structured properties.
    /// </summary>
    public void LogTrace(string messageTemplate, params object[] propertyValues)
    {
        _logger.Verbose(messageTemplate, propertyValues);
    }
}
