using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Common.Services.FileService;

public class NetworkFileService(HttpClient httpClient) : IFileService
{
    #region Private Variables

    private readonly HttpClient _httpClient = httpClient;

    #endregion

    public string DataRootDirectory => ""; // Application root

    #region Constructors

    #endregion

    #region Public Methods

    public Task<byte[]> LoadDataAsync(string path)
    {
        //using (var request = new HttpRequestMessage(HttpMethod.Get, path))
        //{
        //    request.Headers.CacheControl = new CacheControlHeaderValue
        //    {
        //        MaxAge = TimeSpan.FromDays(30),
        //        Public = true,
        //    };

        //    request.SetBrowserRequestCache(BrowserRequestCache.ForceCache);
        //    using (var response = await _httpClient.SendAsync(request))
        //    {
        //        response.EnsureSuccessStatusCode();
        //        return await response.Content.ReadAsByteArrayAsync();
        //    }
        //}

        return _httpClient.GetByteArrayAsync(path);
    }

    #endregion
}
