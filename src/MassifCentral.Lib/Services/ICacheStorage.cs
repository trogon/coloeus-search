using MassifCentral.Lib.Models;

namespace MassifCentral.Lib.Services;

/// <summary>
/// Defines the contract for cache storage implementations.
/// Allows developers to implement custom cache storage strategies (file, database, cloud, etc.).
/// </summary>
public interface ICacheStorage
{
    /// <summary>
    /// Loads cached file entries and metadata asynchronously.
    /// </summary>
    /// <returns>
    /// A CacheData object containing metadata and file entries if cache exists and is valid;
    /// null if cache does not exist or is corrupted.
    /// </returns>
    Task<CacheData?> LoadCacheAsync();

    /// <summary>
    /// Persists file entries and metadata to cache storage asynchronously.
    /// </summary>
    /// <param name="cacheData">The cache data containing metadata and file entries to persist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveCacheAsync(CacheData cacheData);

    /// <summary>
    /// Removes cached data from storage asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteCacheAsync();
}
