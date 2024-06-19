using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using MudBlazor;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Search.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Common.Services.TextSearch;
using SimplyScriptures.Common.Services.TextSearch.Models;
using SimplyScriptures.Pages.Common;
using SimplyScriptures.Pages.Models;
using SimplyScriptures.Services.ApplicationState.Interfaces;
using static MudBlazor.CategoryTypes;

namespace SimplyScriptures.Pages;

public class DisplayPageBase : ViewModelBase
{
    #region Private Variables

    private const string _zoomLevelSettingName = "ZoomLevel";

    [Inject]
    private NavigationManager? _navManager { get; set; }

    [Inject]
    private IJSRuntime? _jsRuntime { get; set; }

    [Inject]
    private IDialogService? _dialogService { get; set; }

    [Inject]
    private Blazored.LocalStorage.ILocalStorageService? _localStorage { get; set; }

    [Inject]
    private ClipboardService? _clipboard { get; set; }

    [Inject]
    private IFileService? _fileService { get; set; }

    [Inject]
    private IApplicationStateService? _applicationState { get; set; }

    private readonly ConcurrentDictionary<ScriptureBook, BookInfo> _bookInfoLookup = new();

    private TaskCompletionSource _browserInitialization = new();

    #endregion

    #region Public Properties

    public static TextSearchService? PageSearch { get; set; }
    public ScriptureBook[] AllScriptures { get; } = Enum.GetValues<ScriptureBook>().Where(x => x != ScriptureBook.None).ToArray();
    public Bookmark[] AllBookmarks { get; set; } = [];
    public MudMessageBox? BookmarksMessageBox { get; set; }
    public MudIconButton? HighlightColorButton { get; set; }
    public MudMessageBox? HighlightColorPopup { get; set; }
    public MudMenu? BrowserContextMenu { get; set; }
    public MudTabs? MenuTabs { get; set; }

    #region AllColors

    private string[] _allColors =
    [
        "transparent",
        "#FFFFFF",
        "#EE534F",
        "#AB47BC",
        "#7E57C2",
        "#5D6BC0",
        "#42A5F5",
        "#26C5DA",
        "#24A79A",
        "#66BB6A",
        "#9CCC65",
        "#D4E157",
        "#FFEE58",
        "#FFCA29",
        "#FFA726",
        "#FF7043",
        "#8D6E63",
        "#BDBDBD",
        "#78909C",
        "#3C3C3C",
        "#000000"
    ];

    public string[] AllColors
    {
        get => _allColors;
        set => UpdateProperty(ref _allColors, value);
    }

    #endregion AllColors

    #region AllHighlights

    private HighlightSelection[] _allHighlights = [];

    public HighlightSelection[] AllHighlights
    {
        get => _allHighlights;
        set => UpdateProperty(ref _allHighlights, value);
    }

    #endregion AllHighlights

    #region IsMenuOpen

    private bool _isMenuOpen = true;

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set => UpdateProperty(ref _isMenuOpen, value);
    }

    #endregion IsMenuOpen

    #region IsSearchOpen

    private bool _isSearchOpen;

    public bool IsSearchOpen
    {
        get => _isSearchOpen;
        set => UpdateProperty(ref _isSearchOpen, value,
            v =>
            {
                IsMenuOpen = IsSearchOpen || IsContentOpen;

                InvokeAsync(() => MenuTabs!.ActivatePanel(1));
            });
    }

    #endregion IsSearchOpen

    #region IsContentOpen

    private bool _isContentOpen;

    public bool IsContentOpen
    {
        get => _isContentOpen;
        set => UpdateProperty(ref _isContentOpen, value,
            v =>
            {
                IsMenuOpen = IsSearchOpen || IsContentOpen;
                InvokeAsync(() => MenuTabs!.ActivatePanel(0));
            });
    }

    #endregion IsContentOpen

    #region NewBookmarkName

    private string _newBookmarkName = "";

    public string NewBookmarkName
    {
        get => _newBookmarkName;
        set => UpdateProperty(ref _newBookmarkName, value);
    }

    #endregion NewBookmarkName

    #region SearchText

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set => UpdateProperty(ref _searchText, value);
    }

    #endregion SearchText

    #region ContentFilterText

    private string _contentFilterText = "";

    public string ContentFilterText
    {
        get => _contentFilterText;
        set => UpdateProperty(ref _contentFilterText, value,
            FilterContentItems);
    }

    #endregion ContentFilterText

    #region CurrentBook

    private BookInfo? _currentBook = null;

    public BookInfo? CurrentBook
    {
        get => _currentBook;
        set => UpdateProperty(ref _currentBook, value);
    }

    #endregion CurrentBook

    #region IsSearchBusy

    private bool _isSearchBusy;

    public bool IsSearchBusy
    {
        get => _isSearchBusy;
        set => UpdateProperty(ref _isSearchBusy, value);
    }

    #endregion IsSearchBusy

    #region IsSearchInitializing

    private bool _isSearchInitializing;

    public bool IsSearchInitializing
    {
        get => _isSearchInitializing;
        set => UpdateProperty(ref _isSearchInitializing, value);
    }

    #endregion IsSearchInitializing

    #region SelectedScripture

    private ScriptureBook _selectedScripture = ScriptureBook.BM;

    public ScriptureBook SelectedScripture
    {
        get => _selectedScripture;
        set => UpdateProperty(ref _selectedScripture, value);
    }

    #endregion SelectedScripture

    #region SelectedScriptureBook

    private ScriptureBook _selectedScriptureBook = ScriptureBook.BM_About;

    public ScriptureBook SelectedScriptureBook
    {
        get => _selectedScriptureBook;
        set => UpdateProperty(ref _selectedScriptureBook, value);
    }

    #endregion SelectedScriptureBook

    #region SelectedHighlightColor

    private string? _selectedHighlightColor = "transparent";

    public string? SelectedHighlightColor
    {
        get => _selectedHighlightColor;
        set => UpdateProperty(ref _selectedHighlightColor, value);
    }

    #endregion SelectedHighlightColor

    #region IsDisplayInverted

    private bool _isDisplayInverted = false;

    public bool IsDisplayInverted
    {
        get => _isDisplayInverted;
        set => UpdateProperty(ref _isDisplayInverted, value,
            v => ProcessInvertedStatusAsync(v));
    }

    #endregion IsDisplayInverted

    #region SearchResults

    private SearchResults? _searchResults = null;

    public SearchResults? SearchResults
    {
        get => _searchResults;
        set => UpdateProperty(ref _searchResults, value);
    }

    #endregion SearchResults

    #region SelectedSearchMatch

    private SearchMatch? _selectedSearchMatch = null;

    public SearchMatch? SelectedSearchMatch
    {
        get => _selectedSearchMatch;
        set => UpdateProperty(ref _selectedSearchMatch, value);
    }

    #endregion SelectedSearchMatch

    #region SelectedNavigationItem

    private ContentItem? _selectedNavigationItem = null;

    public ContentItem? SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set => UpdateProperty(ref _selectedNavigationItem, value);
    }

    #endregion SelectedNavigationItem

    #region ToggleMenuVisibilityAsyncCommand

    private Func<Task>? _toggleMenuVisibilityAsyncCommand;

    public Func<Task> ToggleMenuVisibilityAsyncCommand => _toggleMenuVisibilityAsyncCommand ??= CreateEventCallbackAsyncCommand(ToggleMenuVisibilityAsync, "Unable to toggle menu visibility");

    #endregion ToggleMenuVisibilityAsyncCommand

    #region NavItemSelectedAsyncCommand

    private Func<ContentItem, Task>? _navItemSelectedAsyncCommand;

    public Func<ContentItem, Task> NavItemSelectedAsyncCommand => _navItemSelectedAsyncCommand ??= CreateEventCallbackAsyncCommand<ContentItem>(NavItemSelectedAsync, "Unable to select navigation item");

    #endregion NavItemSelectedAsyncCommand

    #region HandleSearchKeypressAsyncCommand

    private Func<KeyboardEventArgs, Task>? _handleSearchKeypressAsyncCommand;

    public Func<KeyboardEventArgs, Task> HandleSearchKeypressAsyncCommand => _handleSearchKeypressAsyncCommand ??= CreateEventCallbackAsyncCommand<KeyboardEventArgs>(HandleSearchKeypressAsync, "Unable to handle keypress");

    #endregion HandleSearchKeypressAsyncCommand

    #region HandleBookmarkNameKeypressAsyncCommand

    private Func<KeyboardEventArgs, Task>? _handleBookmarkNameKeypressAsyncCommand;

    public Func<KeyboardEventArgs, Task> HandleBookmarkNameKeypressAsyncCommand => _handleBookmarkNameKeypressAsyncCommand ??= CreateEventCallbackAsyncCommand<KeyboardEventArgs>(HandleBookmarkNameKeypressAsync, "Unable to handle keypress");

    #endregion HandleBookmarkNameKeypressAsyncCommand

    #region SearchAsyncCommand

    private Func<Task>? _searchAsyncCommand;

    public Func<Task> SearchAsyncCommand => _searchAsyncCommand ??= CreateEventCallbackAsyncCommand(SearchAsync, "Unable to search");

    #endregion SearchAsyncCommand

    #region SearchMatchSelectedAsyncCommand

    private Func<SearchMatch, Task>? _searchMatchSelectedAsyncCommand;

    public Func<SearchMatch, Task> SearchMatchSelectedAsyncCommand => _searchMatchSelectedAsyncCommand ??= CreateEventCallbackAsyncCommand<SearchMatch>(SearchMatchSelectedAsync, "Unable to select search match");

    #endregion SearchMatchSelectedAsyncCommand

    #region SelectScriptureBookAsyncCommand

    private Func<ScriptureBook, Task>? _selectScriptureBookAsyncCommand;

    public Func<ScriptureBook, Task> SelectScriptureBookAsyncCommand => _selectScriptureBookAsyncCommand ??= CreateEventCallbackAsyncCommand<ScriptureBook>(SelectScriptureBookAsync, "Unable to select book");

    #endregion SelectScriptureBookAsyncCommand

    #region ShowPrintableScripturesAsyncCommand

    private Func<MouseEventArgs, Task>? _showPrintableScripturesAsyncCommand;

    public Func<MouseEventArgs, Task> ShowPrintableScripturesAsyncCommand => _showPrintableScripturesAsyncCommand ??= CreateEventCallbackAsyncCommand<MouseEventArgs>(ShowPrintableScripturesAsync, "Unable to show printable scriptures");

    #endregion ShowPrintableScripturesAsyncCommand

    #region ShowBookmarksAsyncCommand

    private Func<Task>? _showBookmarksAsyncCommand;

    public Func<Task> ShowBookmarksAsyncCommand => _showBookmarksAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowBookmarksAsync, "Unable to show bookmarks");

    #endregion ShowBookmarksAsyncCommand

    #region DeleteBookmarkAsyncCommand

    private Func<Bookmark, Task>? _deleteBookmarkAsyncCommand;

    public Func<Bookmark, Task> DeleteBookmarkAsyncCommand => _deleteBookmarkAsyncCommand ??= CreateEventCallbackAsyncCommand<Bookmark>(DeleteBookmarkAsync, "Unable to delete bookmark");

    #endregion DeleteBookmarkAsyncCommand

    #region AddBookmarkAsyncCommand

    private Func<Task>? _addBookmarkAsyncCommand;

    public Func<Task> AddBookmarkAsyncCommand => _addBookmarkAsyncCommand ??= CreateEventCallbackAsyncCommand(AddBookmarkAsync, "Unable to add bookmark");

    #endregion AddBookmarkAsyncCommand

    #region DisplayBookmarkAsyncCommand

    private Func<Bookmark, Task>? _displayBookmarkAsyncCommand;

    public Func<Bookmark, Task> DisplayBookmarkAsyncCommand => _displayBookmarkAsyncCommand ??= CreateEventCallbackAsyncCommand<Bookmark>(DisplayBookmarkAsync, "Unable to display bookmark");

    #endregion DisplayBookmarkAsyncCommand

    #region ZoomInAsyncCommand

    private Func<Task>? _zoomInAsyncCommand;

    public Func<Task> ZoomInAsyncCommand => _zoomInAsyncCommand ??= CreateEventCallbackAsyncCommand(ZoomInAsync, "Unable to zoom in");

    #endregion ZoomInAsyncCommand

    #region ZoomOutAsyncCommand

    private Func<Task>? _zoomOutAsyncCommand;

    public Func<Task> ZoomOutAsyncCommand => _zoomOutAsyncCommand ??= CreateEventCallbackAsyncCommand(ZoomOutAsync, "Unable to zoom out");

    #endregion ZoomOutAsyncCommand

    #region ApplyHighlightAsyncCommand

    private Func<Task>? _applyHighlightAsyncCommand;

    public Func<Task> ApplyHighlightAsyncCommand => _applyHighlightAsyncCommand ??= CreateEventCallbackAsyncCommand(ApplyHighlightAsync, "Unable to apply highlight");

    #endregion ApplyHighlightAsyncCommand

    #region RemoveHighlightAsyncCommand

    private Func<Task>? _removeHighlightAsyncCommand;

    public Func<Task> RemoveHighlightAsyncCommand => _removeHighlightAsyncCommand ??= CreateEventCallbackAsyncCommand(RemoveHighlightAsync, "Unable to remove highlight");

    #endregion RemoveHighlightAsyncCommand

    #region CopyTextAsyncCommand

    private Func<Task>? _copyTextAsyncCommand;

    public Func<Task> CopyTextAsyncCommand => _copyTextAsyncCommand ??= CreateEventCallbackAsyncCommand(CopyTextAsync, "Unable to copy text");

    #endregion CopyTextAsyncCommand

    #region CopyLinkAsyncCommand

    private Func<Task>? _copyLinkAsyncCommand;

    public Func<Task> CopyLinkAsyncCommand => _copyLinkAsyncCommand ??= CreateEventCallbackAsyncCommand(CopyLinkAsync, "Unable to copy link");

    #endregion CopyLinkAsyncCommand

    #region ShowHighlightColorsAsyncCommand

    private Func<Task>? _showHighlightColorsAsyncCommand;

    public Func<Task> ShowHighlightColorsAsyncCommand => _showHighlightColorsAsyncCommand ??= CreateEventCallbackAsyncCommand(ShowHighlightColorsAsync, "Unable to show highlight colors");

    #endregion ShowHighlightColorsAsyncCommand

    #region SetSelectedHighlightColorAsyncCommand

    private Func<string, Task>? _setSelectedHighlightColorAsyncCommand;

    public Func<string, Task> SetSelectedHighlightColorAsyncCommand => _setSelectedHighlightColorAsyncCommand ??= CreateEventCallbackAsyncCommand<string>(SetSelectedHighlightColorAsync, "Unable to set selected highlight color");

    #endregion SetSelectedHighlightColorAsyncCommand

    #region InitializePageFrameAsyncCommand

    private Func<Task>? _initializePageFrameAsyncCommand;

    public Func<Task> InitializePageFrameAsyncCommand => _initializePageFrameAsyncCommand ??= CreateEventCallbackAsyncCommand(InitializePageFrameAsync, "Unable to initialize page frame");

    #endregion InitializePageFrameAsyncCommand

    #endregion

    #region Protected Methods

    protected override async Task OnInitializedAsync()
    {
        Console.WriteLine("Initializing display page");
        await base.OnInitializedAsync();

        _browserInitialization.SetResult();
        PageSearch ??= new TextSearchService(_fileService!, RefreshAsync);
        SetPageState();
        Console.WriteLine("Display page initialization complete");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                Console.WriteLine("First render initialization");

                await _applicationState!.LoadCurrentStateAsync();

                SetPageState();
                await RefreshAsync();

                Console.WriteLine("Initializing search");
                await InitializeSearchAsync();

                Console.WriteLine("Processing page parameters");
                await ProcessPageParametersAsync();

                Console.WriteLine("Loading highlights");
                await LoadHighlightsAsync();

                Console.WriteLine("Loading bookmarks");
                await LoadBookmarksAsync();

                Console.WriteLine("Alerting if mobile");
                await AlertIfMobileAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to initialize: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("First render initialization complete");
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected string CleanMenuName(string name)
    {
        return HttpUtility.HtmlDecode(name);
    }

    protected string ConvertItemToLink(SearchMatch item)
    {
        var p = new DisplayPageParameters
        {
            Book = item.Book,
            XPaths = [item.XPath],
        };

        var root = new Uri(_navManager!.BaseUri, UriKind.Absolute);
        return p.ConvertToLink(root);
    }

    protected string ConvertItemToLink(ContentItem item)
    {
        var p = new DisplayPageParameters
        {
            Book = item.Book,
            XPaths = [item.XPath],
        };

        var root = new Uri(_navManager!.BaseUri, UriKind.Absolute);
        return p.ConvertToLink(root);
    }

    protected string MenuItemFontWeight(ContentItem? item)
    {
        return item?.Children != null && item.Children.Length > 0
            ? "font-weight: bold; "
            : "";
    }

    #endregion

    #region Private Methods

    private Task ToggleMenuVisibilityAsync()
    {
        IsMenuOpen = !IsMenuOpen;
        return RefreshAsync();
    }

    private async Task NavItemSelectedAsync(ContentItem item)
    {
        Console.WriteLine("Navigating to: " + item.Name + ", " + item.XPath);
        SetSelectedScripture(item.Book);
        SelectedNavigationItem = item;

        await RefreshAsync();
        await LoadCurrentBookAsync(item.Book);
        await HighlightXPathLocationsAsync([item.XPath], false);
    }

    private Task HandleSearchKeypressAsync(KeyboardEventArgs args)
    {
        return args.Code is "Enter" or "Return" or "NumpadEnter" ? SearchAsyncCommand!() : Task.CompletedTask;
    }

    private Task HandleBookmarkNameKeypressAsync(KeyboardEventArgs args)
    {
        return args.Code == "Enter" || args.Code == "Return" || args.Code == "NumpadEnter" ||
            args.Key == "Enter" || args.Key == "Return" || args.Key == "NumpadEnter"
            ? AddBookmarkAsyncCommand!()
            : Task.CompletedTask;
    }

    private async Task SearchAsync()
    {
        await Task.Yield();
        await SearchForTextAsync(SearchText ?? "");
    }

    private async Task<bool> SearchForTextAsync(string? text)
    {
        await Task.Yield();
        text = (text ?? "").Trim();

        if (text.StartsWith('"') && text.EndsWith('"'))
        {
            await FindExactMatchesAsync(text);
            await RefreshAsync();
            return false;
        }

        var isScriptureMatch = await FindScriptureMatchesAsync(text);

        if (isScriptureMatch == false)
        {
            await FindPhraseMatchesAsync(text);
        }

        await RefreshAsync();
        return isScriptureMatch;
    }

    private async Task LoadCurrentBookAsync(ScriptureBook book)
    {
        var timer = Stopwatch.StartNew();
        Console.WriteLine($"Loading book: {book}");

        book = book.ToSpecificBook();
        var bookItem = await LoadScriptureBookAsync(book);

        if (CurrentBook != null && CurrentBook.HtmlPath == bookItem.HtmlPath)
        {
            return;
        }

        _browserInitialization = new TaskCompletionSource();
        CurrentBook = bookItem;
        SetSelectedScripture(book);
        FilterContentItems(ContentFilterText);

        await Task.Yield();
        await RefreshAsync();

        // Wait for the initialization to complete
        await Task.WhenAny(_browserInitialization.Task, Task.Delay(5000));

        Console.WriteLine($"Book loaded: {timer.ElapsedMilliseconds} ms");
    }

    private async Task<BookInfo> LoadScriptureBookAsync(ScriptureBook book)
    {
        var isFound = _bookInfoLookup.TryGetValue(book, out var result);

        if (isFound && result != null)
        {
            return result;
        }

        Console.WriteLine($"Loading book info: {book}");
        var htmlPath = book.ToHtmlPath();
        var jsonPath = book.ToMenuContentPath();

        Console.WriteLine($"jsonPath: {jsonPath}");
        var contentData = await _fileService!.LoadDataAsync(jsonPath);

        var contentItems = contentData.DeserializeFromJson<ContentItem[]>()
                           ?? [];

        AssignNodeParents(contentItems);

        var item = new BookInfo()
        {
            Book = book,
            HtmlPath = htmlPath,
            ContentItems = contentItems,
        };

        _bookInfoLookup[book] = item;
        return item;
    }

    private static void AssignNodeParents(ContentItem[] contentItems)
    {
        foreach (var node in contentItems)
        {
            AssignNodeParents(node, null);
        }
    }

    private static void AssignNodeParents(ContentItem node, ContentItem? parent)
    {
        node.Parent = parent;

        foreach (var child in node.Children)
        {
            AssignNodeParents(child, node);
        }
    }

    private async Task<bool> FindExactMatchesAsync(string search)
    {
        var matches = await ExecuteSearchAsync(() => PageSearch!.FindExactMatchesAsync(search));

        SearchResults = new SearchResults
        {
            AllMatches = matches,
            MatchMode = matches.Length > 0
                ? SearchMatchMode.SearchMatches
                : SearchMatchMode.NoMatches
        };

        Console.WriteLine($"Found {matches.Length} exact matches");
        await RefreshAsync();

        return matches.Length > 0;
    }

    private async Task<bool> FindPhraseMatchesAsync(string search)
    {
        var matches = await ExecuteSearchAsync(() => PageSearch!.FindPhraseMatchesAsync(search));

        SearchResults = new SearchResults
        {
            AllMatches = matches,
            MatchMode = matches.Length > 0
                ? SearchMatchMode.SearchMatches
                : SearchMatchMode.NoMatches
        };

        Console.WriteLine($"Found {matches.Length} exact matches");
        await RefreshAsync();
        return matches.Length > 0;
    }

    private async Task<bool> FindScriptureMatchesAsync(string search)
    {
        var matches = await ExecuteSearchAsync(() => PageSearch!.FindScriptureMatchesAsync(search));

        if (matches.Length > 0)
        {
            var xpaths = matches
                .Select(x => x.XPath)
                .ToArray();

            await SearchMatchSelectedAsync(matches[0]);
            await HighlightXPathLocationsAsync(xpaths, true);
        }

        SearchResults = new SearchResults
        {
            AllMatches = matches,
            MatchMode = matches.Length > 0
                ? SearchMatchMode.ScriptureMatch
                : SearchMatchMode.NoMatches
        };

        Console.WriteLine($"Found {matches.Length} scripture matches");
        await RefreshAsync();
        return matches.Length > 0;
    }

    private async Task SearchMatchSelectedAsync(SearchMatch item)
    {
        await LoadCurrentBookAsync(item.Book);
        await Task.Yield();
        await HighlightXPathLocationsAsync([item.XPath], true);

        SelectedSearchMatch = item;
    }

    private async Task ScrollToLocationAsync(int location)
    {
        for (var x = 0; x < 50; x++)
        {
            try
            {
                var result = await _jsRuntime
                    !.InvokeAsync<bool>("setCurrentFrameScrollLocation", location)
                    .ConfigureAwait(false);

                if (result)
                {
                    return;
                }
            }
            catch
            {
                // Nothing
            }

            await Task.Delay(100);
        }
    }

    private async Task HighlightXPathLocationsAsync(string[] xpath, bool isHighlight)
    {
        var json = xpath.SerializeToJson();

        for (var x = 0; x < 50; x++)
        {
            try
            {
                var result = await _jsRuntime
                    !.InvokeAsync<bool>("highlightSearchResults", json, isHighlight)
                    .ConfigureAwait(false);

                if (result)
                {
                    return;
                }
            }
            catch
            {
                // Nothing
            }

            await Task.Delay(100);
        }
    }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    private async Task ProcessPageParametersAsync()
    {
        try
        {
            Uri.TryCreate(_navManager!.Uri ?? "", UriKind.Absolute, out var uri);

            var queryParameters = HttpUtility.ParseQueryString(uri?.Query ?? "");
            var book = queryParameters.Get("s");
            var searchText = queryParameters.Get("t");
            var location = queryParameters.Get("l");
            var xpaths = queryParameters.Get("x") ?? "";

            Enum.TryParse<ScriptureBook>(book, out var bookParam);
            int.TryParse(location, out var locationParam);

            if (bookParam == ScriptureBook.None)
            {
                bookParam = ScriptureBook.BM_About;
            }

            var displayParams = new DisplayPageQueryParameters
            {
                ScriptureParam = bookParam,
                SearchText = searchText ?? "",
                LocationParam = locationParam,
                XPathsParam = xpaths,
            };

            await ProcessPageParametersAsync(displayParams);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process page parameters: {ex.Message}");
        }
    }

    private async Task ProcessPageParametersAsync(DisplayPageQueryParameters queryParameters)
    {
        Console.WriteLine($"Page parameters: {queryParameters}");

        var isBookLoaded = false;
        var isSearchQuery = false;
        if (string.IsNullOrWhiteSpace(queryParameters.SearchText) == false)
        {
            IsContentOpen = false;
            IsSearchOpen = true;
            SearchText = queryParameters.SearchText;
            isSearchQuery = true;

            isBookLoaded = await SearchForTextAsync(queryParameters.SearchText);
        }

        if (isBookLoaded == false)
        {
            await ProcessBookParameterAsync(queryParameters.ScriptureParam, isSearchQuery);
        }

        await ProcessLocationParameterAsync(queryParameters.LocationParam);
        await ProcessXPathsParameterAsync(queryParameters.XPathsParam);
        await RefreshAsync();
    }

    private async Task ProcessBookParameterAsync(ScriptureBook book, bool isSearchQuery)
    {
        if (book == ScriptureBook.None)
        {
            book = ScriptureBook.BM_About;
        }

        try
        {
            SetSelectedScripture(book);

            if (isSearchQuery == false)
            {
                IsSearchOpen = false;
                IsContentOpen = false;
            }

            await Task.Yield();

            await LoadCurrentBookAsync(book);

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process book parameter: {ex.Message}");
        }
    }

    private async Task ProcessLocationParameterAsync(int location)
    {
        if (location <= 0)
        {
            return;
        }

        try
        {
            await ScrollToLocationAsync(location);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process location parameter: {ex.Message}");
        }
    }

    private async Task ProcessXPathsParameterAsync(string xpaths)
    {
        if (string.IsNullOrWhiteSpace(xpaths))
        {
            return;
        }

        try
        {
            var decompressed = await xpaths.DecompressTextAsync();
            var decoded = HttpUtility.UrlDecode(decompressed);
            var locations = decoded.DeserializeFromJson<string[]>() ?? [];

            await HighlightXPathLocationsAsync(locations, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process xpath parameter: {ex.Message}");
        }
    }

    private async Task ApplySavedZoomSettingAsync()
    {
        try
        {
            var zoomLevel = await LoadDoubleSettingAsync(_zoomLevelSettingName, 1.0);

            await _jsRuntime!.InvokeAsync<object>("applyZoom", zoomLevel)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to apply zoom level: {ex.Message}");
        }
    }

    private async Task SelectScriptureBookAsync(ScriptureBook book)
    {
        IsSearchOpen = false;
        IsContentOpen = true;
        SetSelectedScripture(book);

        await LoadCurrentBookAsync(book);
        await RefreshAsync();
    }

    private void FilterContentItems(string filter)
    {
        var contentItems = CurrentBook?.ContentItems ?? [];
        filter = (filter ?? "").Trim();

        // Show all
        if (string.IsNullOrWhiteSpace(filter))
        {
            contentItems
                .TraverseItems(x => x.Children)
                .ForEach(x => x.IsVisible = true);
        }
        else
        {
            // Hide all
            contentItems
                .TraverseItems(x => x.Children)
                .ForEach(x => x.IsVisible = false);

            contentItems
                .TraverseItems(x => x.Children)
                .ForEach(x => ProcessContentItemFilter(x, filter));
        }
    }

    private static void ProcessContentItemFilter(ContentItem item, string filter)
    {
        var isVisible = item.Name
            .Contains(filter, StringComparison.OrdinalIgnoreCase);
        item.IsVisible = isVisible;

        if (isVisible == false)
        {
            return;
        }

        // Make path to root visible
        var node = item.Parent;
        while (node != null)
        {
            // Exit early if this path already shown
            if (node.IsVisible)
            {
                break;
            }

            node.IsVisible = true;
            node = node.Parent;
        }
    }

    private async Task ShowPrintableScripturesAsync(MouseEventArgs args)
    {
        if (_jsRuntime == null)
        {
            return;
        }

        // A cheat to aid in setting up scripture links
        if (args.ShiftKey && args.CtrlKey)
        {
            await DisplayCurrentSelectionInfoAsync();

            return;
        }

        var url = SelectedScripture.ToPDFPath();
        await _jsRuntime.InvokeAsync<object>("open", url, "_blank")
            .ConfigureAwait(false);
    }

    private async Task DisplayCurrentSelectionInfoAsync()
    {
        var text = await _jsRuntime!.InvokeAsync<string>("getSelectedText")
            .ConfigureAwait(false);
        var location = await _jsRuntime!.InvokeAsync<int>("getCurrentFrameScrollLocation")
            .ConfigureAwait(false);
        var xpaths = await _jsRuntime!.InvokeAsync<string[]>("getSelectedXPaths")
            .ConfigureAwait(false);

        var item = new
        {
            Book = SelectedScriptureBook.ToString(),
            Text = text,
            Location = location,
            XPaths = xpaths,
            Verse = "",
        };

        var json = item.SerializeToJson();
        var isSupported = await InvokeAsync(async () => await _clipboard!.IsSupportedAsync()
);

        if (isSupported == false)
        {
            return;
        }

        await InvokeAsync(async () => await _clipboard!.WriteTextAsync(json)
);

        await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Copy item", "Item copied to clipboard")
);
    }

    private async Task ZoomInAsync()
    {
        var currentZoom = await _jsRuntime!.InvokeAsync<double>("zoomIn")
            .ConfigureAwait(false);

        await SaveSettingAsync(_zoomLevelSettingName, currentZoom);
        await RefreshAsync();
    }

    private async Task ZoomOutAsync()
    {
        var currentZoom = await _jsRuntime!.InvokeAsync<double>("zoomOut")
            .ConfigureAwait(false);

        await SaveSettingAsync(_zoomLevelSettingName, currentZoom);
        await RefreshAsync();
    }

    private async Task InitializeSearchAsync()
    {
        IsSearchInitializing = true;
        await RefreshAsync();
        await Task.Delay(15);

        await PageSearch!.InitializeAsync();

        IsSearchInitializing = false;
        await RefreshAsync();
        await Task.Delay(15);
    }

    private async Task<SearchMatch[]> ExecuteSearchAsync(Func<Task<SearchMatch[]>> action)
    {
        IsSearchBusy = true;
        await RefreshAsync();
        await Task.Delay(15);

        var matches = await action();

        IsSearchBusy = false;
        await RefreshAsync();
        await Task.Delay(15);

        return matches;
    }

    private Task ShowBookmarksAsync()
    {
        return BookmarksMessageBox!.Show();
    }

    private async Task DeleteBookmarkAsync(Bookmark bookmark)
    {
        var confirm = await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Delete bookmark?", $"Delete {bookmark.Name}?")
);

        if (confirm != true)
        {
            return;
        }

        AllBookmarks = AllBookmarks
            .Except(new[] { bookmark })
            .ToArray();
        await SaveBookmarksAsync();

        // TODO: MudBlazor doesn't remove them automatically
        await _jsRuntime!.InvokeAsync<object>("removeBookmarkById", bookmark.BookmarkId.ToString())
            .ConfigureAwait(false);

        await RefreshAsync();
    }

    private async Task AddBookmarkAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBookmarkName))
        {
            return;
        }

        var location = await _jsRuntime!.InvokeAsync<int>("getCurrentFrameScrollLocation")
            .ConfigureAwait(false);

        var bookmark = new Bookmark
        {
            Name = NewBookmarkName,
            Book = SelectedScriptureBook,
            Location = location
        };

        NewBookmarkName = "";
        AllBookmarks = [.. AllBookmarks, .. new[] { bookmark }];
        await SaveBookmarksAsync();

        await InvokeAsync(() => BookmarksMessageBox?.Close());
        await RefreshAsync();
    }

    private async Task DisplayBookmarkAsync(Bookmark bookmark)
    {
        await InvokeAsync(() => BookmarksMessageBox?.Close());

        if (SelectedScriptureBook != bookmark.Book)
        {
            SetSelectedScripture(bookmark.Book);

            await LoadCurrentBookAsync(bookmark.Book);
        }

        await _jsRuntime!.InvokeAsync<object>("setCurrentFrameScrollLocation", bookmark.Location)
            .ConfigureAwait(false);

        await RefreshAsync();
    }

    private async Task SaveBookmarksAsync()
    {
        var json = AllBookmarks.SerializeToJson();
        await _localStorage!.SetItemAsStringAsync("bookmarks.json", json)
            .ConfigureAwait(false);
    }

    private async Task LoadBookmarksAsync()
    {
        var bookmarks = await ReadBookmarksAsync();

        AllBookmarks = [.. bookmarks.OrderBy(x => x.Name),];

        await RefreshAsync();
    }

    private async Task<Bookmark[]> ReadBookmarksAsync()
    {
        var exists = await _localStorage!.ContainKeyAsync("bookmarks.json")
            .ConfigureAwait(false);

        if (exists == false)
        {
            return [];
        }

        var json = await _localStorage.GetItemAsStringAsync("bookmarks.json")
            .ConfigureAwait(false);
        return json.DeserializeFromJson<Bookmark[]>() ?? [];
    }

    private async Task ProcessInvertedStatusAsync(bool isInverted)
    {
        _applicationState!.IsDisplayInverted = isInverted;

        await ApplyHighlightsForBookAsync();
        await _jsRuntime!.InvokeAsync<object>("initializePageFrame", IsDisplayInverted)
            .ConfigureAwait(false);
    }

    private Task ShowHighlightColorsAsync()
    {
        return HighlightColorPopup!.Show();
    }

    private Task SetSelectedHighlightColorAsync(string color)
    {
        HighlightColorPopup!.Close();
        SelectedHighlightColor = color;

        return RefreshAsync();
    }

    private async Task ApplyHighlightAsync()
    {
        var highlight = await GetHighlightSelectionsAsync();

        if (highlight == null)
        {
            return;
        }

        await UpdateSelectionHighlightsAsync(highlight);
        await SaveCurrentHighlightsAsync();
    }

    private async Task RemoveHighlightAsync()
    {
        var highlight = await GetHighlightSelectionsAsync();

        if (highlight == null)
        {
            return;
        }

        await RemoveSelectionHighlightsAsync(highlight);
        await SaveCurrentHighlightsAsync();
    }

    private async Task<HighlightSelection?> GetHighlightSelectionsAsync()
    {
        var selections = await _jsRuntime!.InvokeAsync<string[]>("getSelectedXPaths")
            .ConfigureAwait(false);

        if (selections.Length == 0)
        {
            return null;
        }

        var validSelections = selections
            .Select(x => x.Replace('"', '\''))
            .ToArray();

        var color = SelectedHighlightColor == "transparent"
            ? ""
            : SelectedHighlightColor ?? "";

        return new HighlightSelection
        {
            Book = SelectedScriptureBook,
            Color = color,
            XPath = validSelections
        };
    }

    private async Task RemoveSelectionHighlightsAsync(HighlightSelection selection)
    {
        await selection.XPath
            .ForAllAsync(async item => await _jsRuntime!.InvokeAsync<object>("updateHighlightColor", item, "")
                    .ConfigureAwait(false));

        var matches = AllHighlights
            .Where(x => x.Book == selection.Book && x.XPath.Any(y => selection.XPath.Contains(y)))
            .ToArray();

        AllHighlights = AllHighlights
            .Except(matches)
            .ToArray();

        await RefreshAsync();
    }

    private async Task UpdateSelectionHighlightsAsync(HighlightSelection selection)
    {
        await RemoveSelectionHighlightsAsync(selection);
        await AddSelectionHighlightsAsync(selection);
    }

    private async Task AddSelectionHighlightsAsync(HighlightSelection selection)
    {
        await selection.XPath
            .ForAllAsync(async item =>
            {
                var colorText = IsDisplayInverted
                    ? $"{selection.Color.ToInverseColor()}66"
                    : $"{selection.Color}66";

                await _jsRuntime!.InvokeAsync<object>("updateHighlightColor", item, colorText)
                    .ConfigureAwait(false);
            });

        AllHighlights = [.. AllHighlights, .. new[] { selection }];
        await RefreshAsync();
    }

    private Task ApplyHighlightsForBookAsync()
    {
        var highlights = GetCurrentBookHighlights();

        return highlights
            .ForAllAsync(async selection => await AddSelectionHighlightsAsync(selection));
    }

    private HighlightSelection[] GetCurrentBookHighlights()
    {
        return AllHighlights
            .Where(x => x.Book == SelectedScriptureBook)
            .ToArray();
    }

    private async Task SaveCurrentHighlightsAsync()
    {
        var json = AllHighlights.SerializeToJson();
        await _localStorage!.SetItemAsStringAsync(nameof(AllHighlights), json)
            .ConfigureAwait(false);
    }

    private async Task LoadHighlightsAsync()
    {
        var highlights = await ReadHighlightsAsync();

        Console.WriteLine($"Loaded {highlights.Length} highlights");
        AllHighlights = highlights;
    }

    private async Task<HighlightSelection[]> ReadHighlightsAsync()
    {
        var highlights = await _localStorage!.GetItemAsStringAsync(nameof(AllHighlights))
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(highlights) ? ([]) : highlights.DeserializeFromJson<HighlightSelection[]>() ?? [];
    }

    private async Task CopyTextAsync()
    {
        var text = await _jsRuntime!.InvokeAsync<string>("getSelectedText")
            .ConfigureAwait(false);

        var isSupported = await InvokeAsync(async () => await _clipboard!.IsSupportedAsync()
);

        if (isSupported == false)
        {
            return;
        }

        await InvokeAsync(async () => await _clipboard!.WriteTextAsync(text));
        await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Copy item", "Item copied to clipboard"));
    }

    private async Task CopyLinkAsync()
    {
        var isSupported = await InvokeAsync(async () => await _clipboard!.IsSupportedAsync()
);

        if (isSupported == false)
        {
            return;
        }

        var link = await GetSelectionLinkAsync();

        await InvokeAsync(async () => await _clipboard!.WriteTextAsync(link));

        await InvokeAsync(async () => await _dialogService!.ShowMessageBox("Copy item", "Item copied to clipboard"));
    }

    private async Task<string> GetSelectionLinkAsync()
    {
        var location = await _jsRuntime!.InvokeAsync<int>("getCurrentFrameScrollLocation")
            .ConfigureAwait(false);
        var xpaths = await _jsRuntime!.InvokeAsync<string[]?>("getSelectedXPaths")
            .ConfigureAwait(false);

        var p = new DisplayPageParameters
        {
            Book = SelectedScriptureBook,
            Location = location,
            XPaths = xpaths ?? [],
        };

        var root = new Uri(_navManager!.BaseUri, UriKind.Absolute);
        return p.ConvertToLink(root);
    }

    private async Task InitializePageFrameAsync()
    {
        Console.WriteLine($"Initializing page frame. Inverted: {IsDisplayInverted}");

        try
        {
            await _jsRuntime!.InvokeAsync<object>("initializePageFrame", IsDisplayInverted);
            await ApplySavedZoomSettingAsync();
            await ApplyHighlightsForBookAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to initialize page frame: {ex.Message}");
        }
        finally
        {
            _browserInitialization.TrySetResult();
            Console.WriteLine("Page frame initialization complete");
        }
    }

    private void SetPageState()
    {
        _isDisplayInverted = _applicationState!.IsDisplayInverted;
    }

    private void SetSelectedScripture(ScriptureBook book)
    {
        SelectedScripture = book.ToRootBook();
        SelectedScriptureBook = book;
    }

    #endregion
}
