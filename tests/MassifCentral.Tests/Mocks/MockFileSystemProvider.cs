using MassifCentral.Lib.Services;

namespace MassifCentral.Tests.Mocks;

/// <summary>
/// Mock implementation of IFileSystemProvider for unit testing.
/// Allows simulating directory structures without actual IO.
/// </summary>
public class MockFileSystemProvider : IFileSystemProvider
{
    private readonly Dictionary<string, MockDirectoryInfo> _directories = new();
    private readonly HashSet<string> _inaccessiblePaths = new();

    /// <summary>
    /// Initializes the mock file system with a root directory.
    /// </summary>
    public MockFileSystemProvider(string rootPath = "/root")
    {
        // Normalize root path
        var normalizedRoot = Path.GetFullPath(rootPath);
        _directories[normalizedRoot] = new MockDirectoryInfo(normalizedRoot, this);
    }

    /// <summary>
    /// Adds a virtual directory to the mock file system.
    /// </summary>
    public void AddDirectory(string path)
    {
        var normalizedPath = Path.GetFullPath(path);
        if (!_directories.ContainsKey(normalizedPath))
        {
            _directories[normalizedPath] = new MockDirectoryInfo(normalizedPath, this);
        }
    }

    /// <summary>
    /// Adds a virtual file to the mock file system.
    /// </summary>
    public void AddFile(string directoryPath, string fileName, long size, DateTime? createdUtc = null, DateTime? modifiedUtc = null, bool isReadOnly = false)
    {
        var normalizedPath = Path.GetFullPath(directoryPath);
        if (_directories.TryGetValue(normalizedPath, out var dir))
        {
            dir.AddFile(new MockFileInfo(
                Path.Combine(normalizedPath, fileName),
                fileName,
                size,
                createdUtc ?? DateTime.UtcNow,
                modifiedUtc ?? DateTime.UtcNow,
                isReadOnly
            ));
        }
    }

    /// <summary>
    /// Adds a subdirectory to a parent directory in the mock file system.
    /// </summary>
    public void AddSubdirectory(string parentPath, string directoryName)
    {
        var normalizedParent = Path.GetFullPath(parentPath);
        var subPath = Path.Combine(normalizedParent, directoryName);
        var normalizedSub = Path.GetFullPath(subPath);

        if (_directories.TryGetValue(normalizedParent, out var parent))
        {
            var subDir = new MockDirectoryInfo(normalizedSub, this);
            _directories[normalizedSub] = subDir;
            parent.AddSubdirectory(subDir);
        }
    }

    /// <summary>
    /// Marks a directory as inaccessible (access denied).
    /// </summary>
    public void MakeDirectoryInaccessible(string path)
    {
        var normalizedPath = Path.GetFullPath(path);
        _inaccessiblePaths.Add(normalizedPath);
    }

    /// <summary>
    /// Checks if a directory exists in the mock file system.
    /// </summary>
    public bool DirectoryExists(string path)
    {
        var normalizedPath = Path.GetFullPath(path);
        return _directories.ContainsKey(normalizedPath);
    }

    /// <summary>
    /// Gets directory information from the mock file system.
    /// </summary>
    public IDirectoryInfo GetDirectoryInfo(string path)
    {
        var normalizedPath = Path.GetFullPath(path);
        if (_directories.TryGetValue(normalizedPath, out var dir))
        {
            return dir;
        }
        throw new DirectoryNotFoundException($"Directory not found in mock: {path}");
    }

    /// <summary>
    /// Validates directory access (checks if marked as inaccessible).
    /// </summary>
    public void ValidateDirectoryAccess(string path)
    {
        var normalizedPath = Path.GetFullPath(path);
        if (_inaccessiblePaths.Contains(normalizedPath))
        {
            throw new UnauthorizedAccessException($"Access denied to directory: {path}");
        }
    }
}

/// <summary>
/// Mock implementation of IDirectoryInfo.
/// </summary>
internal class MockDirectoryInfo : IDirectoryInfo
{
    private readonly List<IFileInfo> _files = new();
    private readonly List<IDirectoryInfo> _subdirectories = new();
    private readonly MockFileSystemProvider _fileSystemProvider;

    public string FullPath { get; }

    public MockDirectoryInfo(string fullPath, MockFileSystemProvider fileSystemProvider)
    {
        FullPath = fullPath;
        _fileSystemProvider = fileSystemProvider;
    }

    internal void AddFile(IFileInfo file)
    {
        _files.Add(file);
    }

    internal void AddSubdirectory(IDirectoryInfo directory)
    {
        _subdirectories.Add(directory);
    }

    public IFileInfo[] GetFiles()
    {
        return _files.ToArray();
    }

    public IDirectoryInfo[] GetDirectories()
    {
        return _subdirectories.ToArray();
    }
}

/// <summary>
/// Mock implementation of IFileInfo.
/// </summary>
internal class MockFileInfo : IFileInfo
{
    public string FullPath { get; }
    public string Name { get; }
    public string Extension { get; }
    public string DirectoryPath { get; }
    public long Length { get; }
    public DateTime CreatedUtc { get; }
    public DateTime LastModifiedUtc { get; }
    public bool IsReadOnly { get; }

    public MockFileInfo(string fullPath, string name, long length, DateTime createdUtc, DateTime modifiedUtc, bool isReadOnly)
    {
        FullPath = fullPath;
        Name = name;
        Extension = Path.GetExtension(name);
        DirectoryPath = Path.GetDirectoryName(fullPath) ?? string.Empty;
        Length = length;
        CreatedUtc = createdUtc;
        LastModifiedUtc = modifiedUtc;
        IsReadOnly = isReadOnly;
    }
}
