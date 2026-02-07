using MassifCentral.Lib.Models;
using MassifCentral.Lib.Utilities;

namespace MassifCentral.Lib.Services;

/// <summary>
/// Service for analyzing large files within directory hierarchies.
/// Implements scan-once, query-many pattern with persistent caching for optimal performance.
/// Uses load-first strategy to minimize file system I/O on repeated invocations.
/// </summary>
public class LargeFileAnalyzerService : ILargeFileAnalyzerService
{
    private const int MaxRecursionDepth = 5000;
    private const int PathLengthWarningThreshold600 = 600;
    private const int PathLengthWarningThreshold800 = 800;
    private const int FileCountWarningThreshold200K = 200_000;
    private const int FileCountWarningThreshold500K = 500_000;
    private const int FileCountWarningThreshold1M = 1_000_000;
    
    private readonly ILogger _logger;
    private readonly ICacheStorage _cacheStorage;
    private readonly IFileSystemProvider _fileSystemProvider;
    private List<FileEntry> _cachedFiles = new();
    private CacheMetadata? _cacheMetadata;
    private int _maxDirectoryDepth = 0;
    
    // Path length monitoring
    private int _maxPathLength = 0;
    private long _totalPathLength = 0;
    private int _pathsExceeding600Chars = 0;
    private int _pathsExceeding800Chars = 0;
    
    // GC collection tracking
    private long _memoryAtStart = 0;

    /// <summary>
    /// Initializes a new instance of the LargeFileAnalyzerService class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="cacheStorage">The cache storage implementation for persisting file lists.</param>
    /// <param name="fileSystemProvider">The file system provider for abstracted IO operations (optional, defaults to real file system).</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if logger or cacheStorage is null.
    /// </exception>
    public LargeFileAnalyzerService(ILogger logger, ICacheStorage cacheStorage, IFileSystemProvider? fileSystemProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheStorage = cacheStorage ?? throw new ArgumentNullException(nameof(cacheStorage));
        _fileSystemProvider = fileSystemProvider ?? new RealFileSystemProvider();
    }

    /// <summary>
    /// Initiates directory scan or loads from cache using load-first strategy.
    /// </summary>
    public void ScanDirectory(string directoryPath)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            _logger.LogError("ScanDirectory called with null or empty directoryPath");
            throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));
        }

        // Normalize path
        var normalizedPath = Path.GetFullPath(directoryPath);

        if (!_fileSystemProvider.DirectoryExists(normalizedPath))
        {
            _logger.LogError("Directory not found: {DirectoryPath}", normalizedPath);
            throw new DirectoryNotFoundException($"Directory not found: {normalizedPath}");
        }

        _fileSystemProvider.ValidateDirectoryAccess(normalizedPath);
        _logger.LogDebug("ScanDirectory initiated for: {DirectoryPath}", normalizedPath);

        // Load-first strategy: try to load from cache, otherwise perform fresh scan
        if (!TryLoadCacheForDirectory(normalizedPath))
        {
            PerformDirectoryScan(normalizedPath);
        }
    }



    /// <summary>
    /// Attempts to load cache for the specified directory.
    /// Returns true if cache was found and loaded successfully; false if cache miss or different directory.
    /// </summary>
    /// <param name="normalizedPath">The normalized directory path to load cache for.</param>
    /// <returns>True if cache was loaded and is valid for the directory; false otherwise.</returns>
    private bool TryLoadCacheForDirectory(string normalizedPath)
    {
        try
        {
            var cachedData = _cacheStorage.LoadCacheAsync().GetAwaiter().GetResult();

            // If no cache exists, indicate cache miss
            if (cachedData == null)
            {
                return false;
            }

            // If cache exists but is for different directory, log warning and force rescan
            if (!normalizedPath.Equals(cachedData.Metadata.ScannedDirectoryPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Existing cache is from different directory: {CachedPath}. Will scan new directory: {NewPath}",
                    cachedData.Metadata.ScannedDirectoryPath,
                    normalizedPath);
                return false;
            }

            // Cache matches - load it into memory
            _cachedFiles = cachedData.Files;
            _cacheMetadata = cachedData.Metadata;

            var ageSeconds = (DateTime.UtcNow - _cacheMetadata.ScanDateTimeUtc).TotalSeconds;
            _logger.LogDebug(
                "Loaded cache from disk: {FileCount} files from {ScannedPath} (scanned {AgeSeconds:F1}s ago)",
                _cachedFiles.Count,
                _cacheMetadata.ScannedDirectoryPath,
                ageSeconds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Failed to load cache, will perform fresh scan: {Exception}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Performs a fresh directory scan and saves results to cache.
    /// </summary>
    private void PerformDirectoryScan(string directoryPath)
    {
        var scanStartTime = DateTime.UtcNow;
        _logger.LogDebug("Starting directory scan: {DirectoryPath}", directoryPath);

        // Capture memory usage before scan
        GC.Collect(2, GCCollectionMode.Optimized);
        GC.WaitForPendingFinalizers();
        _memoryAtStart = GC.GetTotalMemory(false);

        try
        {
            _cachedFiles = new();
            _maxDirectoryDepth = 0;  // Reset depth tracking for new scan
            _maxPathLength = 0;       // Reset path length tracking
            _totalPathLength = 0;
            _pathsExceeding600Chars = 0;
            _pathsExceeding800Chars = 0;

            // Recursive directory scan
            var rootDirInfo = _fileSystemProvider.GetDirectoryInfo(directoryPath);
            ScanDirectoryRecursive(rootDirInfo, depth: 0);

            var scanDuration = DateTime.UtcNow - scanStartTime;
            
            // Calculate path length metrics
            var averagePathLength = _cachedFiles.Count > 0 
                ? (decimal)_totalPathLength / _cachedFiles.Count 
                : 0;
            
            _logger.LogDebug(
                "Directory scan completed: {FileCount} files discovered in {DurationSeconds:F2}s (max depth: {MaxDepth})",
                _cachedFiles.Count,
                scanDuration.TotalSeconds,
                _maxDirectoryDepth);
            
            // Log path length metrics
            LogPathLengthMetrics(averagePathLength);
            
            // Log file count warnings if thresholds exceeded
            LogFileCountWarnings();
            
            // Log memory usage metrics
            LogMemoryMetrics();

            // Create and save cache metadata
            _cacheMetadata = new CacheMetadata
            {
                ScannedDirectoryPath = directoryPath,
                ScanDateTimeUtc = scanStartTime,
                FileCount = _cachedFiles.Count,
                CacheVersionNumber = 1
            };

            // Save cache to disk
            var cacheData = new CacheData
            {
                Metadata = _cacheMetadata,
                Files = _cachedFiles
            };

            _cacheStorage.SaveCacheAsync(cacheData).GetAwaiter().GetResult();
            _logger.LogDebug("Cache saved successfully for directory: {DirectoryPath}", directoryPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError("Access denied during scan of: {DirectoryPath}", directoryPath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during directory scan: {Exception}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Recursively scans a directory and its subdirectories for files.
    /// Limits recursion depth to 5,000 levels to prevent stack overflow on pathological directory structures.
    /// </summary>
    /// <param name="directory">The directory to scan.</param>
    /// <param name="depth">The current recursion depth (0 for root).</param>
    private void ScanDirectoryRecursive(IDirectoryInfo directory, int depth = 0)
    {
        // Track maximum depth encountered
        _maxDirectoryDepth = Math.Max(_maxDirectoryDepth, depth);

        // Prevent stack overflow on deeply nested directory structures
        if (depth > MaxRecursionDepth)
        {
            _logger.LogWarning(
                "Maximum directory nesting depth ({MaxDepth}) exceeded at {DirectoryPath}. " +
                "Subdirectories will not be scanned to prevent stack overflow.",
                MaxRecursionDepth,
                directory.FullPath);
            return;
        }

        try
        {
            // Get all files in current directory
            var files = directory.GetFiles();
            
            foreach (var file in files)
            {
                try
                {
                    var fileEntry = ConvertToFileEntry(file);
                    _cachedFiles.Add(fileEntry);
                    
                    // Track path length metrics
                    TrackPathLength(fileEntry.FullPath);

                    // Log progress for large scans
                    if (_cachedFiles.Count % 10000 == 0)
                    {
                        _logger.LogDebug("Scan progress: {FileCount} files discovered", _cachedFiles.Count);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogWarning("Access denied to file: {FilePath}", file.FullPath);
                    // Continue scanning other files
                }
                catch (Exception exception)
                {
                    _logger.LogWarning("Error reading file metadata for {FilePath}: {Exception}",
                        file.FullPath, exception.Message);
                    // Continue scanning other files
                }
            }

            // Recursively scan subdirectories
            var subdirectories = directory.GetDirectories();
            foreach (var subdirectory in subdirectories)
            {
                try
                {
                    ScanDirectoryRecursive(subdirectory, depth + 1);  // Pass incremented depth
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogWarning("Access denied to directory: {DirectoryPath}", subdirectory.FullPath);
                    // Continue scanning other directories
                }
                catch (Exception)
                {
                    _logger.LogWarning("Error scanning subdirectory {DirectoryPath}: {Exception}",
                        subdirectory.FullPath, "Unknown error");
                    // Continue scanning other directories
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogError("Access denied to directory: {DirectoryPath}", directory.FullPath);
            throw;
        }
        catch (Exception)
        {
            _logger.LogError("Error accessing directory {DirectoryPath}: {Exception}",
                directory.FullPath, "Unknown error");
            throw;
        }
    }

    /// <summary>
    /// Converts a FileInfo object to a FileEntry model.
    /// </summary>
    private static FileEntry ConvertToFileEntry(IFileInfo file)
    {
        return new FileEntry
        {
            FullPath = file.FullPath,
            FileName = file.Name,
            Extension = file.Extension,
            DirectoryName = file.DirectoryPath,
            SizeBytes = file.Length,
            CreatedUtc = file.CreatedUtc,
            LastModifiedUtc = file.LastModifiedUtc,
            IsReadOnly = file.IsReadOnly
        };
    }

    /// <summary>
    /// Retrieves the top N largest files from cache, optionally filtered by extension.
    /// </summary>
    public IEnumerable<FileEntry> GetTopLargestFiles(int count = 10, string? fileExtension = null)
    {
        if (!IsScanComplete())
        {
            _logger.LogError("GetTopLargestFiles called before ScanDirectory");
            throw new InvalidOperationException("ScanDirectory must be called before querying results.");
        }

        if (count <= 0)
        {
            throw new ArgumentException("Count must be a positive integer.", nameof(count));
        }

        IEnumerable<FileEntry> query = _cachedFiles;

        // Apply extension filter if provided
        if (!string.IsNullOrWhiteSpace(fileExtension))
        {
            var normalizedExtension = fileExtension.StartsWith(".")
                ? fileExtension
                : "." + fileExtension;

            query = query.Where(f =>
                f.Extension.Equals(normalizedExtension, StringComparison.OrdinalIgnoreCase));
        }

        // Sort by size (largest first) and take top N
        var results = query
            .OrderByDescending(f => f.SizeBytes)
            .Take(count)
            .ToList();

        _logger.LogDebug(
            "Query results: {Count} files returned (extension filter: {Extension})",
            results.Count,
            fileExtension ?? "none");

        return results;
    }

    /// <summary>
    /// Gets the total number of files in the in-memory cache.
    /// </summary>
    public int GetScannedFileCount()
    {
        if (!IsScanComplete())
        {
            _logger.LogError("GetScannedFileCount called before ScanDirectory");
            throw new InvalidOperationException("ScanDirectory must be called before querying results.");
        }

        return _cachedFiles.Count;
    }

    /// <summary>
    /// Gets the directory path that was scanned.
    /// </summary>
    public string GetScannedDirectoryPath()
    {
        if (!IsScanComplete())
        {
            _logger.LogError("GetScannedDirectoryPath called before ScanDirectory");
            throw new InvalidOperationException("ScanDirectory must be called before querying results.");
        }

        return _cacheMetadata?.ScannedDirectoryPath ?? string.Empty;
    }

    /// <summary>
    /// Gets cache metadata including provenance and scan timing.
    /// </summary>
    public CacheMetadata? GetCacheMetadata()
    {
        return _cacheMetadata;
    }

    /// <summary>
    /// Gets the maximum directory nesting depth encountered during the last scan.
    /// Useful for monitoring directory structure and identifying deep nesting patterns.
    /// </summary>
    public int MaxDirectoryDepth => _maxDirectoryDepth;

    /// <summary>
    /// Checks whether a directory scan has been completed.
    /// </summary>
    public bool IsScanComplete()
    {
        return _cacheMetadata != null && _cachedFiles.Count >= 0;
    }

    /// <summary>
    /// Clears the in-memory cache only.
    /// </summary>
    public void ClearCache()
    {
        _cachedFiles = new();
        _cacheMetadata = null;
        _maxDirectoryDepth = 0;
        _logger.LogDebug("In-memory cache cleared");
    }

    /// <summary>
    /// Removes both in-memory cache and persistent cache file from disk.
    /// </summary>
    public async Task ClearCacheFromDiskAsync()
    {
        try
        {
            ClearCache();
            await _cacheStorage.DeleteCacheAsync();
            _logger.LogDebug("Cache cleared from disk");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to clear cache from disk: {Exception}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Tracks path length metrics for monitoring long paths.
    /// </summary>
    /// <param name="path">The file path to analyze.</param>
    private void TrackPathLength(string path)
    {
        int pathLength = path.Length;
        _maxPathLength = Math.Max(_maxPathLength, pathLength);
        _totalPathLength += pathLength;
        
        if (pathLength > PathLengthWarningThreshold800)
        {
            _pathsExceeding800Chars++;
        }
        else if (pathLength > PathLengthWarningThreshold600)
        {
            _pathsExceeding600Chars++;
        }
    }

    /// <summary>
    /// Logs path length metrics if thresholds are exceeded.
    /// </summary>
    private void LogPathLengthMetrics(decimal averagePathLength)
    {
        if (_pathsExceeding800Chars > 0 || _pathsExceeding600Chars > 0)
        {
            _logger.LogWarning(
                "Long paths detected: max={MaxPathLength} chars, avg={AvgPathLength:F0} chars, " +
                "exceeding600+={PathsExceeding600}, exceeding800+={PathsExceeding800}. " +
                "Memory footprint may be higher than typical. Cache persistence recommended.",
                _maxPathLength,
                averagePathLength,
                _pathsExceeding600Chars,
                _pathsExceeding800Chars);
        }
        else if (averagePathLength > PathLengthWarningThreshold600)
        {
            _logger.LogDebug(
                "Moderate path lengths detected: max={MaxPathLength} chars, avg={AvgPathLength:F0} chars. " +
                "Monitor memory usage during subsequent queries.",
                _maxPathLength,
                averagePathLength);
        }
        else
        {
            _logger.LogDebug(
                "Path length metrics: max={MaxPathLength} chars, avg={AvgPathLength:F0} chars",
                _maxPathLength,
                averagePathLength);
        }
    }

    /// <summary>
    /// Logs warnings if file count exceeds known thresholds.
    /// </summary>
    private void LogFileCountWarnings()
    {
        if (_cachedFiles.Count >= FileCountWarningThreshold1M)
        {
            _logger.LogWarning(
                "Very large file system scanned: {FileCount:N0} files. " +
                "Consider using iterative approach for such large file counts. " +
                "Memory usage approaching 500 MB specification limit.",
                _cachedFiles.Count);
        }
        else if (_cachedFiles.Count >= FileCountWarningThreshold500K)
        {
            _logger.LogWarning(
                "Large file system scanned: {FileCount:N0} files. " +
                "Cache persistence critical to avoid repeated expensive scans. " +
                "Iterative approach recommended for larger structures.",
                _cachedFiles.Count);
        }
        else if (_cachedFiles.Count >= FileCountWarningThreshold200K)
        {
            _logger.LogDebug(
                "Scanned {FileCount:N0} files. Cache persistence enabled for performance on repeated queries.",
                _cachedFiles.Count);
        }
    }

    /// <summary>
    /// Logs garbage collection metrics to indicate memory pressure during scan.
    /// </summary>
    private void LogMemoryMetrics()
    {
        var memoryAtEnd = GC.GetTotalMemory(false);
        var memoryUsedMB = (memoryAtEnd - _memoryAtStart) / (1024.0 * 1024.0);
        var totalMemoryMB = memoryAtEnd / (1024.0 * 1024.0);
        
        if (memoryUsedMB > 400)
        {
            _logger.LogWarning(
                "High memory usage during scan: {MemoryUsedMB:F1} MB used, {TotalMemoryMB:F1} MB total. " +
                "Approaching 500 MB specification limit. Consider caching strategy or iterative approach.",
                memoryUsedMB, totalMemoryMB);
        }
        else if (memoryUsedMB > 200)
        {
            _logger.LogDebug(
                "Moderate memory usage during scan: {MemoryUsedMB:F1} MB used, {TotalMemoryMB:F1} MB total",
                memoryUsedMB, totalMemoryMB);
        }
        else
        {
            _logger.LogDebug(
                "Memory usage during scan: {MemoryUsedMB:F1} MB used, {TotalMemoryMB:F1} MB total",
                memoryUsedMB, totalMemoryMB);
        }
    }
}
