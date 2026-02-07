using MassifCentral.Lib.Models;

namespace MassifCentral.Lib.Services;

/// <summary>
/// Represents a cache data container with metadata and file entries.
/// </summary>
public class CacheData
{
    /// <summary>
    /// Gets or sets the cache metadata including scan information.
    /// </summary>
    public CacheMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of cached file entries.
    /// </summary>
    public List<FileEntry> Files { get; set; } = new();
}
