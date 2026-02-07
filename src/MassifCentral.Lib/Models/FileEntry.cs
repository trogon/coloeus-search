namespace MassifCentral.Lib.Models;

/// <summary>
/// Represents metadata for a file discovered during directory scanning.
/// Contains file information without reading file content.
/// </summary>
public class FileEntry
{
    /// <summary>
    /// Gets or sets the complete file path including filename and extension.
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filename with extension.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file extension including the dot (e.g., ".log", ".bak", ".tmp").
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent directory path.
    /// </summary>
    public string DirectoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the last modification timestamp in UTC.
    /// </summary>
    public DateTime LastModifiedUtc { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }
}
