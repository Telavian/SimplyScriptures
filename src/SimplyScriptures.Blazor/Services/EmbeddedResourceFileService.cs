using System.Collections.Concurrent;
using System.Reflection;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Services;

public class EmbeddedResourceFileService : IFileService
{
    private static readonly ConcurrentDictionary<string, byte[]> _dataLookup = new();

    public string DataRootDirectory => "";

    public async Task<byte[]> LoadDataAsync(string path)
    {
        var isFound = _dataLookup.TryGetValue(path, out var numArray);

        if (isFound && numArray != null)
        {
            return numArray;
        }

        var str = path;
        if (str.StartsWith("./"))
        {
            str = str[2..];
        }

        var name = "SimplyScriptures.wwwroot." + str.Replace('/', '.');
        var manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name) ?? throw new Exception("Unable to find resource '" + path + "'");
        await using (manifestResourceStream)
        {
            using var memStream = new MemoryStream();
            await manifestResourceStream.CopyToAsync(memStream);
            var array = memStream.ToArray();
            _dataLookup.TryAdd(path, array);
            return array;
        }
    }
}
