using System.Collections.Concurrent;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Services;

public class MauiFileService : IFileService
{
    public string DataRootDirectory => FileSystem.AppDataDirectory;

    public async Task<byte[]> LoadDataAsync(string path)
    {
        // Root path is Resources/Raw
        path = path.Replace("./", "");

        using var memStream = new MemoryStream();
        var fileStream = await FileSystem.OpenAppPackageFileAsync(path);
        await fileStream.CopyToAsync(memStream);

        memStream.Position = 0;
        return memStream.ToArray();
    }
}
