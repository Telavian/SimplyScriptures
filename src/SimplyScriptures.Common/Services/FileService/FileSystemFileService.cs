using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Common.Services.FileService;

public class FileSystemFileService : IFileService
{
    #region Private Variables

    #endregion

    public string DataRootDirectory { get; }

    #region Constructors

    public FileSystemFileService(string rootPath)
    {
        if (rootPath.EndsWith("/") == false)
        {
            rootPath += "/";
        }

        DataRootDirectory = rootPath;
    }

    #endregion

    #region Public Methods

    public Task<byte[]> LoadDataAsync(string path)
    {
        path = path.Replace("./", DataRootDirectory);
        return File.ReadAllBytesAsync(path)
            ;
    }

    #endregion
}
