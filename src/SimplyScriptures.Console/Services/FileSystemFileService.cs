using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Console.Services;

public class FileSystemFileService : IFileService
{
    #region Private Variables

    private readonly string _rootPath;

    #endregion

    public string DataRootDirectory => _rootPath;

    #region Constructors

    public FileSystemFileService(string rootPath)
    {
        if (rootPath.EndsWith("/") == false)
        {
            rootPath += "/";
        }

        _rootPath = rootPath;
    }

    #endregion

    #region Public Methods

    public Task<byte[]> LoadDataAsync(string path)
    {
        path = path.Replace("./", _rootPath);
        return File.ReadAllBytesAsync(path)
            ;
    }

    #endregion
}