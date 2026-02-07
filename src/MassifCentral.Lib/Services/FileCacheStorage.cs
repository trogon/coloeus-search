using System.Text.Json;
using MassifCentral.Lib.Models;
using MassifCentral.Lib.Utilities;

namespace MassifCentral.Lib.Services;

/// <summary>
/// File-based implementation of cache storage using JSON serialization.
/// Stores cache in a configurable directory with metadata-based lookup.
/// </summary>
public class FileCacheStorage : ICacheStorage
{
    private const string CacheFileName = "cache.db";
    private readonly string _cacheDirectory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the FileCacheStorage class.
    /// </summary>
    /// <param name="logger">The logger for diagnostic information.</param>
    /// <param name="cacheDirectory">
    /// Optional custom cache directory. If not provided, uses %TEMP%\MassifCentral\.
    /// </param>
    public FileCacheStorage(ILogger logger, string? cacheDirectory = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        if (!string.IsNullOrWhiteSpace(cacheDirectory))
        {
            _cacheDirectory = cacheDirectory;
        }
        else
        {
            _cacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "MassifCentral");
        }

        // Ensure cache directory exists
        try
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
                _logger.LogDebug("Created cache directory: {CacheDirectory}", _cacheDirectory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to create cache directory {CacheDirectory}: {Exception}",
                _cacheDirectory, ex.Message);
        }
    }

    /// <summary>
    /// Loads cached file entries and metadata from disk asynchronously.
    /// </summary>
    /// <returns>
    /// A CacheData object if cache exists and is valid; null if cache does not exist or is corrupted.
    /// </returns>
    public async Task<CacheData?> LoadCacheAsync()
    {
        var cacheFilePath = Path.Combine(_cacheDirectory, CacheFileName);

        if (!File.Exists(cacheFilePath))
        {
            _logger.LogDebug("Cache file not found: {CacheFilePath}", cacheFilePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(cacheFilePath);
            var cacheData = JsonSerializer.Deserialize<CacheData>(json);

            if (cacheData == null)
            {
                _logger.LogError("Cache file deserialization returned null: {CacheFilePath}", cacheFilePath);
                return null;
            }

            _logger.LogDebug(
                "Loaded cache successfully: {FileCount} files from {ScannedPath}",
                cacheData.Files.Count,
                cacheData.Metadata.ScannedDirectoryPath);

            return cacheData;
        }
        catch (JsonException ex)
        {
            _logger.LogError("Cache file is corrupted (invalid JSON): {Exception}. Deleting corrupted file.", 
                ex.Message);
            
            try
            {
                File.Delete(cacheFilePath);
            }
            catch (Exception deleteEx)
            {
                _logger.LogError("Failed to delete corrupted cache file: {Exception}", deleteEx.Message);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load cache: {Exception}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Saves cache data (metadata and file entries) to disk asynchronously.
    /// </summary>
    /// <param name="cacheData">The cache data to persist.</param>
    public async Task SaveCacheAsync(CacheData cacheData)
    {
        ArgumentNullException.ThrowIfNull(cacheData);

        var cacheFilePath = Path.Combine(_cacheDirectory, CacheFileName);

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(cacheData, options);
            
            await File.WriteAllTextAsync(cacheFilePath, json);

            _logger.LogDebug(
                "Saved cache successfully: {FileCount} files to {CacheFilePath}",
                cacheData.Files.Count,
                cacheFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save cache to {CacheFilePath}: {Exception}",
                cacheFilePath, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Deletes the cache file from disk asynchronously.
    /// </summary>
    public async Task DeleteCacheAsync()
    {
        var cacheFilePath = Path.Combine(_cacheDirectory, CacheFileName);

        if (!File.Exists(cacheFilePath))
        {
            _logger.LogDebug("Cache file does not exist, nothing to delete: {CacheFilePath}", cacheFilePath);
            return;
        }

        try
        {
            File.Delete(cacheFilePath);
            _logger.LogDebug("Cache file deleted: {CacheFilePath}", cacheFilePath);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to delete cache file {CacheFilePath}: {Exception}",
                cacheFilePath, ex.Message);
            throw;
        }
    }
}
