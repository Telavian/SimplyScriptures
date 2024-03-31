using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Common.Services.FileService;

public class CachingFileService : IFileService
{
    private readonly IMemoryCache _cache;
    private readonly IFileService _innerService;

    public string DataRootDirectory => _innerService.DataRootDirectory;

    public CachingFileService(IMemoryCache cache, IFileService innerService)
    {
        _cache = cache;
        _innerService = innerService;
    }

    public async Task<byte[]> LoadDataAsync(string path)
    {
        var isFound = _cache.TryGetValue(path, out byte[]? data);
        if (isFound)
        {
            return data!;
        }

        data = await _innerService.LoadDataAsync(path)
            ;

        _cache.Set(path, data, TimeSpan.FromMinutes(5));
        return data;
    }
}
