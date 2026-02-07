using MassifCentral.Lib;
using MassifCentral.Lib.Models;

namespace MassifCentral.Tests;

/// <summary>
/// Unit tests for the MassifCentral library constants and models.
/// </summary>
public class LibraryTests
{
    [Fact]
    public void Constants_HasValidApplicationName()
    {
        // Arrange & Act
        var appName = Constants.ApplicationName;

        // Assert
        Assert.NotNull(appName);
        Assert.Equal("MassifCentral", appName);
    }

    [Fact]
    public void Constants_HasValidVersion()
    {
        // Arrange & Act
        var version = Constants.Version;

        // Assert
        Assert.NotNull(version);
        Assert.Equal("1.0.0", version);
    }

    [Fact]
    public void BaseEntity_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.IsActive);
        Assert.True(entity.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public void BaseEntity_HasValidModifiedAtTime()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert - Allow a small time difference due to precision
        var timeDifference = Math.Abs((entity.ModifiedAt - entity.CreatedAt).TotalMilliseconds);
        Assert.True(timeDifference < 100, $"Time difference should be very small, but was {timeDifference}ms");
    }

    /// <summary>
    /// Test implementation of BaseEntity for testing purposes.
    /// </summary>
    private class TestEntity : BaseEntity
    {
    }
}
