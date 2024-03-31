using SimplyScriptures.Common.Services.FileService.Interfaces;

namespace SimplyScriptures.Common.Services.FileService;

public class NetworkFileService : IFileService
{
    #region Private Variables

    private readonly HttpClient _httpClient;

    #endregion

    public string DataRootDirectory => ""; // Application root

    #region Constructors

    public NetworkFileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

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
        //        return await response.Content.ReadAsByteArrayAsync()
        //            ;
        //    }
        //}
            
        return _httpClient.GetByteArrayAsync(path);
    }

    #endregion
}