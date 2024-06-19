using System.Collections.Concurrent;
using System.Diagnostics;
using System.Web;
using SimplyScriptures.Commands;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Search.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Common.Services.TextSearch;
using SimplyScriptures.Common.Services.TextSearch.Models;
using SimplyScriptures.Models;
using SimplyScriptures.Models.Interfaces;
using Telerik.Maui.Controls;

namespace SimplyScriptures.ViewModels;

public class DisplayViewModel : ViewModelBase, IQueryAttributable
{
    #region Private Variables

    private const string _zoomLevelSettingName = "ZoomLevel";

    private readonly IFileService _fileService;
    private readonly ConcurrentDictionary<ScriptureBook, BookInfo> _bookInfoLookup = new ConcurrentDictionary<ScriptureBook, BookInfo>();

    private TopicsParameters? _pageParameters;

    private bool _isInitialized;
    private TaskCompletionSource _browserInitialization = new TaskCompletionSource();
    private SemaphoreSlim _bookLoadingLock = new SemaphoreSlim(1);

    #endregion

    #region Public Properties

    public TextSearchService? PageSearch { get; set; }
    public ScriptureBook[] AllScriptures { get; } = new[] { ScriptureBook.OT1, ScriptureBook.OT2, ScriptureBook.OT3, ScriptureBook.NT, ScriptureBook.BM, ScriptureBook.DC };
    public RadPopup? HighlightColorPopup { get; set; }
    public RadPopup? BookmarksPopup { get; set; }
    public WebView? MainContentWebView { get; set; }

    #region IsMenuOpen

    private bool _isMenuOpen = true;

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set => SetProperty(ref _isMenuOpen, value);
    }

    #endregion IsMenuOpen

    #region NewBookmarkName

    private string _newBookmarkName = "";

    public string NewBookmarkName
    {
        get => _newBookmarkName;
        set => SetProperty(ref _newBookmarkName, value);
    }

    #endregion NewBookmarkName

    #region SearchText

    private string _searchText = "";

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    #endregion SearchText

    #region ContentFilterText

    private string _contentFilterText = "";

    public string ContentFilterText
    {
        get => _contentFilterText;
        set => SetProperty(ref _contentFilterText, value,
            async x => await FilterContentItemsAsync(x)
                );
    }

    #endregion ContentFilterText

    #region AllContentItems

    private MenuContentItem<ContentItem>[] _allContentItems = Array.Empty<MenuContentItem<ContentItem>>();

    public MenuContentItem<ContentItem>[] AllContentItems
    {
        get => _allContentItems;
        set => SetProperty(ref _allContentItems, value);
    }

    #endregion AllContentItems

    #region AllSearchItems

    private MenuContentItem<SearchMatch?>[] _allSearchItems = Array.Empty<MenuContentItem<SearchMatch?>>();

    public MenuContentItem<SearchMatch?>[] AllSearchItems
    {
        get => _allSearchItems;
        set => SetProperty(ref _allSearchItems, value);
    }

    #endregion AllSearchItems

    #region AllBookmarkItems

    private MenuContentItem<Bookmark>[] _allBookmarkItems = Array.Empty<MenuContentItem<Bookmark>>();

    public MenuContentItem<Bookmark>[] AllBookmarkItems
    {
        get => _allBookmarkItems;
        set => SetProperty(ref _allBookmarkItems, value);
    }

    #endregion AllBookmarkItems

    #region AllColors

    private Color[] _allColors = new[]
    {
        Colors.Transparent,
        Color.FromArgb("#FFFFFF"),
        Color.FromArgb("#EE534F"),
        Color.FromArgb("#AB47BC"),
        Color.FromArgb("#7E57C2"),
        Color.FromArgb("#5D6BC0"),
        Color.FromArgb("#42A5F5"),
        Color.FromArgb("#26C5DA"),
        Color.FromArgb("#24A79A"),
        Color.FromArgb("#66BB6A"),
        Color.FromArgb("#9CCC65"),
        Color.FromArgb("#D4E157"),
        Color.FromArgb("#FFEE58"),
        Color.FromArgb("#FFCA29"),
        Color.FromArgb("#FFA726"),
        Color.FromArgb("#FF7043"),
        Color.FromArgb("#8D6E63"),
        Color.FromArgb("#BDBDBD"),
        Color.FromArgb("#78909C"),
        Color.FromArgb("#3C3C3C"),
        Color.FromArgb("#000000")
    };

    public Color[] AllColors
    {
        get => _allColors;
        set => SetProperty(ref _allColors, value);
    }

    #endregion AllColors

    #region AllHighlights

    private HighlightSelection[] _allHighlights = Array.Empty<HighlightSelection>();

    public HighlightSelection[] AllHighlights
    {
        get => _allHighlights;
        set => SetProperty(ref _allHighlights, value);
    }

    #endregion AllHighlights

    #region CurrentBook

    private BookInfo? _currentBook = null;

    public BookInfo? CurrentBook
    {
        get => _currentBook;
        set => SetProperty(ref _currentBook, value);
    }

    #endregion CurrentBook

    #region IsSearchBusy

    private bool _isSearchBusy;

    public bool IsSearchBusy
    {
        get => _isSearchBusy;
        set => SetProperty(ref _isSearchBusy, value);
    }

    #endregion IsSearchBusy

    #region IsSearchInitializing

    private bool _isSearchInitializing;

    public bool IsSearchInitializing
    {
        get => _isSearchInitializing;
        set => SetProperty(ref _isSearchInitializing, value);
    }

    #endregion IsSearchInitializing

    #region IsContentInitializing

    private bool _isContentInitializing;

    public bool IsContentInitializing
    {
        get => _isContentInitializing;
        set => SetProperty(ref _isContentInitializing, value);
    }

    #endregion IsContentInitializing

    #region IsPageLoading

    private bool _isPageLoading;

    public bool IsPageLoading
    {
        get => _isPageLoading;
        set => SetProperty(ref _isPageLoading, value);
    }

    #endregion IsPageLoading

    #region SelectedScripture

    private ScriptureBook _selectedScripture;

    public ScriptureBook SelectedScripture
    {
        get => _selectedScripture;
        set => SetProperty(ref _selectedScripture, value,
            b => SelectScriptureBookAsyncCommand.Execute(b));
    }

    #endregion SelectedScripture

    #region SelectedScriptureBook

    private ScriptureBook _selectedScriptureBook;

    public ScriptureBook SelectedScriptureBook
    {
        get => _selectedScriptureBook;
        set => SetProperty(ref _selectedScriptureBook, value);
    }

    #endregion SelectedScriptureBook

    #region IsSearchActive

    private bool _isSearchActive;

    public bool IsSearchActive
    {
        get => _isSearchActive;
        set => SetProperty(ref _isSearchActive, value);
    }

    #endregion IsSearchActive

    #region IsContentActive

    private bool _isContentActive;

    public bool IsContentActive
    {
        get => _isContentActive;
        set => SetProperty(ref _isContentActive, value);
    }

    #endregion IsContentActive

    #region SelectedHighlightColor

    private Color _selectedHighlightColor = Colors.Transparent;

    public Color SelectedHighlightColor
    {
        get => _selectedHighlightColor;
        set => SetProperty(ref _selectedHighlightColor, value,
            c => HighlightColorPopup!.IsOpen = false);
    }

    #endregion SelectedHighlightColor

    #region SearchResults

    private SearchResults? _searchResults = null;

    public SearchResults? SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    #endregion SearchResults

    #region ShowDisplayMenuAsyncCommand

    private AsyncCommand? _showDisplayMenuAsyncCommand;

    public AsyncCommand ShowDisplayMenuAsyncCommand
    {
        get
        {
            return _showDisplayMenuAsyncCommand ??= CreateAsyncCommand(() => ShowDisplayMenuAsync(), "Unable to show display menu");
        }
    }

    #endregion ShowDisplayAsyncCommand

    #region NavItemSelectedAsyncCommand

    private AsyncCommand<ContentItem>? _navItemSelectedAsyncCommand;

    public AsyncCommand<ContentItem> NavItemSelectedAsyncCommand
    {
        get
        {
            return _navItemSelectedAsyncCommand ??= CreateAsyncCommand<ContentItem>(item => NavItemSelectedAsync(item), "Unable to select navigation item");
        }
    }

    #endregion NavItemSelectedAsyncCommand

    #region MenuContentItemSelectedAsyncCommand

    private AsyncCommand<IMenuContentItem>? _menuContentItemSelectedAsyncCommand;

    public AsyncCommand<IMenuContentItem> MenuContentItemSelectedAsyncCommand
    {
        get
        {
            return _menuContentItemSelectedAsyncCommand ??= CreateAsyncCommand<IMenuContentItem>(item => MenuContentItemSelectedAsync(item), "Unable to select item");
        }
    }

    #endregion MenuContentItemSelectedAsyncCommand

    #region SearchAsyncCommand

    private AsyncCommand? _searchAsyncCommand;

    public AsyncCommand SearchAsyncCommand
    {
        get
        {
            return _searchAsyncCommand ??= CreateAsyncCommand(() => SearchAsync(), "Unable to search");
        }
    }

    #endregion SearchAsyncCommand

    #region SearchMatchSelectedAsyncCommand

    private AsyncCommand<SearchMatch>? _searchMatchSelectedAsyncCommand;

    public AsyncCommand<SearchMatch> SearchMatchSelectedAsyncCommand
    {
        get
        {
            return _searchMatchSelectedAsyncCommand ??= CreateAsyncCommand<SearchMatch>(item => SearchMatchSelectedAsync(item), "Unable to select search match");
        }
    }

    #endregion SearchMatchSelectedAsyncCommand

    #region SelectScriptureBookAsyncCommand

    private AsyncCommand<ScriptureBook>? _selectScriptureBookAsyncCommand;

    public AsyncCommand<ScriptureBook> SelectScriptureBookAsyncCommand
    {
        get
        {
            switch (_selectScriptureBookAsyncCommand)
            {
                case null:
                    {
                        return _selectScriptureBookAsyncCommand ??= CreateAsyncCommand<ScriptureBook>(item => SelectScriptureBookAsync(item), "Unable to select scripture book");
                    }

                default:
                    return _selectScriptureBookAsyncCommand;
            }
        }
    }

    #endregion SelectScriptureBookAsyncCommand

    #region ShowPrintableScripturesAsyncCommand

    private AsyncCommand? _showPrintableScripturesAsyncCommand;

    public AsyncCommand ShowPrintableScripturesAsyncCommand
    {
        get
        {
            return _showPrintableScripturesAsyncCommand ??= CreateAsyncCommand(() => ShowPrintableScripturesAsync(), "Unable to show printable scriptures");
        }
    }

    #endregion ShowPrintableScripturesAsyncCommand

    #region CopyLinkAsyncCommand

    private AsyncCommand? _copyLinkAsyncCommand;

    public AsyncCommand CopyLinkAsyncCommand
    {
        get
        {
            return _copyLinkAsyncCommand ??= CreateAsyncCommand(() => CopyLinkAsync(), "Unable to copy link");
        }
    }

    #endregion ShareLinkAsyncCommand

    #region ZoomScripturesInAsyncCommand

    private AsyncCommand? _zoomScripturesInAsyncCommand;

    public AsyncCommand ZoomScripturesInAsyncCommand
    {
        get
        {
            return _zoomScripturesInAsyncCommand ??= CreateAsyncCommand(() => ZoomScripturesInAsync(), "Unable to zoom scriptures in");
        }
    }

    #endregion ZoomScripturesInAsyncCommand

    #region ZoomScripturesOutAsyncCommand

    private AsyncCommand? _zoomScripturesOutAsyncCommand;

    public AsyncCommand ZoomScripturesOutAsyncCommand
    {
        get
        {
            return _zoomScripturesOutAsyncCommand ??= CreateAsyncCommand(() => ZoomScripturesOutAsync(), "Unable to zoom scriptures out");
        }
    }

    #endregion ZoomScripturesOutAsyncCommand

    #region CloseSideMenuAsyncCommand

    private AsyncCommand? _closeSideMenuAsyncCommand;

    public AsyncCommand CloseSideMenuAsyncCommand
    {
        get
        {
            return _closeSideMenuAsyncCommand ??= CreateAsyncCommand(() => CloseSideMenuAsync(), "Unable to close side menu");
        }
    }

    #endregion CloseSideMenuAsyncCommand

    #region ShowBookmarksAsyncCommand

    private AsyncCommand? _showBookmarksAsyncCommand;

    public AsyncCommand ShowBookmarksAsyncCommand
    {
        get
        {
            return _showBookmarksAsyncCommand ??= CreateAsyncCommand(() => ShowBookmarksAsync(), "Unable to show bookmarks");
        }
    }

    #endregion ShowBookmarksAsyncCommand

    #region ShowHighlightColorsAsyncCommand

    private AsyncCommand? _showHighlightColorsAsyncCommand;

    public AsyncCommand ShowHighlightColorsAsyncCommand
    {
        get
        {
            return _showHighlightColorsAsyncCommand ??= CreateAsyncCommand(() => ShowHighlightColorsAsync(), "Unable to show highlight colors");
        }
    }

    #endregion ShowHighlightColorsAsyncCommand

    #region DeleteBookmarkAsyncCommand

    private AsyncCommand<MenuContentItem<Bookmark>>? _deleteBookmarkAsyncCommand;

    public AsyncCommand<MenuContentItem<Bookmark>> DeleteBookmarkAsyncCommand
    {
        get
        {
            return _deleteBookmarkAsyncCommand ??= CreateAsyncCommand<MenuContentItem<Bookmark>>(item => DeleteBookmarkAsync(item), "Unable to delete bookmark");
        }
    }

    #endregion DeleteBookmarkAsyncCommand

    #region AddBookmarkAsyncCommand

    private AsyncCommand? _addBookmarkAsyncCommand;

    public AsyncCommand AddBookmarkAsyncCommand
    {
        get
        {
            return _addBookmarkAsyncCommand ??= CreateAsyncCommand(() => AddBookmarkAsync(), "Unable to add bookmark");
        }
    }

    #endregion AddBookmarkAsyncCommand

    #region DisplayBookmarkAsyncCommand

    private AsyncCommand<Bookmark>? _displayBookmarkAsyncCommand;

    public AsyncCommand<Bookmark> DisplayBookmarkAsyncCommand
    {
        get
        {
            return _displayBookmarkAsyncCommand ??= CreateAsyncCommand<Bookmark>(item => DisplayBookmarkAsync(item), "Unable to display bookmark");
        }
    }

    #endregion DisplayBookmarkAsyncCommand

    #region ApplyHighlightAsyncCommand

    private AsyncCommand? _applyHighlightAsyncCommand;

    public AsyncCommand ApplyHighlightAsyncCommand
    {
        get
        {
            return _applyHighlightAsyncCommand ??= CreateAsyncCommand(() => ApplyHighlightAsync(), "Unable to apply highlight");
        }
    }

    #endregion ApplyHighlightAsyncCommand

    #region RemoveHighlightAsyncCommand

    private AsyncCommand? _removeHighlightAsyncCommand;

    public AsyncCommand RemoveHighlightAsyncCommand
    {
        get
        {
            return _removeHighlightAsyncCommand ??= CreateAsyncCommand(() => RemoveHighlightAsync(), "Unable to remove highlight");
        }
    }

    #endregion RemoveHighlightAsyncCommand

    #region InitializePageFrameAsyncCommand

    private AsyncCommand? _initializePageFrameAsyncCommand;

    public AsyncCommand InitializePageFrameAsyncCommand
    {
        get
        {
            return _initializePageFrameAsyncCommand ??= CreateAsyncCommand(() => InitializePageFrameAsync(), "Unable to initialize page frame");
        }
    }

    #endregion InitializePageFrameAsyncCommand

    #endregion

    #region Constructors

    public DisplayViewModel(IFileService fileService)
    {
        _browserInitialization.SetResult();
        _fileService = fileService;
        PageSearch = new TextSearchService(_fileService, () => Task.CompletedTask);
    }

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        if (_isInitialized == false)
        {
            await LoadHighlightsAsync();
            await LoadBookmarksAsync();
        }

        _isInitialized = true;

        _ = InitializeSearchAsync();
        _ = InitializeContentItemsAsync();

        await ProcessPageParametersAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var parameters = new TopicsParameters();

        var isFound = query.TryGetValue("ScriptureBook", out var scriptureBookParam);
        if (isFound && scriptureBookParam is ScriptureBook book)
        {
            parameters.SelectedBook = book;
        }

        isFound = query.TryGetValue("SearchText", out var searchTextParam);
        if (isFound && searchTextParam is string text)
        {
            parameters.SearchText = text;
        }

        isFound = query.TryGetValue("Location", out var locationParam);
        if (isFound && locationParam is int location)
        {
            parameters.HighlightLocation = location;
        }

        isFound = query.TryGetValue("XPaths", out var xpathsParam);
        if (isFound && xpathsParam is string[] xpaths)
        {
            parameters.HighlightXPaths = xpaths;
        }

        _pageParameters = parameters;
    }

    #endregion

    #region Private Methods

    private async Task ProcessPageParametersAsync()
    {
        var (isBookLoaded, isSearch) = await ProcessSearchTextParameterAsync();

        if (isBookLoaded == false)
        {
            await ProcessBookParameterAsync(isSearch);
        }

        await ProcessHighlightLocationParameterAsync();
        await ProcessHighlightXPathsParameterAsync();

        _pageParameters = null;
    }

    private async Task ProcessBookParameterAsync(bool isSearch)
    {
        try
        {
            var book = _pageParameters?.SelectedBook ?? ScriptureBook.BM_About;

            if (book == ScriptureBook.None)
            {
                book = ScriptureBook.BM_About;
            }

            await DispatchAsync(async () =>
            {
                if (isSearch == false)
                {
                    IsMenuOpen = true;
                    IsSearchActive = false;
                    IsContentActive = true;
                }

                await SetSelectedScriptureAsync(book);
            });

            await LoadCurrentBookAsync(book);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Unable to select book", "OK");
            Debug.WriteLine(ex.Message);
        }
    }

    private async Task<(bool isScripture, bool isSearch)> ProcessSearchTextParameterAsync()
    {
        try
        {
            var text = _pageParameters?.SearchText ?? "";
            if (string.IsNullOrWhiteSpace(text))
            {
                return (false, false);
            }

            await DispatchAsync(() =>
            {
                SearchText = text;
                IsMenuOpen = true;
                IsContentActive = false;
                IsSearchActive = true;
            });

            await InitializeSearchAsync();
            var isScripture = await SearchAsync();

            return (isScripture, true);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Unable to search text", "OK");
            Debug.WriteLine(ex.Message);
        }

        return (false, false);
    }

    private async Task ProcessHighlightLocationParameterAsync()
    {
        try
        {
            if (_pageParameters?.HighlightLocation == null)
            {
                return;
            }

            await ExecuteOnDocumentLoadedAsync(async () =>
            {
                await ScrollToLocationAsync(_pageParameters.HighlightLocation ?? 0)
                    ;
            })
            ;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Unable to highlight location", "OK")
                ;

            Debug.WriteLine(ex.Message);
        }
    }

    private async Task ProcessHighlightXPathsParameterAsync()
    {
        try
        {
            if (_pageParameters?.HighlightXPaths == null)
            {
                return;
            }

            await ExecuteOnDocumentLoadedAsync(async () =>
            {
                var xpath = _pageParameters.HighlightXPaths ?? Array.Empty<string>();
                await HighlightXPathLocationsAsync(xpath, true)
                    ;
            })
                ;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", "Unable to highlight items", "OK")
                ;

            Debug.WriteLine(ex.Message);
        }
    }

    private async Task ScrollToLocationAsync(int location)
    {
        for (var x = 0; x < 50; x++)
        {
            var script = $"setCurrentFrameScrollLocation({location});";
            var result = await ExecuteJavascriptAsync(script)
                ;

            bool.TryParse(result, out var success);

            if (success)
            {
                return;
            }

            await Task.Delay(100)
                ;
        }
    }

    private Task ShowDisplayMenuAsync()
    {
        return DispatchAsync(() =>
        {
            IsMenuOpen = !IsMenuOpen;
        });
    }

    private async Task InitializeContentItemsAsync()
    {
        await DispatchAsync(() =>
        {
            IsContentInitializing = true;
        })
        ;

        await Task.Delay(500)
            ;

        await FilterContentItemsAsync("")
            ;

        await Task.Delay(500)
            ;

        await DispatchAsync(() =>
        {
            IsContentInitializing = false;
        })
        ;
    }

    private async Task NavItemSelectedAsync(ContentItem? item)
    {
        if (item == null)
        {
            return;
        }

        Debug.WriteLine("Navigating to: " + item.Name + ", " + item.XPath);
        await SetSelectedScriptureAsync(item.Book)
            ;

        await DispatchAsync(() =>
        {
            IsMenuOpen = false;
        })
        ;

        await LoadCurrentBookAsync(item.Book)
            ;

        await HighlightXPathLocationsAsync(new[] { item.XPath }, false)
            ;
    }

    private async Task<bool> SearchAsync()
    {
        var isScriptureMatch = await SearchForTextAsync(SearchText ?? "")
            ;

        await ConvertSearchResultsAsync()
            ;

        return isScriptureMatch;
    }

    private async Task<bool> SearchForTextAsync(string? text)
    {
        text = (text ?? "").Trim();

        if (text.StartsWith('"') && text.EndsWith('"'))
        {
            await FindExactMatchesAsync(text)
                ;
            return false;
        }

        var isScriptureMatch = await FindScriptureMatchesAsync(text)
            ;

        if (isScriptureMatch == false)
        {
            await FindPhraseMatchesAsync(text)
                ;
        }

        return isScriptureMatch;
    }

    private Task ConvertSearchResultsAsync()
    {
        var results = SearchResults ?? new SearchResults();

        var allItems = results.AllMatches
            .GroupBy(x => x.Book.ToRootBook())
            .OrderBy(x => x.Key)
            .Select(x => ConvertSearchResults(x.Key, x.ToArray()))
            .TraverseItems(x => x.AllChildren)
            .ToArray();

        var newItems = allItems
            .Where(x => x.IsVisible && x.Parent == null)
            .ToArray();

        return DispatchAsync(() =>
        {
            AllSearchItems = newItems;
        });
    }

    private MenuContentItem<SearchMatch?> ConvertSearchResults(ScriptureBook book, SearchMatch[] items)
    {
        var searchGroup = new MenuContentItem<SearchMatch?>(null)
        {
            Parent = null,
            IsVisible = true,
            IsExpanded = false,
            Level = 0,
            Text = FixMenuTextForDisplay($"{book.ToDisplayString()} ({items.Length:N0})"),
            Action = () => Task.CompletedTask,
            Handler = new AsyncCommand<MenuContentItem<SearchMatch?>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x))),
        };

        searchGroup.AllChildren = ConvertSearchResults(searchGroup, items);
        searchGroup.HasChildren = searchGroup.AllChildren.Any();
        return searchGroup;
    }

    private MenuContentItem<SearchMatch?>[] ConvertSearchResults(MenuContentItem<SearchMatch?> container, SearchMatch?[] items)
    {
        return items
            .OrderByDescending(x => x!.Score)
            .Select(item => new MenuContentItem<SearchMatch?>(item)
            {
                Parent = container,
                TextHeader = FixMenuTextForDisplay(item!.BuildScriptureReference()),
                Text = FixMenuTextForDisplay(item.Text),
                IsVisible = true,
                IsExpanded = false,
                Level = 1,
                Action = () => Task.Run(() => SearchMatchSelectedAsyncCommand.Execute(item)),
                Handler = new AsyncCommand<MenuContentItem<SearchMatch?>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x))),
                HasChildren = false,
            })
            .ToArray();
    }

    private async Task LoadCurrentBookAsync(ScriptureBook book)
    {
        await _bookLoadingLock.WaitAsync();

        await DispatchAsync(() =>
        {
            IsPageLoading = true;
        });

        try
        {
            Debug.WriteLine($"Loading book: {book}");
            book = book.ToSpecificBook();
            var bookItem = await LoadScriptureBookAsync(book);

            if (CurrentBook != null && CurrentBook.HtmlPath == bookItem.HtmlPath)
            {
                return;
            }

            _browserInitialization = new TaskCompletionSource();
            await DispatchAsync(() =>
            {
                CurrentBook = bookItem;
                SelectedScripture = book.ToRootBook();
            });

            await FilterContentItemsAsync(ContentFilterText);

            // Wait for the initialization to complete
            await Task.WhenAny(_browserInitialization.Task, Task.Delay(5000));

            Debug.WriteLine($"Book loading complete: {book}");
        }
        finally
        {
            await DispatchAsync(() =>
            {
                IsPageLoading = false;
            });

            _bookLoadingLock.Release();
        }
    }

    private async Task ExecuteOnDocumentLoadedAsync(Func<Task> action)
    {
        await WaitForDocumentLoadedAsync();

        await action();
    }

    private async Task WaitForDocumentLoadedAsync()
    {
        while (true)
        {
            var isLoaded = await ExecuteJavascriptAsync("document.readyState !== 'loading'");

            if (isLoaded == "true")
            {
                return;
            }

            await Task.Delay(100);
        }
    }

    private async Task<BookInfo> LoadScriptureBookAsync(ScriptureBook book)
    {
        var isFound = _bookInfoLookup.TryGetValue(book, out var result);

        if (isFound && result != null)
        {
            return result;
        }

        Debug.WriteLine($"Loading book info: {book}");
        var htmlPath = book.ToHtmlPath();
        var jsonPath = book.ToMenuContentPath();

        var contentData = await _fileService!.LoadDataAsync(jsonPath);

        var contentItems = contentData.DeserializeFromJson<ContentItem[]>()
                       ?? Array.Empty<ContentItem>();

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

    private Task ApplyHighlightsForBookAsync()
    {
        var highlights = GetCurrentBookHighlights();

        return highlights
            .ForAllAsync(async selection =>
            {
                await AddSelectionHighlightsAsync(selection);
            });
    }

    private HighlightSelection[] GetCurrentBookHighlights()
    {
        return AllHighlights
            .Where(x => x.Book == SelectedScriptureBook)
            .ToArray();
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

        await DispatchAsync(() =>
        {
            SearchResults = new SearchResults
            {
                AllMatches = matches,
                MatchMode = matches.Length > 0
                    ? SearchMatchMode.SearchMatches
                    : SearchMatchMode.NoMatches
            };
        })
        ;

        Debug.WriteLine($"Found {matches.Length} exact matches");
        return matches.Length > 0;
    }

    private async Task<bool> FindPhraseMatchesAsync(string search)
    {
        var matches = await ExecuteSearchAsync(() => PageSearch!.FindPhraseMatchesAsync(search))
            ;

        await DispatchAsync(() =>
        {
            SearchResults = new SearchResults
            {
                AllMatches = matches,
                MatchMode = matches.Length > 0
                    ? SearchMatchMode.SearchMatches
                    : SearchMatchMode.NoMatches
            };
        })
            ;

        Debug.WriteLine($"Found {matches.Length} exact matches");
        return matches.Length > 0;
    }

    private async Task<bool> FindScriptureMatchesAsync(string search)
    {
        var matches = await ExecuteSearchAsync(() => PageSearch!.FindScriptureMatchesAsync(search))
            ;

        if (matches.Length > 0)
        {
            var xpaths = matches
                .Select(x => x.XPath)
                .ToArray();

            await SearchMatchSelectedAsync(matches[0])
                ;

            await HighlightXPathLocationsAsync(xpaths, true)
                ;
        }

        SearchResults = new SearchResults
        {
            AllMatches = matches,
            MatchMode = matches.Length > 0
                ? SearchMatchMode.ScriptureMatch
                : SearchMatchMode.NoMatches
        };

        Debug.WriteLine($"Found {matches.Length} scripture matches");
        return matches.Length > 0;
    }

    private async Task SearchMatchSelectedAsync(SearchMatch? item)
    {
        if (item == null)
        {
            return;
        }

        await LoadCurrentBookAsync(item.Book)
            ;

        await HighlightXPathLocationsAsync(new[] { item.XPath }, true)
            ;

        IsMenuOpen = false;
    }

    private async Task HighlightXPathLocationsAsync(string[] xpath, bool isHighlight)
    {
        var json = xpath.SerializeToJson()
            .Replace("'", "\\'");
        var script = $"highlightSearchResults('{json}', {isHighlight.ToString().ToLower()});";

        for (var x = 0; x < 50; x++)
        {
            var result = await ExecuteJavascriptAsync(script)
                ;

            bool.TryParse(result, out var success);

            if (success)
            {
                return;
            }

            await Task.Delay(100)
                ;
        }
    }

    private async Task SelectScriptureBookAsync(ScriptureBook book)
    {
        await SetSelectedScriptureAsync(book)
            ;

        await LoadCurrentBookAsync(book)
            ;

        await DispatchAsync(() =>
        {
            IsMenuOpen = true;

            IsSearchActive = false;
            IsContentActive = true;
        })
        ;
    }

    private Task FilterContentItemsAsync(string filter)
    {
        var contentItems = (CurrentBook?.ContentItems ?? Array.Empty<ContentItem>())
            .Select(x => ConvertContentItems(x, null, 0))
            .ToArray();

        var allItems = contentItems
                .TraverseItems(x => x.AllChildren)
                .ToArray();

        // Show all
        if (string.IsNullOrWhiteSpace(filter))
        {
            allItems
                .ForEach(x => x.IsVisible = true);
        }
        else
        {
            // Hide all
            allItems
                .ForEach(x => x.IsVisible = false);

            allItems
                .ForEach(x =>
                {
                    ProcessContentItemFilter(x, filter);
                });
        }

        return DispatchAsync(() =>
        {
            AllContentItems = allItems
                .Where(x => x.IsVisible && x.Parent == null)
                .ToArray();
        });
    }

    private static void ProcessContentItemFilter(IMenuContentItem item, string filter)
    {
        var isVisible = item.Text
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

    private MenuContentItem<ContentItem> ConvertContentItems(ContentItem item, MenuContentItem<ContentItem>? parent, int level)
    {
        var newItem = new MenuContentItem<ContentItem>(item)
        {
            Parent = parent,
            IsVisible = true,
            IsExpanded = false,
            Level = level,
            Text = FixMenuTextForDisplay(item.Name),
            Action = () => Task.Run(() => NavItemSelectedAsyncCommand.Execute(item)),
            Handler = new AsyncCommand<MenuContentItem<ContentItem>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x)))
        };

        var children = (item.Children ?? Array.Empty<ContentItem>())
            .Select(x => ConvertContentItems(x, newItem, level + 1))
            .ToArray();

        newItem.AllChildren = children;
        newItem.HasChildren = children.Any();
        return newItem;
    }

    private static string FixMenuTextForDisplay(string text)
    {
        return text.Trim()
            .Replace("&amp;", "&")
            .Replace("&apos;", "'");
    }

    private Task ShowPrintableScripturesAsync()
    {
        var url = SelectedScriptureBook.ToPDFPath();
        url = url.Replace("./", "https://simplyscriptures.com/");

        return Launcher.Default.OpenAsync(url);
    }

    private async Task InitializeSearchAsync()
    {
        await DispatchAsync(() =>
        {
            IsSearchInitializing = true;
        })
        ;

        await PageSearch!.InitializeAsync()
            ;

        await DispatchAsync(() =>
        {
            IsSearchInitializing = false;
        })
            ;
    }

    private async Task<SearchMatch[]> ExecuteSearchAsync(Func<Task<SearchMatch[]>> action)
    {
        IsSearchBusy = true;
        var matches = await action()
            ;

        IsSearchBusy = false;
        return matches;
    }

    private async Task ZoomScripturesInAsync()
    {
        var currentZoomText = await ExecuteJavascriptAsync("zoomIn();")
            ;

        var isValid = double.TryParse(currentZoomText, out var currentZoom);
        currentZoom = isValid
            ? currentZoom
            : 1.0;

        await SaveSettingAsync(_zoomLevelSettingName, currentZoom)
            ;
    }

    private async Task ZoomScripturesOutAsync()
    {
        var currentZoomText = await ExecuteJavascriptAsync("zoomOut();")
            ;

        var isValid = double.TryParse(currentZoomText, out var currentZoom);
        currentZoom = isValid
            ? currentZoom
            : 1.0;

        await SaveSettingAsync(_zoomLevelSettingName, currentZoom)
            ;
    }

    private Task CloseSideMenuAsync()
    {
        IsMenuOpen = false;
        return Task.CompletedTask;
    }

    private Task ShowBookmarksAsync()
    {
        BookmarksPopup!.IsOpen = true;
        return Task.CompletedTask;
    }

    private Task ShowHighlightColorsAsync()
    {
        HighlightColorPopup!.IsOpen = true;
        return Task.CompletedTask;
    }

    private async Task DeleteBookmarkAsync(MenuContentItem<Bookmark>? bookmark)
    {
        if (bookmark == null)
        {
            return;
        }

        switch (bookmark.Parent)
        {
            case null:
                await RemoveSectionBookmarksAsync(bookmark)
                            ;
                break;
            default:
                await RemoveSingleBookmarkAsync(bookmark)
                        ;
                break;
        }
    }

    private async Task RemoveSectionBookmarksAsync(MenuContentItem<Bookmark> bookmark)
    {
        var confirm = await DisplayAlertAsync("Delete bookmarks?", $"Delete all for {bookmark.Text}?", "Yes", "No")
            ;

        if (confirm != true)
        {
            return;
        }

        await DispatchAsync(() =>
        {
            AllBookmarkItems = AllBookmarkItems
                .Except(new[] { bookmark })
                .ToArray();
        })
        ;

        await SaveBookmarksAsync()
            ;
    }

    private async Task RemoveSingleBookmarkAsync(MenuContentItem<Bookmark> bookmark)
    {
        var confirm = await DisplayAlertAsync("Delete bookmark?", $"Delete {bookmark.Text}?", "Yes", "No")
            ;

        if (confirm != true)
        {
            return;
        }

        await DispatchAsync(() =>
        {
            bookmark.Parent?.RemoveChild(bookmark);
        })
        ;

        await SaveBookmarksAsync()
            ;
    }

    private async Task AddBookmarkAsync()
    {
        if (string.IsNullOrWhiteSpace(NewBookmarkName))
        {
            return;
        }

        var locationText = await ExecuteJavascriptAsync("getCurrentFrameScrollLocation();")
            ;
        var location = int.Parse(locationText);

        var bookmark = new Bookmark
        {
            Name = NewBookmarkName,
            Book = SelectedScriptureBook,
            Location = location
        };

        await DispatchAsync(() =>
        {
            NewBookmarkName = "";

            var parent = AllBookmarkItems.FirstOrDefault(x => x.Item!.Book == SelectedScripture);
            if (parent == null)
            {
                parent = CreateBookmarkGroupHeader(SelectedScriptureBook);
                AllBookmarkItems = AllBookmarkItems
                    .Concat(new[] { parent })
                    .ToArray();
            }

            var bookmarkMenuItem = CreateBookmarkMenuItem(parent, bookmark);
            parent.AddChild(bookmarkMenuItem);

            BookmarksPopup!.IsOpen = false;
        });

        await SaveBookmarksAsync();
    }

    private MenuContentItem<Bookmark> CreateBookmarkMenuItem(MenuContentItem<Bookmark> parent, Bookmark bookmark)
    {
        return new MenuContentItem<Bookmark>(bookmark)
        {
            Action = () => Task.Run(() => DisplayBookmarkAsyncCommand.Execute(bookmark)),
            Handler = new AsyncCommand<MenuContentItem<Bookmark>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x))),
            IsExpanded = false,
            IsVisible = true,
            Level = 1,
            Parent = parent,
            Text = bookmark.Name,
            HasChildren = false,
        };
    }

    private async Task DisplayBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark == null)
        {
            return;
        }

        await DispatchAsync(async () =>
        {
            BookmarksPopup!.IsOpen = false;

            if (SelectedScriptureBook != bookmark.Book)
            {
                await SetSelectedScriptureAsync(bookmark.Book);
                await LoadCurrentBookAsync(bookmark.Book);
            }
        })
        ;

        _ = ExecuteJavascriptAsync($"setCurrentFrameScrollLocation('{bookmark.Location}')");
    }

    private Task SaveBookmarksAsync()
    {
        var allBookmarks = AllBookmarkItems
            .TraverseItems(x => x.AllChildren)
            .Where(x => x.Parent != null)
            .Select(x => x.Item)
            .Where(x => x != null)
            .ToArray();

        var json = allBookmarks.SerializeToJson();
        var location = Path.Combine(FileSystem.Current.AppDataDirectory, "bookmarks.json");

        return File.WriteAllTextAsync(location, json);
    }

    private async Task LoadBookmarksAsync()
    {
        var bookmarks = await ReadBookmarksAsync();

        Debug.WriteLine($"Loaded {bookmarks.Length} bookmarks");
        var bookmarkGroups = bookmarks
            .GroupBy(x => x.Book)
            .Select(x => ConvertBookmarks(x.Key, x.ToArray()))
            .ToArray();

        await DispatchAsync(() =>
        {
            AllBookmarkItems = bookmarkGroups;
        });
    }

    private async Task LoadHighlightsAsync()
    {
        var highlights = await ReadHighlightsAsync();

        Debug.WriteLine($"Loaded {highlights.Length} highlights");
        await DispatchAsync(() =>
        {
            AllHighlights = highlights;
        });
    }

    private MenuContentItem<Bookmark> ConvertBookmarks(ScriptureBook book, Bookmark[] bookmarks)
    {
        var header = CreateBookmarkGroupHeader(book);
        header.AllChildren = ConvertBookmarks(header, bookmarks);

        return header;
    }

    private MenuContentItem<Bookmark> CreateBookmarkGroupHeader(ScriptureBook book)
    {
        return new MenuContentItem<Bookmark>(new Bookmark { Book = book })
        {
            Handler = new AsyncCommand<MenuContentItem<Bookmark>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x))),
            Action = () => Task.CompletedTask,
            Text = book.ToDisplayString(),
            IsVisible = true,
            HasChildren = true,
            IsExpanded = false,
        };
    }

    private MenuContentItem<Bookmark>[] ConvertBookmarks(MenuContentItem<Bookmark> parent, Bookmark[] bookmarks)
    {
        return bookmarks
            .Select(x => new MenuContentItem<Bookmark>(x)
            {
                Level = 1,
                Parent = parent,
                Text = x.Name,
                IsVisible = true,
                IsExpanded = false,
                HasChildren = false,
                Handler = new AsyncCommand<MenuContentItem<Bookmark>>(x => Task.Run(() => MenuContentItemSelectedAsyncCommand.Execute(x))),
                Action = () => Task.Run(() => DisplayBookmarkAsyncCommand.Execute(x)),
            })
            .ToArray();
    }

    private static async Task<Bookmark[]> ReadBookmarksAsync()
    {
        var location = Path.Combine(FileSystem.Current.AppDataDirectory, "bookmarks.json");

        if (File.Exists(location) == false)
        {
            return Array.Empty<Bookmark>();
        }

        var json = await File.ReadAllTextAsync(location);
        return json.DeserializeFromJson<Bookmark[]>() ?? Array.Empty<Bookmark>();
    }

    private static async Task<HighlightSelection[]> ReadHighlightsAsync()
    {
        var location = Path.Combine(FileSystem.Current.AppDataDirectory, "highlights.json");

        if (File.Exists(location) == false)
        {
            return Array.Empty<HighlightSelection>();
        }

        var json = await File.ReadAllTextAsync(location);
        return json.DeserializeFromJson<HighlightSelection[]>() ?? Array.Empty<HighlightSelection>();
    }

    private static async Task MenuContentItemSelectedAsync(IMenuContentItem? item)
    {
        if (item == null)
        {
            return;
        }

        if (item.HasChildren)
        {
            await DispatchAsync(() =>
            {
                if (item.IsExpanded)
                {
                    item.CollapseItem();
                }
                else
                {
                    item.ExpandItem();
                }
            })
            ;
        }
        else
        {
            await item.Action();
        }
    }

    private Task<string> ExecuteJavascriptAsync(string script, bool displayErrors = false)
    {
        script = script.Replace("\r\n", " ").Replace("\n", " ");
        var fullScript = displayErrors
            ? $"try {{ {script} }} catch (error) {{ alert(`Error: ${{error.stack}}`); }}"
            : script;

        return DispatchAsync(async () =>
        {
            return await MainContentWebView!.EvaluateJavaScriptAsync(fullScript);
        });
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
        var selections = await GetSelectedXPathsAsync()
            ;

        switch (selections.Length)
        {
            case 0:
                return null;
            default:
                return new HighlightSelection
                {
                    Book = SelectedScriptureBook,
                    Color = SelectedHighlightColor.ToHex(),
                    XPath = selections
                };
        }
    }

    private async Task<string[]> GetSelectedXPathsAsync()
    {
        var json = await ExecuteJavascriptAsync("getSelectedXPaths();");

        json = json.UnescapeJson();

        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        var selections = json.DeserializeFromJson<string[]>() ?? Array.Empty<string>();
        return selections
            .Select(x => x.Replace('"', '\''))
            .ToArray();
    }

    private async Task RemoveSelectionHighlightsAsync(HighlightSelection selection)
    {
        await selection.XPath
            .ForAllAsync(async item =>
            {
                await ExecuteJavascriptAsync($"updateHighlightColor(\"{item}\", '');");
            });

        var matches = AllHighlights
            .Where(x => x.Book == selection.Book && x.XPath.Any(y => selection.XPath.Contains(y)))
            .ToArray();

        AllHighlights = AllHighlights
            .Except(matches)
            .ToArray();
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
                var colorText = App.IsDarkTheme
                    ? $"{Color.Parse(selection.Color).ToString().ToInverseColor()}66"
                    : $"{selection.Color}66";

                await ExecuteJavascriptAsync($"updateHighlightColor(\"{item}\", \"{colorText}\");");
            });

        await DispatchAsync(() =>
        {
            AllHighlights = AllHighlights
                .Concat(new[] { selection })
                .ToArray();
        });
    }

    private Task SaveCurrentHighlightsAsync()
    {
        var json = AllHighlights.SerializeToJson();
        var location = Path.Combine(FileSystem.Current.AppDataDirectory, "highlights.json");

        return File.WriteAllTextAsync(location, json);
    }

    private async Task CopyLinkAsync()
    {
        var link = await GetSelectionLinkAsync()
            ;

        await CopyItemToClipboardAsync(link)
            ;
    }

    private async Task<string> GetSelectionLinkAsync()
    {
        var locationResult = await ExecuteJavascriptAsync("getCurrentFrameScrollLocation();")
            .ConfigureAwait(false);
        int.TryParse(locationResult, out var location);

        var xpaths = await GetSelectedXPathsAsync()
            ;

        var query = $"s={SelectedScriptureBook}";
        if (location > 0)
        {
            query += $"&l={location}";
        }

        if (xpaths.Length > 0)
        {
            var json = xpaths.SerializeToJson();
            var compressed = await json.CompressTextAsync()
                ;
            var encoded = HttpUtility.UrlEncode(compressed);

            query += $"&x={encoded}";
        }

        var root = "https://simplyscriptures.com/display";

        return $"{root}?{query}";
    }

    private async Task InitializePageFrameAsync()
    {
        Debug.WriteLine($"Initializing page frame. Inverted: {App.IsDarkTheme}");

        try
        {
            await ExecuteJavascriptAsync($"initializePageFrame({App.IsDarkTheme.ToString().ToLower()});");
            await ApplySavedZoomSettingAsync();
            await ApplyHighlightsForBookAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to initialize page frame: {ex.Message}");
        }
        finally
        {
            _browserInitialization.TrySetResult();
            Debug.WriteLine("Page frame initialization complete");
        }
    }

    private Task SetSelectedScriptureAsync(ScriptureBook book)
    {
        return DispatchAsync(() =>
        {
            SelectedScripture = book.ToRootBook();
            SelectedScriptureBook = book;
        });
    }

    private async Task ApplySavedZoomSettingAsync()
    {
        try
        {
            var zoomLevel = await LoadDoubleSettingAsync(_zoomLevelSettingName, 1.0);

            await ExecuteJavascriptAsync($"applyZoom({zoomLevel});");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to apply zoom level: {ex.Message}");
        }
    }

    #endregion
}
