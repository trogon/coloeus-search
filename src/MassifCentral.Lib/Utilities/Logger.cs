namespace MassifCentral.Lib.Utilities;

/// <summary>
/// Provides simple logging functionality for the application.
/// This is a basic implementation that can be replaced with a more sophisticated logging framework.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
    }

    /// <summary>
    /// Logs an error message with an exception.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log.</param>
    public static void LogError(string message, Exception exception)
    {
        Console.WriteLine($"[ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        Console.WriteLine($"Exception: {exception.Message}");
        Console.WriteLine($"StackTrace: {exception.StackTrace}");
    }
}
