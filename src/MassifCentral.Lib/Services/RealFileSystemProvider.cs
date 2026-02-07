namespace MassifCentral.Lib.Services;

/// <summary>
/// Real implementation of IFileSystemProvider that uses actual .NET file system APIs.
/// </summary>
public class RealFileSystemProvider : IFileSystemProvider
{
    /// <summary>
    /// Checks if a directory exists at the given path.
    /// </summary>
    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// Gets directory information for the specified path.
    /// </summary>
    public IDirectoryInfo GetDirectoryInfo(string path)
    {
        return new RealDirectoryInfo(new DirectoryInfo(path));
    }

    /// <summary>
    /// Validates that a directory is accessible.
    /// </summary>
    public void ValidateDirectoryAccess(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);
            dirInfo.GetAccessControl();
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Access denied to directory: {path}", ex);
        }
    }
}

/// <summary>
/// Wrapper for DirectoryInfo that implements IDirectoryInfo.
/// </summary>
internal class RealDirectoryInfo : IDirectoryInfo
{
    private readonly DirectoryInfo _directoryInfo;

    public RealDirectoryInfo(DirectoryInfo directoryInfo)
    {
        _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
    }

    public string FullPath => _directoryInfo.FullName;

    public IFileInfo[] GetFiles()
    {
        return _directoryInfo
            .GetFiles(searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly)
            .Select(f => (IFileInfo)new RealFileInfo(f))
            .ToArray();
    }

    public IDirectoryInfo[] GetDirectories()
    {
        return _directoryInfo
            .GetDirectories(searchPattern: "*", searchOption: SearchOption.TopDirectoryOnly)
            .Select(d => (IDirectoryInfo)new RealDirectoryInfo(d))
            .ToArray();
    }
}

/// <summary>
/// Wrapper for FileInfo that implements IFileInfo.
/// </summary>
internal class RealFileInfo : IFileInfo
{
    private readonly FileInfo _fileInfo;

    public RealFileInfo(FileInfo fileInfo)
    {
        _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
    }

    public string FullPath => _fileInfo.FullName;
    public string Name => _fileInfo.Name;
    public string Extension => _fileInfo.Extension;
    public string DirectoryPath => _fileInfo.DirectoryName ?? string.Empty;
    public long Length => _fileInfo.Length;
    public DateTime CreatedUtc => _fileInfo.CreationTimeUtc;
    public DateTime LastModifiedUtc => _fileInfo.LastWriteTimeUtc;
    public bool IsReadOnly => _fileInfo.IsReadOnly;
}
