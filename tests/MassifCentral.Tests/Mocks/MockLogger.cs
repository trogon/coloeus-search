using MassifCentral.Lib.Utilities;

namespace MassifCentral.Tests.Mocks;

/// <summary>
/// Mock implementation of ILogger for unit testing.
/// Captures all log messages for assertion and verification in tests.
/// </summary>
public class MockLogger : ILogger
{
    /// <summary>
    /// Gets the list of informational messages that were logged.
    /// </summary>
    public List<string> InfoMessages { get; } = new();

    /// <summary>
    /// Gets the list of warning messages that were logged.
    /// </summary>
    public List<string> WarningMessages { get; } = new();

    /// <summary>
    /// Gets the list of error messages that were logged.
    /// </summary>
    public List<string> ErrorMessages { get; } = new();

    /// <summary>
    /// Gets the list of exceptions that were logged.
    /// </summary>
    public List<Exception> LoggedExceptions { get; } = new();

    /// <summary>
    /// Logs an informational message and captures it.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogInfo(string message)
    {
        InfoMessages.Add(message);
    }

    /// <summary>
    /// Logs a warning message and captures it.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogWarning(string message)
    {
        WarningMessages.Add(message);
    }

    /// <summary>
    /// Logs an error message and captures it.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void LogError(string message)
    {
        ErrorMessages.Add(message);
    }

    /// <summary>
    /// Logs an error message with exception and captures both.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    public void LogError(string message, Exception exception)
    {
        ErrorMessages.Add(message);
        LoggedExceptions.Add(exception);
    }

    /// <summary>
    /// Clears all captured log messages and exceptions.
    /// Useful for resetting state between test assertions.
    /// </summary>
    public void Clear()
    {
        InfoMessages.Clear();
        WarningMessages.Clear();
        ErrorMessages.Clear();
        LoggedExceptions.Clear();
    }
}
