using MassifCentral.Lib.Models;

namespace MassifCentral.Lib.Services;

/// <summary>
/// Defines the contract for analyzing large files within directory hierarchies.
/// Provides scan-once, query-many functionality with persistent caching.
/// </summary>
public interface ILargeFileAnalyzerService
{
    /// <summary>
    /// Initiates a directory scan or loads from cache if available.
    /// Uses load-first strategy: checks for existing cache with matching directory path.
    /// If cache exists and path matches, loads from disk (fast).
    /// If no cache exists, scans directory recursively and saves cache (slow, one-time cost).
    /// </summary>
    /// <param name="directoryPath">The absolute path to the directory to scan.</param>
    /// <exception cref="ArgumentException">Thrown if directoryPath is null or empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exist.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if access to the directory is denied.</exception>
    void ScanDirectory(string directoryPath);

    /// <summary>
    /// Retrieves the top N largest files from the scanned directory cache.
    /// Optionally filters by file extension.
    /// </summary>
    /// <param name="count">
    /// The number of largest files to return (default: 10).
    /// Must be a positive integer.
    /// </param>
    /// <param name="fileExtension">
    /// Optional file extension filter (e.g., ".log", ".bak", ".tmp").
    /// Case-insensitive. If null or empty, returns results for all files.
    /// Exact matching: ".log" matches ".log" files only, not ".log.bak".
    /// </param>
    /// <returns>
    /// An enumerable of FileEntry objects sorted by size (largest first).
    /// Returns empty enumerable if no files match the extension filter and count is less than total files.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if ScanDirectory has not been called.</exception>
    IEnumerable<FileEntry> GetTopLargestFiles(int count = 10, string? fileExtension = null);

    /// <summary>
    /// Gets the total number of files in the current in-memory cache.
    /// </summary>
    /// <returns>The count of files currently cached in memory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if ScanDirectory has not been called.</exception>
    int GetScannedFileCount();

    /// <summary>
    /// Gets the directory path that was scanned to create the current cache.
    /// </summary>
    /// <returns>The absolute path of the scanned directory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if ScanDirectory has not been called.</exception>
    string GetScannedDirectoryPath();

    /// <summary>
    /// Gets metadata about the current cache including provenance and scan timing.
    /// </summary>
    /// <returns>
    /// A CacheMetadata object containing the scanned directory path, scan date/time, and file count;
    /// null if no scan has been performed.
    /// </returns>
    CacheMetadata? GetCacheMetadata();

    /// <summary>
    /// Checks whether a directory scan has been completed.
    /// </summary>
    /// <returns>true if ScanDirectory has been called successfully; otherwise false.</returns>
    bool IsScanComplete();

    /// <summary>
    /// Clears the in-memory cache of file entries and metadata.
    /// Note: Does NOT delete the cache file from disk; use ClearCacheFromDisk() for that.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Removes both the in-memory cache and the persistent cache file from disk.
    /// </summary>
    /// <exception cref="Exception">May throw exceptions from file system operations.</exception>
    Task ClearCacheFromDiskAsync();
}
