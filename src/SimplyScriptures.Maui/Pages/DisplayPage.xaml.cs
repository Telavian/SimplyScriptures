using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Models;
using SimplyScriptures.ViewModels;
using Telerik.Maui.Controls;
using Bookmark = SimplyScriptures.Common.Models.Bookmark;

namespace SimplyScriptures.Pages;

[SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
public partial class DisplayPage : ContentPage
{
    #region Private Variables

    private readonly DisplayViewModel _viewModel;

    #endregion

    #region Constructors

    public DisplayPage(DisplayViewModel vm)
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
        _viewModel.HighlightColorPopup = HighlightColorPopup;
        _viewModel.BookmarksPopup = BookmarksPopup;
        _viewModel.MainContentWebView = MainContentWebView;

        _ = _viewModel.InitializeAsync()
            ;
    }

    #endregion Protected Methods

    #region Event Handlers

    [SuppressMessage("ReSharper", "AsyncApostle.AsyncAwaitMayBeElidedHighlighting")]
    private async void BookmarkDeleteButton_Clicked(object sender, EventArgs e)
    {
        await Task.Yield();

        //TODO: Not sure how to do this through bindings
        var item = (ImageButton)sender;
        var dataContext = item.BindingContext as MenuContentItem<Bookmark>;

        _viewModel.DeleteBookmarkAsyncCommand
            .Execute(dataContext);
    }

    #endregion

    private void MainContentWebView_OnNavigated(object? sender, WebNavigatedEventArgs e)
    {        
        Debug.WriteLine($"Initializing browser for url: {e.Url}");

        _viewModel.InitializePageFrameAsyncCommand
            .Execute(null);
    }
}
