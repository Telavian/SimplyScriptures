using SimplyScriptures.ViewModels;

namespace SimplyScriptures.Pages;

public partial class MainPage : ContentPage
{
    #region Private Variables

    private readonly MainViewModel _viewModel;

    #endregion 

    #region Constructors

    public MainPage(MainViewModel vm)
    {
        _viewModel = vm;

        InitializeComponent();
        BindingContext = vm;
        NavigationPage.SetHasBackButton(this, false);
    }

    #endregion
}
