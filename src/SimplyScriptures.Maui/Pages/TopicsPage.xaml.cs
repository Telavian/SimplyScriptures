using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.ViewModels;

namespace SimplyScriptures.Pages;

public partial class TopicsPage : ContentPage
{
    #region Private Variables

    private readonly TopicsViewModel _viewModel;

    #endregion

    #region Constructors

    public TopicsPage(TopicsViewModel vm)
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

        _ = _viewModel.InitializeAsync()
            ;
    }

    #endregion Protected Methods

}