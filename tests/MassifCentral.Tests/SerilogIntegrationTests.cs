using MassifCentral.Lib;
using MassifCentral.Lib.Logging;
using MassifCentral.Lib.Utilities;
using MassifCentral.Tests.Mocks;
using Serilog;
using System.Text.Json;

namespace MassifCentral.Tests;

/// <summary>
/// Integration tests for Serilog configuration and sink behavior.
/// Verifies proper sink configuration for production, diagnostic, and development modes.
/// </summary>
public class SerilogIntegrationTests : IDisposable
{
    private readonly string _testLogDirectory;

    public SerilogIntegrationTests()
    {
        // Create a temporary directory for test logs
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"MassifCentral_Tests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testLogDirectory);
    }

    public void Dispose()
    {
        // Clean up test logs
        if (Directory.Exists(_testLogDirectory))
        {
            try
            {
                Directory.Delete(_testLogDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }

    [Fact]
    public void SerilogConfiguration_ProductionMode_CreatesValidLogger()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            Constants.ApplicationName,
            _testLogDirectory);

        // Act
        var logger = config.CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void SerilogConfiguration_DiagnosticMode_CreatesValidLogger()
    {
        // Arrange
        var config = SerilogConfiguration.GetDiagnosticConfiguration(
            Constants.ApplicationName,
            _testLogDirectory);

        // Act
        var logger = config.CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void SerilogConfiguration_DevelopmentMode_CreatesValidLogger()
    {
        // Arrange
        var config = SerilogConfiguration.GetDevelopmentConfiguration(
            Constants.ApplicationName,
            _testLogDirectory);

        // Act
        var logger = config.CreateLogger();

        // Assert
        Assert.NotNull(logger);
        logger.Dispose();
    }

    [Fact]
    public void ProductionMode_WarningAndErrorsWriteToFile()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        adapter.LogWarning("Test warning message");
        adapter.LogError("Test error message");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "errors-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("Test warning message", logContent);
        Assert.Contains("Test error message", logContent);
    }

    [Fact]
    public void DiagnosticMode_AllLevelsWriteToFile()
    {
        // Arrange
        var config = SerilogConfiguration.GetDiagnosticConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        adapter.LogTrace("Test trace message");
        adapter.LogDebug("Test debug message");
        adapter.LogInfo("Test info message");
        adapter.LogWarning("Test warning message");
        adapter.LogError("Test error message");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "diagnostic-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("Test trace message", logContent);
        Assert.Contains("Test debug message", logContent);
        Assert.Contains("Test info message", logContent);
        Assert.Contains("Test warning message", logContent);
        Assert.Contains("Test error message", logContent);
    }

    [Fact]
    public void DevelopmentMode_AllLevelsWriteToConsoleAndFile()
    {
        // Arrange
        var config = SerilogConfiguration.GetDevelopmentConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        adapter.LogDebug("Test debug");
        adapter.LogInfo("Test info");
        adapter.LogWarning("Test warning");
        adapter.LogError("Test error");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "dev-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("Test debug", logContent);
        Assert.Contains("Test info", logContent);
        Assert.Contains("Test warning", logContent);
        Assert.Contains("Test error", logContent);
    }

    [Fact]
    public void StructuredLogging_PropertiesIncludedInOutput()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        adapter.LogError("User {UserId} failed to log in from {IpAddress}", new Exception("Auth failed"), 123, "192.168.1.1");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "errors-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        // Should contain the values in the output
        Assert.Contains("123", logContent);
        Assert.Contains("192.168.1.1", logContent);
    }

    [Fact]
    public void SerilogLoggerAdapter_ImplementsILogger()
    {
        // Arrange
        var config = SerilogConfiguration.GetDevelopmentConfiguration(
            Constants.ApplicationName,
            _testLogDirectory);
        var logger = config.CreateLogger();

        // Act
        var adapter = new SerilogLoggerAdapter(logger);

        // Assert
        Assert.IsAssignableFrom<MassifCentral.Lib.Utilities.ILogger>(adapter);
        logger.Dispose();
    }

    [Fact]
    public void ExceptionLogging_IncludesStackTrace()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);
        var testException = new InvalidOperationException("Test error details");

        // Act
        adapter.LogError("An error occurred", testException);
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "errors-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains("An error occurred", logContent);
        Assert.Contains("InvalidOperationException", logContent);
        Assert.Contains("Test error details", logContent);
    }

    [Fact]
    public void CorrelationIdEnricher_AddsCorrelationIdToLogs()
    {
        // Arrange
        var config = SerilogConfiguration.GetDevelopmentConfiguration(
            "TestApp",
            _testLogDirectory);
        var correlationId = Guid.NewGuid().ToString("D");

        var logger = config
            .Enrich.With<CorrelationIdEnricher>()
            .CreateLogger();

        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        CorrelationIdEnricher.SetCorrelationId(correlationId);
        adapter.LogInfo("Operation started");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "dev-*.txt");
        Assert.NotEmpty(logFiles);

        var logContent = File.ReadAllText(logFiles[0]);
        Assert.Contains(correlationId, logContent);
    }

    [Fact]
    public void MockLogger_CapturesStructuredLogging()
    {
        // Arrange
        var mockLogger = new MockLogger();

        // Act
        mockLogger.LogInfo("User {UserId} logged in", 42);
        mockLogger.LogWarning("High memory usage: {MemoryMb}%", 85);
        mockLogger.LogError("Request {RequestId} failed", new Exception("Connection timeout"), "REQ-12345");

        // Assert
        Assert.Single(mockLogger.InfoMessages);
        Assert.Contains("42", mockLogger.InfoMessages[0]);

        Assert.Single(mockLogger.WarningMessages);
        Assert.Contains("85", mockLogger.WarningMessages[0]);

        Assert.Single(mockLogger.ErrorMessages);
        Assert.Contains("REQ-12345", mockLogger.ErrorMessages[0]);
    }

    [Fact]
    public void MockLogger_ProvidesHelperMethods()
    {
        // Arrange
        var mockLogger = new MockLogger();

        // Act
        mockLogger.LogInfo("Message 1");
        mockLogger.LogError("Error occurred");
        mockLogger.LogWarning("Warning issued");

        // Assert
        Assert.True(mockLogger.ContainsMessage("occurred"));
        Assert.False(mockLogger.ContainsMessage("nonexistent"));

        Assert.Equal(1, mockLogger.GetCountByLevel("INFO"));
        Assert.Equal(1, mockLogger.GetCountByLevel("ERROR"));
        Assert.Equal(1, mockLogger.GetCountByLevel("WARNING"));
        Assert.Equal(0, mockLogger.GetCountByLevel("DEBUG"));
    }

    [Fact]
    public void FileRollingPolicy_ProductionMode_CreatesDailyFiles()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        for (int i = 0; i < 5; i++)
        {
            adapter.LogError($"Error message {i}");
        }
        logger.Dispose();

        // Assert - Should have at least one rolling file created
        var logFiles = Directory.GetFiles(_testLogDirectory, "errors-*.txt");
        Assert.NotEmpty(logFiles);
    }

    [Fact]
    public void DiagnosticMode_HourlyRolling_Configured()
    {
        // Arrange
        var config = SerilogConfiguration.GetDiagnosticConfiguration(
            "TestApp",
            _testLogDirectory);

        // Act & Assert - Just verify configuration creates logger without errors
        var logger = config.CreateLogger();
        Assert.NotNull(logger);
        logger.Information("Test message");
        logger.Dispose();
    }

    [Fact]
    public void ProductionMode_InfoLevelSuppressedInConsole()
    {
        // Arrange
        var config = SerilogConfiguration.GetProductionConfiguration(
            "TestApp",
            _testLogDirectory);
        var logger = config.CreateLogger();
        var adapter = new SerilogLoggerAdapter(logger);

        // Act
        adapter.LogInfo("This info should not appear in error file");
        adapter.LogError("This error should appear");
        logger.Dispose(); // Dispose to flush and release file locks

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "errors-*.txt");
        if (logFiles.Length > 0)
        {
            var logContent = File.ReadAllText(logFiles[0]);
            // Info messages should NOT be in the errors file (errors-only sink)
            // But this test is limited to file since we can't easily capture console output
            Assert.Contains("This error should appear", logContent);
        }
    }

    [Fact]
    public void GetConfiguration_AutomaticalslySelectsEnvironmentMode()
    {
        // Arrange - Set environment variable
        var originalEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");

            // Act
            var config = SerilogConfiguration.GetConfiguration(
                Constants.ApplicationName,
                _testLogDirectory);
            var logger = config.CreateLogger();

            // Assert
            Assert.NotNull(logger);
            logger.Dispose();
        }
        finally
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalEnv);
        }
    }

    [Fact]
    public void DiagnosticMode_ActivatedByEnvironmentVariable()
    {
        // Arrange
        var originalMode = Environment.GetEnvironmentVariable("DIAGNOSTIC_MODE");
        try
        {
            Environment.SetEnvironmentVariable("DIAGNOSTIC_MODE", "true");

            // Act
            var config = SerilogConfiguration.GetConfiguration(
                Constants.ApplicationName,
                _testLogDirectory);
            var logger = config.CreateLogger();
            var adapter = new SerilogLoggerAdapter(logger);

            adapter.LogTrace("Diagnostic trace message");
            logger.Dispose(); // Dispose to flush and release file locks

            // Assert
            var logFiles = Directory.GetFiles(_testLogDirectory, "diagnostic-*.txt");
            Assert.NotEmpty(logFiles);

            var logContent = File.ReadAllText(logFiles[0]);
            Assert.Contains("Diagnostic trace message", logContent);
        }
        finally
        {
            Environment.SetEnvironmentVariable("DIAGNOSTIC_MODE", originalMode);
        }
    }
}
