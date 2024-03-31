using Microsoft.Maui.Platform;

namespace SimplyScriptures;

public partial class App : Application
{
    public static bool IsDarkTheme => Application.Current != null &&
                                      Application.Current.UserAppTheme != AppTheme.Light && Application.Current.RequestedTheme != AppTheme.Light &&
                                      Application.Current.PlatformAppTheme != AppTheme.Light;

    public static string BaseDirectory { get; private set; }

    static App()
    {
        BaseDirectory = Directory.GetCurrentDirectory();
    }

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        //Application.Current.UserAppTheme = AppTheme.Light;
    }
}
