using Blazored.LocalStorage;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.Extensions.Caching.Memory;
using MudBlazor.Services;
using SimplyScriptures.Common.Services.FileService;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Services;
using SimplyScriptures.Services.ApplicationState;
using SimplyScriptures.Services.ApplicationState.Interfaces;

namespace SimplyScriptures;

public class Program
{
    public static async Task Main(string[] args)
    {
        await Task.Yield();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        builder.Services.AddMudServices();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddClipboard();
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IFileService>(p => new CachingFileService(p.GetService<IMemoryCache>()!, new EmbeddedResourceFileService()));
        builder.Services.AddScoped<IApplicationStateService, ApplicationStateService>();

        var app = builder.Build();

        app.UseStaticFiles();
        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

        app.Run();
    }
}
