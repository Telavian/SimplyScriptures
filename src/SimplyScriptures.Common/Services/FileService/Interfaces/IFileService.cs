using SimplyScriptures.Common.Models;

namespace SimplyScriptures.Common.Services.FileService.Interfaces;

public interface IFileService
{
    public string DataRootDirectory { get; }
    public Task<byte[]> LoadDataAsync(string path);
}
