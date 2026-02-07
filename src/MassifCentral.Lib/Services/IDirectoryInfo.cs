namespace MassifCentral.Lib.Services;

/// <summary>
/// Abstraction for directory information.
/// </summary>
public interface IDirectoryInfo
{
    /// <summary>
    /// Gets the full path of the directory.
    /// </summary>
    string FullPath { get; }

    /// <summary>
    /// Gets all files in this directory (non-recursive).
    /// </summary>
    IFileInfo[] GetFiles();

    /// <summary>
    /// Gets all subdirectories in this directory (non-recursive).
    /// </summary>
    IDirectoryInfo[] GetDirectories();
}
