namespace MassifCentral.Lib.Utilities;

/// <summary>
/// Interface for logging operations across the application.
/// Enables dependency injection, testability, and structured logging through abstraction.
/// Supports simple and structured logging with properties for enhanced searchability and traceability.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs an informational message with structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders (e.g., "User {UserId} logged in").</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogInfo(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs a warning message with structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogWarning(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogError(string message);

    /// <summary>
    /// Logs an error message with structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogError(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    void LogError(string message, Exception exception);

    /// <summary>
    /// Logs an error message with exception and structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogError(string messageTemplate, Exception exception, params object[] propertyValues);

    /// <summary>
    /// Logs a debug message for development and diagnostics.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs a debug message with structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogDebug(string messageTemplate, params object[] propertyValues);

    /// <summary>
    /// Logs a trace (verbose) message for detailed diagnostics.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void LogTrace(string message);

    /// <summary>
    /// Logs a trace (verbose) message with structured properties.
    /// </summary>
    /// <param name="messageTemplate">Message template with named placeholders.</param>
    /// <param name="propertyValues">Values to substitute into the template.</param>
    void LogTrace(string messageTemplate, params object[] propertyValues);
}
