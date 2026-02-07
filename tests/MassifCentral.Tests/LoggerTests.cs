using MassifCentral.Lib.Utilities;
using MassifCentral.Tests.Mocks;

namespace MassifCentral.Tests;

/// <summary>
/// Unit tests for the Logger implementation and ILogger interface.
/// </summary>
public class LoggerTests
{
    [Fact]
    public void Logger_ImplementsILogger()
    {
        // Arrange & Act
        var logger = new Logger();

        // Assert
        Assert.IsAssignableFrom<ILogger>(logger);
    }

    [Fact]
    public void MockLogger_CapturesInfoMessages()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var testMessage = "Test info message";

        // Act
        mockLogger.LogInfo(testMessage);

        // Assert
        Assert.Single(mockLogger.InfoMessages);
        Assert.Equal(testMessage, mockLogger.InfoMessages[0]);
    }

    [Fact]
    public void MockLogger_CapturesWarningMessages()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var testMessage = "Test warning message";

        // Act
        mockLogger.LogWarning(testMessage);

        // Assert
        Assert.Single(mockLogger.WarningMessages);
        Assert.Equal(testMessage, mockLogger.WarningMessages[0]);
    }

    [Fact]
    public void MockLogger_CapturesErrorMessages()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var testMessage = "Test error message";

        // Act
        mockLogger.LogError(testMessage);

        // Assert
        Assert.Single(mockLogger.ErrorMessages);
        Assert.Equal(testMessage, mockLogger.ErrorMessages[0]);
    }

    [Fact]
    public void MockLogger_CapturesErrorMessagesWithException()
    {
        // Arrange
        var mockLogger = new MockLogger();
        var testMessage = "Test error with exception";
        var testException = new InvalidOperationException("Test exception");

        // Act
        mockLogger.LogError(testMessage, testException);

        // Assert
        Assert.Single(mockLogger.ErrorMessages);
        Assert.Single(mockLogger.LoggedExceptions);
        Assert.Equal(testMessage, mockLogger.ErrorMessages[0]);
        Assert.Same(testException, mockLogger.LoggedExceptions[0]);
    }

    [Fact]
    public void MockLogger_ClearResetsAllMessages()
    {
        // Arrange
        var mockLogger = new MockLogger();
        mockLogger.LogInfo("Info");
        mockLogger.LogWarning("Warning");
        mockLogger.LogError("Error");
        mockLogger.LogError("Error with exception", new Exception("Test"));

        // Act
        mockLogger.Clear();

        // Assert
        Assert.Empty(mockLogger.InfoMessages);
        Assert.Empty(mockLogger.WarningMessages);
        Assert.Empty(mockLogger.ErrorMessages);
        Assert.Empty(mockLogger.LoggedExceptions);
    }

    [Fact]
    public void MockLogger_SupportsMultipleMessages()
    {
        // Arrange
        var mockLogger = new MockLogger();

        // Act
        mockLogger.LogInfo("First message");
        mockLogger.LogInfo("Second message");
        mockLogger.LogInfo("Third message");

        // Assert
        Assert.Equal(3, mockLogger.InfoMessages.Count);
        Assert.Equal("First message", mockLogger.InfoMessages[0]);
        Assert.Equal("Second message", mockLogger.InfoMessages[1]);
        Assert.Equal("Third message", mockLogger.InfoMessages[2]);
    }
}
