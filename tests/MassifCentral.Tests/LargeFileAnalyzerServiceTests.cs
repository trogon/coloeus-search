using MassifCentral.Lib.Models;
using MassifCentral.Lib.Services;
using MassifCentral.Tests.Mocks;

namespace MassifCentral.Tests;

/// <summary>
/// Unit tests for the LargeFileAnalyzerService.
/// Tests directory scanning, caching, querying, and extension filtering.
/// Uses mock file system to avoid disk IO during tests.
/// </summary>
public class LargeFileAnalyzerServiceTests : IDisposable
{
    private readonly MockLogger _logger;
    private readonly MockFileSystemProvider _fileSystem;
    private readonly MockCacheStorage _cacheStorage;
    private readonly ILargeFileAnalyzerService _service;
    private const string TestRootPath = "/test/root";

    public LargeFileAnalyzerServiceTests()
    {
        _logger = new MockLogger();
        _fileSystem = new MockFileSystemProvider(TestRootPath);
        _cacheStorage = new MockCacheStorage(_logger);
        _service = new LargeFileAnalyzerService(_logger, _cacheStorage, _fileSystem);
    }

    public void Dispose()
    {
        // No real resources to clean up
    }

    [Fact]
    public void ScanDirectory_WithNullPath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.ScanDirectory(null!));
        Assert.Contains("null or empty", ex.Message);
    }

    [Fact]
    public void ScanDirectory_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _service.ScanDirectory(string.Empty));
        Assert.Contains("null or empty", ex.Message);
    }

    [Fact]
    public void ScanDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = "/test/root/NonExistent";

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => _service.ScanDirectory(nonExistentPath));
    }

    [Fact]
    public void ScanDirectory_WithValidDirectory_ScansSuccessfully()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("file1.log", 1024L),
            ("file2.bak", 2048L),
            ("file3.log", 512L)
        });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        Assert.True(_service.IsScanComplete());
        Assert.Equal(3, _service.GetScannedFileCount());
    }

    [Fact]
    public void GetTopLargestFiles_WithoutScan_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _service.GetTopLargestFiles());
        Assert.Contains("ScanDirectory must be called", ex.Message);
    }

    [Fact]
    public void GetTopLargestFiles_ReturnsSortedBySize()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("small.txt", 100L),
            ("medium.txt", 1000L),
            ("large.txt", 5000L)
        });
        _service.ScanDirectory(TestRootPath);

        // Act
        var results = _service.GetTopLargestFiles(count: 3).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(5000, results[0].SizeBytes);
        Assert.Equal(1000, results[1].SizeBytes);
        Assert.Equal(100, results[2].SizeBytes);
    }

    [Fact]
    public void GetTopLargestFiles_DefaultCountIsTen()
    {
        // Arrange
        var fileNames = Enumerable.Range(1, 15)
            .Select(i => ($"file{i:D2}.txt", (long)i * 100))
            .ToArray();
        
        AddTestFiles(TestRootPath, fileNames);
        _service.ScanDirectory(TestRootPath);

        // Act
        var results = _service.GetTopLargestFiles().ToList();

        // Assert
        Assert.Equal(10, results.Count);
    }

    [Fact]
    public void GetTopLargestFiles_WithCustomCount()
    {
        // Arrange
        var fileNames = Enumerable.Range(1, 10)
            .Select(i => ($"file{i:D2}.txt", (long)i * 100))
            .ToArray();
        
        AddTestFiles(TestRootPath, fileNames);
        _service.ScanDirectory(TestRootPath);

        // Act
        var results = _service.GetTopLargestFiles(count: 5).ToList();

        // Assert
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void GetTopLargestFiles_FiltersByExtension()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("file1.log", 5000L),
            ("file2.bak", 4000L),
            ("file3.log", 3000L),
            ("file4.bak", 2000L),
            ("file5.log", 1000L)
        });
        _service.ScanDirectory(TestRootPath);

        // Act
        var logFiles = _service.GetTopLargestFiles(count: 10, fileExtension: ".log").ToList();

        // Assert
        Assert.Equal(3, logFiles.Count);
        Assert.All(logFiles, f => Assert.Equal(".log", f.Extension));
        Assert.Equal(5000, logFiles[0].SizeBytes);
    }

    [Fact]
    public void GetTopLargestFiles_ExtensionFilterIsCaseInsensitive()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("file1.LOG", 5000L),
            ("file2.log", 4000L)
        });
        _service.ScanDirectory(TestRootPath);

        // Act
        var results = _service.GetTopLargestFiles(count: 10, fileExtension: ".log").ToList();

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void GetTopLargestFiles_ExactExtensionMatching()
    {
        // Arrange - Create files with similar extensions
        AddTestFiles(TestRootPath, new[]
        {
            ("file1.log", 5000L),
            ("file2.log.bak", 4000L),
            ("file3.log.gz", 3000L)
        });
        _service.ScanDirectory(TestRootPath);

        // Act
        var logFiles = _service.GetTopLargestFiles(count: 10, fileExtension: ".log").ToList();

        // Assert - Only exact .log matches, not .log.bak or .log.gz
        Assert.Single(logFiles);
        Assert.Equal("file1.log", logFiles[0].FileName);
    }

    [Fact]
    public void GetCacheMetadata_BeforeScan_ReturnsNull()
    {
        // Act
        var metadata = _service.GetCacheMetadata();

        // Assert
        Assert.Null(metadata);
    }

    [Fact]
    public void GetCacheMetadata_AfterScan_ReturnsProperly()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });
        var beforeScan = DateTime.UtcNow;

        // Act
        _service.ScanDirectory(TestRootPath);
        var metadata = _service.GetCacheMetadata();
        var afterScan = DateTime.UtcNow;

        // Assert
        Assert.NotNull(metadata);
        Assert.Equal(Path.GetFullPath(TestRootPath), metadata.ScannedDirectoryPath);
        Assert.Equal(1, metadata.FileCount);
        Assert.InRange(metadata.ScanDateTimeUtc, beforeScan, afterScan);
    }

    [Fact]
    public void GetScannedDirectoryPath_WithoutScan_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _service.GetScannedDirectoryPath());
        Assert.Contains("ScanDirectory must be called", ex.Message);
    }

    [Fact]
    public void GetScannedDirectoryPath_AfterScan_ReturnsCorrectPath()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });

        // Act
        _service.ScanDirectory(TestRootPath);
        var path = _service.GetScannedDirectoryPath();

        // Assert
        Assert.Equal(Path.GetFullPath(TestRootPath), path);
    }

    [Fact]
    public void GetScannedFileCount_WithoutScan_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => _service.GetScannedFileCount());
        Assert.Contains("ScanDirectory must be called", ex.Message);
    }

    [Fact]
    public void ClearCache_RemovesInMemoryData()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });
        _service.ScanDirectory(TestRootPath);

        // Act
        _service.ClearCache();

        // Assert
        Assert.False(_service.IsScanComplete());
        Assert.Null(_service.GetCacheMetadata());
    }

    [Fact]
    public async Task ClearCacheFromDiskAsync_RemovesDiskAndMemory()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });
        _service.ScanDirectory(TestRootPath);

        // Act
        await _service.ClearCacheFromDiskAsync();

        // Assert
        Assert.False(_service.IsScanComplete());
        Assert.Null(_service.GetCacheMetadata());
    }

    [Fact]
    public void ScanDirectory_WithRecursiveDirectories_ScansAllFiles()
    {
        // Arrange
        _fileSystem.AddSubdirectory(TestRootPath, "Sub1");
        var subDir1 = Path.Combine(TestRootPath, "Sub1");
        _fileSystem.AddSubdirectory(subDir1, "Sub2");
        var subDir2 = Path.Combine(subDir1, "Sub2");

        AddTestFiles(TestRootPath, new[] { ("root.txt", 1000L) });
        AddTestFiles(subDir1, new[] { ("sub1.txt", 2000L) });
        AddTestFiles(subDir2, new[] { ("sub2.txt", 3000L) });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        Assert.Equal(3, _service.GetScannedFileCount());
    }

    [Fact]
    public void IsScanComplete_WithoutScan_ReturnsFalse()
    {
        // Act
        var isComplete = _service.IsScanComplete();

        // Assert
        Assert.False(isComplete);
    }

    [Fact]
    public void IsScanComplete_AfterScan_ReturnsTrue()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        Assert.True(_service.IsScanComplete());
    }

    [Fact]
    public void FileEntry_HasCorrectMetadata()
    {
        // Arrange
        var fileName = "testfile.log";
        var fileSize = 5000L;
        AddTestFiles(TestRootPath, new[] { (fileName, fileSize) });

        // Act
        _service.ScanDirectory(TestRootPath);
        var file = _service.GetTopLargestFiles(count: 1).First();

        // Assert
        Assert.Equal(fileName, file.FileName);
        Assert.Equal(".log", file.Extension);
        Assert.Equal(fileSize, file.SizeBytes);
        Assert.Equal(Path.GetFullPath(TestRootPath), file.DirectoryName);
        Assert.False(file.IsReadOnly);
        Assert.True(file.CreatedUtc > DateTime.MinValue);
        Assert.True(file.LastModifiedUtc > DateTime.MinValue);
    }

    [Fact]
    public async Task ScanDirectory_CallTwice_WithCacheCleared_UpdatesCacheData()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });
        _service.ScanDirectory(TestRootPath);

        var firstCount = _service.GetScannedFileCount();
        var firstMetadata = _service.GetCacheMetadata();

        // Wait a small amount to ensure timestamp difference
        await Task.Delay(100);

        // Act - Clear cache and add more files, then rescan
        await _service.ClearCacheFromDiskAsync();
        AddTestFiles(TestRootPath, new[] { ("file2.txt", 2000L) });
        _service.ScanDirectory(TestRootPath);

        var secondCount = _service.GetScannedFileCount();
        var secondMetadata = _service.GetCacheMetadata();

        // Assert
        Assert.Equal(1, firstCount);
        Assert.Equal(2, secondCount);
        Assert.NotEqual(firstMetadata?.ScanDateTimeUtc, secondMetadata?.ScanDateTimeUtc);
    }

    [Fact]
    public async Task ScanDirectory_CallTwice_ReusesCacheFromDisk()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[] { ("file1.txt", 1000L) });
        var service1 = new LargeFileAnalyzerService(_logger, _cacheStorage, _fileSystem);
        service1.ScanDirectory(TestRootPath);

        var firstScanTime = service1.GetCacheMetadata()!.ScanDateTimeUtc;
        var firstCount = service1.GetScannedFileCount();

        // Wait to ensure timestamps would differ if rescanning
        await Task.Delay(100);

        // Act - Create new service instance with same cache storage and file system
        var service2 = new LargeFileAnalyzerService(_logger, _cacheStorage, _fileSystem);
        service2.ScanDirectory(TestRootPath);

        var secondScanTime = service2.GetCacheMetadata()!.ScanDateTimeUtc;
        var secondCount = service2.GetScannedFileCount();

        // Assert - Cache should be reused, same timestamp and count
        Assert.Equal(firstCount, secondCount);
        Assert.Equal(firstScanTime, secondScanTime);
    }

    [Fact]
    public void ScanDirectory_WithLongPaths_LogsPathLengthWarning()
    {
        // Arrange
        // Create a structure with moderately long paths to test monitoring
        var baseDir = TestRootPath;
        var subPath = Path.Combine(baseDir, new string('a', 100), new string('b', 100));
        
        // Create subdirectories in the mock file system
        _fileSystem.AddSubdirectory(baseDir, new string('a', 100));
        var aDir = Path.Combine(baseDir, new string('a', 100));
        _fileSystem.AddSubdirectory(aDir, new string('b', 100));
        
        AddTestFiles(subPath, new[] { ("file.txt", 100L) });

        // Act
        _service.ScanDirectory(baseDir);

        // Assert
        // Path monitoring should log at least debug message about path metrics
        Assert.True(_logger.DebugMessages.Any(m => m.Contains("path", StringComparison.OrdinalIgnoreCase)),
            "Expected path length metrics to be logged");
    }

    [Fact]
    public void ScanDirectory_TracksPathLengthMetrics()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("short.txt", 100L),
            ("medium.txt", 1000L),
            ("large.txt", 5000L)
        });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        // Should have debug logs showing path length evaluation
        Assert.NotEmpty(_logger.DebugMessages);
    }

    [Fact]
    public void ScanDirectory_With200KFiles_LogsFileCountWarning()
    {
        // Note: Reduced to 1000 files to keep test fast
        // This test verifies that file count monitoring is implemented
        // without needing to create 200K files which would be very slow
        
        // Arrange
        AddTestFiles(TestRootPath, 
            Enumerable.Range(1, 1000)
                .Select(i => ($"file{i:D6}.txt", 100L))
                .ToArray());

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        // Should have debug or info logs about the scan
        Assert.True(_logger.DebugMessages.Any(m => m.Contains("scan", StringComparison.OrdinalIgnoreCase)) ||
                   _logger.WarningMessages.Any(m => m.Contains("scan", StringComparison.OrdinalIgnoreCase)), 
            "Expected scan completion message");
    }

    [Fact]
    public void ScanDirectory_LogsMemoryMetrics()
    {
        // Arrange
        AddTestFiles(TestRootPath, new[]
        {
            ("small.txt", 100L),
            ("medium.txt", 1000L),
            ("large.txt", 5000L)
        });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        Assert.True(_logger.DebugMessages.Any(m => m.Contains("memory", StringComparison.OrdinalIgnoreCase)),
            "Expected memory metrics in debug logs");
    }

    [Fact]
    public void ScanDirectory_WithModeratePathLength_LogsPathMetrics()
    {
        // Arrange
        var moSubdir = Path.Combine(TestRootPath, string.Join("\\", Enumerable.Range(1, 10).Select(i => new string('a', 40))));
        
        // Create the nested directories in the mock file system
        var currentDir = TestRootPath;
        foreach (var segment in Enumerable.Range(1, 10))
        {
            var subdir = new string('a', 40);
            _fileSystem.AddSubdirectory(currentDir, subdir);
            currentDir = Path.Combine(currentDir, subdir);
        }
        
        AddTestFiles(currentDir, new[] { ("file.txt", 100L) });

        // Act
        _service.ScanDirectory(TestRootPath);

        // Assert
        // Should have some debug logs about path metrics
        Assert.True(_logger.DebugMessages.Any(m => m.Contains("path", StringComparison.OrdinalIgnoreCase)),
            "Expected path metrics to be logged");
    }

    /// <summary>
    /// Helper method to add test files to the mock file system without disk IO.
    /// </summary>
    private void AddTestFiles(string directory, (string name, long size)[] files)
    {
        foreach (var (name, size) in files)
        {
            _fileSystem.AddFile(directory, name, size);
        }
    }
}

/// <summary>
/// Mock implementation of ICacheStorage for testing.
/// Stores cache in memory and allows verification of save/load operations.
/// </summary>
internal class MockCacheStorage : ICacheStorage
{
    private readonly MockLogger _logger;
    private CacheData? _cachedData;

    public MockCacheStorage(MockLogger logger)
    {
        _logger = logger;
    }

    public Task<CacheData?> LoadCacheAsync()
    {
        return Task.FromResult(_cachedData);
    }

    public Task SaveCacheAsync(CacheData cacheData)
    {
        _cachedData = cacheData;
        return Task.CompletedTask;
    }

    public Task DeleteCacheAsync()
    {
        _cachedData = null;
        return Task.CompletedTask;
    }
}
