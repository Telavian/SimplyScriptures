using Android.App;
using Android.Runtime;

namespace SimplyScriptures;

[Application]
public class MainApplication(IntPtr handle, JniHandleOwnership ownership) : MauiApplication
{
    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp();
    }
}
