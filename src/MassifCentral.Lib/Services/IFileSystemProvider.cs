using System.Security.AccessControl;

namespace MassifCentral.Lib.Services;

/// <summary>
/// Abstraction for file system operations.
/// Enables unit testing without actual IO and allows alternative implementations (virtual FS, cloud storage, etc.).
/// </summary>
public interface IFileSystemProvider
{
    /// <summary>
    /// Checks if a directory exists at the given path.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Gets directory information for the specified path.
    /// </summary>
    IDirectoryInfo GetDirectoryInfo(string path);

    /// <summary>
    /// Validates that a directory is accessible (able to read ACLs or enumerate contents).
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown if access is denied.</exception>
    void ValidateDirectoryAccess(string path);
}
