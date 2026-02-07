namespace MassifCentral.Lib.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;

/// <summary>
/// Centralizes Serilog configuration with environment-specific sink strategies.
/// Production: Console errors only + rolling file for errors/warnings
/// Diagnostic: Single file with 6-hour retention containing all levels (Trace through Error)
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Gets the production Serilog configuration.
    /// Sinks: Console (errors only) + Rolling file (errors and warnings)
    /// </summary>
    /// <param name="applicationName">Application name for enrichment.</param>
    /// <param name="logDirectory">Directory for log files.</param>
    /// <returns>Configured LoggerConfiguration ready to create logger.</returns>
    public static LoggerConfiguration GetProductionConfiguration(
        string applicationName,
        string logDirectory = "logs")
    {
        return new LoggerConfiguration()
            // Production: Informational minimum level globally
            .MinimumLevel.Information()
            
            // Console sink: ERRORS ONLY
            .WriteTo.Logger(lc => lc
                .MinimumLevel.Error()
                .WriteTo.Console(new CompactJsonFormatter()))
            
            // File sink: ERRORS AND WARNINGS (Rolling daily)
            .WriteTo.Logger(lc => lc
                .MinimumLevel.Warning()
                .WriteTo.File(
                    path: Path.Combine(logDirectory, "errors-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {Message} {Properties:j}{NewLine}{Exception}",
                    fileSizeLimitBytes: 104857600)) // 100MB
            
            // Enrichers for all sinks
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", "Production");
    }

    /// <summary>
    /// Gets the diagnostic/debug Serilog configuration.
    /// Sinks: Single file containing all log levels (Trace through Error) with 6-hour rolling window
    /// </summary>
    /// <param name="applicationName">Application name for enrichment.</param>
    /// <param name="logDirectory">Directory for log files.</param>
    /// <returns>Configured LoggerConfiguration ready to create logger.</returns>
    public static LoggerConfiguration GetDiagnosticConfiguration(
        string applicationName,
        string logDirectory = "logs")
    {
        return new LoggerConfiguration()
            // Diagnostic: Verbose (Trace) minimum level to capture everything
            .MinimumLevel.Verbose()
            
            // Single file sink: All levels with 6-hour rolling window
            .WriteTo.File(
                path: Path.Combine(logDirectory, "diagnostic-.txt"),
                rollingInterval: RollingInterval.Hour,
                retainedFileCountLimit: 6, // 6-hour window
                outputTemplate: "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {Message} {Properties:j}{NewLine}{Exception}",
                fileSizeLimitBytes: 52428800) // 50MB per hour
            
            // Enrichers
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", "Diagnostic");
    }

    /// <summary>
    /// Gets the development Serilog configuration (colorized console output).
    /// Sinks: Console (all levels with colors) + Rolling file
    /// </summary>
    /// <param name="applicationName">Application name for enrichment.</param>
    /// <param name="logDirectory">Directory for log files.</param>
    /// <returns>Configured LoggerConfiguration ready to create logger.</returns>
    public static LoggerConfiguration GetDevelopmentConfiguration(
        string applicationName,
        string logDirectory = "logs")
    {
        return new LoggerConfiguration()
            // Development: Debug minimum level
            .MinimumLevel.Debug()
            
            // Console sink: Colorized, all levels
            .WriteTo.Console(new CompactJsonFormatter())
            
            // File sink: All levels with rolling
            .WriteTo.File(
                path: Path.Combine(logDirectory, "dev-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {Message} {Properties:j}{NewLine}{Exception}",
                fileSizeLimitBytes: 52428800) // 50MB
            
            // Enrichers
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", applicationName)
            .Enrich.WithProperty("Environment", "Development");
    }

    /// <summary>
    /// Gets appropriate Serilog configuration based on environment variable.
    /// </summary>
    /// <param name="applicationName">Application name for enrichment.</param>
    /// <param name="logDirectory">Directory for log files.</param>
    /// <returns>Environment-appropriate LoggerConfiguration.</returns>
    public static LoggerConfiguration GetConfiguration(
        string applicationName,
        string logDirectory = "logs")
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        var diagnosticMode = Environment.GetEnvironmentVariable("DIAGNOSTIC_MODE") ?? "false";

        if (diagnosticMode.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return GetDiagnosticConfiguration(applicationName, logDirectory);
        }

        return environment.ToLower() switch
        {
            "development" => GetDevelopmentConfiguration(applicationName, logDirectory),
            "staging" => GetProductionConfiguration(applicationName, logDirectory),
            "production" => GetProductionConfiguration(applicationName, logDirectory),
            _ => GetProductionConfiguration(applicationName, logDirectory)
        };
    }
}
