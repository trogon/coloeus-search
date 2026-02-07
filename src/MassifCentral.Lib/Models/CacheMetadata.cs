namespace MassifCentral.Lib.Models;

/// <summary>
/// Stores cache provenance information including what directory was scanned and when.
/// Enables users to understand the source and age of cached file data.
/// </summary>
public class CacheMetadata
{
    /// <summary>
    /// Gets or sets the exact directory path that was scanned to create this cache.
    /// </summary>
    public string ScannedDirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the directory scan was performed (UTC).
    /// </summary>
    public DateTime ScanDateTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the total number of files cached.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Gets or sets the cache version number for future format compatibility.
    /// </summary>
    public int CacheVersionNumber { get; set; } = 1;
}
