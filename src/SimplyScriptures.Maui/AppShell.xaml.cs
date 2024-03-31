using SimplyScriptures.Pages;

namespace SimplyScriptures;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(DisplayPage), typeof(DisplayPage));
        Routing.RegisterRoute(nameof(TopicsPage), typeof(TopicsPage));
        Routing.RegisterRoute(nameof(DictionaryPage), typeof(DictionaryPage));
    }
}
