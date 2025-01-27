namespace ModShark.Services;

/// <summary>
/// Provides access to files on the local filesystem
/// </summary>
public interface IFileService
{
    /// <inheritdoc cref="File.OpenRead"/>
    Stream OpenRead(string path);
}

public class FileService : IFileService
{
    public Stream OpenRead(string path) => File.OpenRead(path);
}