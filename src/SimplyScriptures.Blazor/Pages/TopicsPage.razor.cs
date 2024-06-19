using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Pages.Common;
using SimplyScriptures.Pages.Models;
using SimplyScriptures.Services.ApplicationState.Interfaces;

namespace SimplyScriptures.Pages;

[SuppressMessage("ReSharper", "AsyncApostle.AsyncAwaitMayBeElidedHighlighting")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "<Pending>")]
public class TopicsPageBase : ViewModelBase
{
    #region Private Variables

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

    #endregion

    #region Public Properties

    public static ContentTopic[] AllTopics { get; private set; } = [];

    #region IsOTVisible

    private bool _isOTVisible = true;

    public bool IsOTVisible
    {
        get => _isOTVisible;
        set => UpdateProperty(ref _isOTVisible, value,
            async v => await SaveSettingAsync(nameof(IsOTVisible), v));
    }

    #endregion IsOTVisible

    #region IsNTVisible

    private bool _isNTVisible = true;

    public bool IsNTVisible
    {
        get => _isNTVisible;
        set => UpdateProperty(ref _isNTVisible, value,
            async v => await SaveSettingAsync(nameof(IsNTVisible), v));
    }

    #endregion IsNTVisible

    #region IsBMVisible

    private bool _isBMVisible = true;

    public bool IsBMVisible
    {
        get => _isBMVisible;
        set => UpdateProperty(ref _isBMVisible, value,
            async v => await SaveSettingAsync(nameof(IsBMVisible), v));
    }

    #endregion IsBMVisible

    #region IsDCVisible

    private bool _isDCVisible = true;

    public bool IsDCVisible
    {
        get => _isDCVisible;
        set => UpdateProperty(ref _isDCVisible, value,
            async v => await SaveSettingAsync(nameof(IsDCVisible), v));
    }

    #endregion IsDCVisible

    #region IsDisplayInverted

    private bool _isDisplayInverted = false;

    public bool IsDisplayInverted
    {
        get => _isDisplayInverted;
        set => UpdateProperty(ref _isDisplayInverted, value,
            v => SaveSettingAsync(nameof(IsDisplayInverted), v));
    }

    #endregion IsDisplayInverted

    #region IsTopicsLoading

    private bool _isTopicsLoading = true;

    public bool IsTopicsLoading
    {
        get => _isTopicsLoading;
        set => UpdateProperty(ref _isTopicsLoading, value);
    }

    #endregion IsTopicsLoading

    #region IsTopicsOpen

    private bool _isTopicsOpen = true;

    public bool IsTopicsOpen
    {
        get => _isTopicsOpen;
        set => UpdateProperty(ref _isTopicsOpen, value);
    }

    #endregion IsTopicsOpen

    #region SelectedTopic

    private ContentTopic? _selectedTopic = null;

    public ContentTopic? SelectedTopic
    {
        get => _selectedTopic;
        set => UpdateProperty(ref _selectedTopic, value,
            v => SelectedTopicValue = v);
    }

    #endregion SelectedTopic

    #region SelectedTopicValue

    private object? _selectedTopicValue = null;

    public object? SelectedTopicValue
    {
        get => _selectedTopicValue;
        set => UpdateProperty(ref _selectedTopicValue, value,
            v => SelectedTopic = v as ContentTopic);
    }

    #endregion SelectedTopicValue

    #region TopicsFilterText

    private string _topicsFilterText = "";

    public string TopicsFilterText
    {
        get => _topicsFilterText;
        set => UpdateProperty(ref _topicsFilterText, value,
            f => FilterContentTopicsAsync());
    }

    #endregion TopicsFilterText

    #region TopicItemsFilterText

    private string _topicItemsFilterText = "";

    public string TopicItemsFilterText
    {
        get => _topicItemsFilterText;
        set => UpdateProperty(ref _topicItemsFilterText, value);
    }

    #endregion TopicItemsFilterText

    #region FilteredTopics

    private ContentTopic[] _filteredTopics = [];

    public ContentTopic[] FilteredTopics
    {
        get => _filteredTopics;
        set => UpdateProperty(ref _filteredTopics, value);
    }

    #endregion FilteredTopics

    #region ContentTopicSelectedAsyncCommand

    private Func<ContentTopic, Task>? _contentTopicSelectedAsyncCommand;

    public Func<ContentTopic, Task> ContentTopicSelectedAsyncCommand => _contentTopicSelectedAsyncCommand ??= CreateEventCallbackAsyncCommand<ContentTopic>(ContentTopicSelectedAsync, "Unable to select content topic");

    #endregion ContentTopicSelectedAsyncCommand

    #region CopyTopicItemToClipboardAsyncCommand

    private Func<ContentTopicItem, Task>? _copyTopicItemToClipboardAsyncCommand;

    public Func<ContentTopicItem, Task> CopyTopicItemToClipboardAsyncCommand => _copyTopicItemToClipboardAsyncCommand ??= CreateEventCallbackAsyncCommand<ContentTopicItem>(CopyTopicItemToClipboardAsync, "Unable to copy content topic item");

    #endregion CopyTopicItemToClipboardAsyncCommand

    #region SuggestNewTopicAsyncCommand

    private Func<Task>? _suggestNewTopicAsyncCommand;

    public Func<Task> SuggestNewTopicAsyncCommand => _suggestNewTopicAsyncCommand ??= CreateEventCallbackAsyncCommand(SuggestNewTopicAsync, "Unable to suggest new topic");

    #endregion SuggestNewTopicAsyncCommand

    #region SuggestNewTopicItemAsyncCommand

    private Func<Task>? _suggestNewTopicItemAsyncCommand;

    public Func<Task> SuggestNewTopicItemAsyncCommand => _suggestNewTopicItemAsyncCommand ??= CreateEventCallbackAsyncCommand(SuggestNewTopicItemAsync, "Unable to suggest new topic item");

    #endregion SuggestNewTopicItemAsyncCommand

    #region ToggleTopicsVisibilityAsyncCommand

    private Func<Task>? _toggleTopicsVisibilityAsyncCommand;

    public Func<Task> ToggleTopicsVisibilityAsyncCommand => _toggleTopicsVisibilityAsyncCommand ??= CreateEventCallbackAsyncCommand(ToggleTopicsVisibilityAsync, "Unable to toggle topics visibility");

    #endregion ToggleTopicsVisibilityAsyncCommand

    #endregion

    #region Protected Methods

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        SetPageState();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _applicationState!.LoadCurrentStateAsync();

            SetPageState();

            await RefreshAsync();
            await LoadCategoryTopicsAsync();
            await ProcessPageParametersAsync();
            await AlertIfMobileAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected bool CheckVisibleItem(ContentTopicItem item)
    {
        var filterText = (TopicItemsFilterText ?? "").Trim();

        var filterMatch = string.IsNullOrWhiteSpace(filterText) ||
                          item.Text.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                          item.Verse.Contains(filterText, StringComparison.OrdinalIgnoreCase);

        return item.Book.IsOldTestament()
            ? filterMatch && IsOTVisible
            : item.Book.IsNewTestament()
            ? filterMatch && IsNTVisible
            : item.Book.IsBookOfMormon() ? filterMatch && IsBMVisible : item.Book.IsDoctrineAndCovenants() && filterMatch && IsDCVisible;
    }

    protected MarkupString ProcessItemText(ContentTopicItem item)
    {
        var html = item.ConvertToFullHtml();
        return new MarkupString(html);
    }

    protected string BuildItemUrl(ContentTopicItem item)
    {
        var root = new Uri(_navManager!.BaseUri, UriKind.Absolute);

        var p = new DisplayPageParameters
        {
            Book = item.Book,
            SearchText = item.Verse
        };

        return p.ConvertToLink(root);
    }

    #endregion

    #region Private Methods

    private async Task ProcessPageParametersAsync()
    {
        try
        {
            var isValid = Uri.TryCreate(_navManager!.Uri ?? "", UriKind.Absolute, out var uri);

            if (isValid == false || uri == null || string.IsNullOrWhiteSpace(uri!.Query))
            {
                return;
            }

            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            var topic = queryParameters.Get("t");

            await ProcessPageParametersAsync(topic ?? "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process page parameters: {ex.Message}");
        }
    }

    private async Task ProcessPageParametersAsync(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return;
        }

        SelectedTopic = AllTopics
            .FirstOrDefault(x => x.Topic.Equals(topic, StringComparison.OrdinalIgnoreCase));

        await RefreshAsync();
    }

    private Task FilterContentTopicsAsync()
    {
        var filterText = (TopicsFilterText ?? "").Trim();

        var filteredItems = AllTopics
            .Where(x => string.IsNullOrWhiteSpace(filterText) ||
                        x.Topic.Contains(filterText, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        FilteredTopics = filteredItems;

        if (SelectedTopic != null && FilteredTopics.Contains(SelectedTopic) == false)
        {
            SelectedTopic = FilteredTopics.FirstOrDefault();
        }

        return RefreshAsync();
    }

    private Task CopyTopicItemToClipboardAsync(ContentTopicItem item)
    {
        var url = BuildItemUrl(item);
        var text = $"{item.ConvertToFormattedText()}\r\n{url}";

        return CopyTextToClipboardAsync(text);
    }

    private async Task LoadCategoryTopicsAsync()
    {
        try
        {
            await LoadAllCategoryTopicsAsync();

            FilteredTopics = AllTopics;
            TopicsFilterText = "";
            IsTopicsLoading = false;
            SelectedTopic = FilteredTopics.FirstOrDefault();

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to load category topics: {ex.Message}");
        }
    }

    private async Task LoadAllCategoryTopicsAsync()
    {
        if (AllTopics.Length > 0)
        {
            return;
        }

        var data = await _fileService!.LoadDataAsync("./Scriptures/Topics/topics.json");

        var topicItems = data.DeserializeFromJson<ContentTopic[]>() ?? [];
        topicItems = [.. topicItems.OrderBy(x => x.Topic),];

        foreach (var item in topicItems)
        {
            item.Items = [.. item.Items.OrderByDescending(x => x.Book),];
        }

        AllTopics = topicItems;
    }

    private Task ContentTopicSelectedAsync(ContentTopic item)
    {
        SelectedTopic = item;

        return RefreshAsync();
    }

    private async Task SuggestNewTopicItemAsync()
    {
        var subject = Uri.EscapeDataString($"Suggest TOC topic item for '{SelectedTopic?.Topic}'");
        var body = Uri.EscapeDataString(@"I think this verse would be a good addition.\r\nVerse: ");
        var url = $"mailto:admin@simplyscriptures.com?subject={subject}&body={body}"
            .Replace("%5Cr%5Cn", "%0D%0A");

        await _jsRuntime!.InvokeVoidAsync("open", url, "_self")
            .ConfigureAwait(false);
    }

    private async Task SuggestNewTopicAsync()
    {
        var subject = Uri.EscapeDataString("Suggest TOC topic");
        var body = Uri.EscapeDataString(@"I think this would be a good topic.\r\nTopic: ");
        var url = $"mailto:admin@simplyscriptures.com?subject={subject}&body={body}"
            .Replace("%5Cr%5Cn", "%0D%0A");

        await _jsRuntime!.InvokeVoidAsync("open", url, "_self")
            .ConfigureAwait(false);
    }

    private Task ToggleTopicsVisibilityAsync()
    {
        IsTopicsOpen = !IsTopicsOpen;

        return RefreshAsync();
    }

    private void SetPageState()
    {
        _isDisplayInverted = _applicationState!.IsDisplayInverted;
        _isBMVisible = _applicationState.IsBMVisible;
        _isNTVisible = _applicationState.IsNTVisible;
        _isOTVisible = _applicationState.IsOTVisible;
        _isDCVisible = _applicationState.IsDCVisible;
    }

    #endregion
}
