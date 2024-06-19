using SimplyScriptures.Common.Extensions;
using SimplyScriptures.ViewModels;

namespace SimplyScriptures.Pages;

public partial class DictionaryPage : ContentPage
{
    #region Private Variables

    private readonly DictionaryViewModel _viewModel;

    #endregion

    #region Constructors

    public DictionaryPage(DictionaryViewModel vm)
    {
        _viewModel = vm;

        InitializeComponent();
        BindingContext = vm;
        NavigationPage.SetHasBackButton(this, false);
    }

    #endregion

    #region Protected Methods

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        _ = _viewModel.InitializeAsync();
    }

    #endregion Protected Methods
}
