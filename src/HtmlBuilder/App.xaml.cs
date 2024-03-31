using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace HtmlBuilder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => HandleErrorAsync(args.ExceptionObject as Exception);

        CrystalPalette.LoadPreset(CrystalPalette.ColorVariation.Dark);
        CrystalPalette.Palette.WindowButtonsAlignment = ButtonsAlignment.Right;
    }

    private static void HandleErrorAsync(Exception? ex)
    {
        if (ex == null)
        {
            return;
        }

        var root = ex.GetBaseException();

        Console.WriteLine(root.Message);
    }
}
