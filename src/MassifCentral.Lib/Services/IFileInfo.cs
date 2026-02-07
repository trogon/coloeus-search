namespace MassifCentral.Lib.Services;

/// <summary>
/// Abstraction for file information.
/// </summary>
public interface IFileInfo
{
    /// <summary>
    /// Gets the full path of the file.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Gets the file name without directory path.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the file extension (e.g., ".txt").
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Gets the directory path containing the file.
    /// </summary>
    string DirectoryPath { get; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    long Length { get; }

    /// <summary>
    /// Gets the file creation time in UTC.
    /// </summary>
    DateTime CreatedUtc { get; }

    /// <summary>
    /// Gets the file last write time in UTC.
    /// </summary>
    DateTime LastModifiedUtc { get; }

    /// <summary>
    /// Gets whether the file is read-only.
    /// </summary>
    bool IsReadOnly { get; }
}
