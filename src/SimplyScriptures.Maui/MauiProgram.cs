using Microsoft.Extensions.Caching.Memory;
using SimplyScriptures.Common.Services.FileService;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Pages;
using SimplyScriptures.Services;
using SimplyScriptures.ViewModels;
using Telerik.Maui.Controls.Compatibility;

namespace SimplyScriptures;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseTelerik()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Oregon_LDO_Extended_Black.ttf", "Oregon_LDO_Extended_Black");
            });

        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IFileService>(p => new CachingFileService(p.GetService<IMemoryCache>()!, new MauiFileService()));

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<DisplayViewModel>();
        builder.Services.AddSingleton<TopicsViewModel>();
        builder.Services.AddSingleton<DictionaryViewModel>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<DisplayPage>();
        builder.Services.AddSingleton<TopicsPage>();
        builder.Services.AddSingleton<DictionaryPage>();

        return builder.Build();
    }
}
