using System.Diagnostics.CodeAnalysis;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Models;
using SimplyScriptures.Pages;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Commands;

namespace SimplyScriptures.ViewModels;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TopicsViewModel(IFileService fileService) : ViewModelBase
{
    #region Private Variables

    private readonly IFileService _fileService = fileService;
    private ContentTopic[] _allLoadedTopics = [];

    #endregion

    #region Public Properties

    #region IsMenuOpen

    private bool _isMenuOpen;

    public bool IsMenuOpen
    {
        get => _isMenuOpen;
        set => SetProperty(ref _isMenuOpen, value);
    }

    #endregion IsMenuOpen

    #region IsTopicsInitializing

    private bool _isTopicsInitializing;

    public bool IsTopicsInitializing
    {
        get => _isTopicsInitializing;
        set => SetProperty(ref _isTopicsInitializing, value);
    }

    #endregion IsTopicsInitializing

    #region IsOTVisible

    private bool _isOTVisible = true;

    public bool IsOTVisible
    {
        get => _isOTVisible;
        set => SetProperty(ref _isOTVisible, value,
            v => FilterTopicItemsAsync(TopicItemsFilterText)
                );
    }

    #endregion IsOTVisible

    #region IsNTVisible

    private bool _isNTVisible = true;

    public bool IsNTVisible
    {
        get => _isNTVisible;
        set => SetProperty(ref _isNTVisible, value,
            v => FilterTopicItemsAsync(TopicItemsFilterText)
                );
    }

    #endregion IsNTVisible

    #region IsBMVisible

    private bool _isBMVisible = true;

    public bool IsBMVisible
    {
        get => _isBMVisible;
        set => SetProperty(ref _isBMVisible, value,
            v => FilterTopicItemsAsync(TopicItemsFilterText)
                );
    }

    #endregion IsBMVisible

    #region IsDCVisible

    private bool _isDCVisible = true;

    public bool IsDCVisible
    {
        get => _isDCVisible;
        set => SetProperty(ref _isDCVisible, value,
            v => FilterTopicItemsAsync(TopicItemsFilterText)
                );
    }

    #endregion IsDCVisible

    #region TopicsFilterText

    private string _topicsFilterText = "";

    public string TopicsFilterText
    {
        get => _topicsFilterText;
        set => SetProperty(ref _topicsFilterText, value,
            FilterTopicsAsync
                );
    }

    #endregion TopicsFilterText

    #region ShowTopicsMenuAsyncCommand

    private AsyncCommand? _showTopicsMenuAsyncCommand;

    public AsyncCommand ShowTopicsMenuAsyncCommand => _showTopicsMenuAsyncCommand ??= CreateAsyncCommand(ShowTopicsMenuAsync, "Unable to show topics menu");

    #endregion ShowTopicsMenuAsyncCommand

    #region ToggleOTVisibilityAsyncCommand

    private AsyncCommand? _toggleOTVisibilityAsyncCommand;

    public AsyncCommand ToggleOTVisibilityAsyncCommand => _toggleOTVisibilityAsyncCommand ??= CreateAsyncCommand(ToggleOTVisibilityAsync, "Unable to toggle visibility");

    #endregion ToggleOTVisibilityAsyncCommand

    #region ToggleNTVisibilityAsyncCommand

    private AsyncCommand? _toggleNTVisibilityAsyncCommand;

    public AsyncCommand ToggleNTVisibilityAsyncCommand => _toggleNTVisibilityAsyncCommand ??= CreateAsyncCommand(ToggleNTVisibilityAsync, "Unable to toggle visibility");

    #endregion ToggleNTVisibilityAsyncCommand

    #region ToggleBMVisibilityAsyncCommand

    private AsyncCommand? _toggleBMVisibilityAsyncCommand;

    public AsyncCommand ToggleBMVisibilityAsyncCommand => _toggleBMVisibilityAsyncCommand ??= CreateAsyncCommand(ToggleBMVisibilityAsync, "Unable to toggle visibility");

    #endregion ToggleBMVisibilityAsyncCommand

    #region ToggleDCVisibilityAsyncCommand

    private AsyncCommand? _toggleDCVisibilityAsyncCommand;

    public AsyncCommand ToggleDCVisibilityAsyncCommand => _toggleDCVisibilityAsyncCommand ??= CreateAsyncCommand(ToggleDCVisibilityAsync, "Unable to toggle visibility");

    #endregion ToggleNTVisibilityAsyncCommand

    #region AllTopics

    private ListItem<ContentTopic>[] _allTopics = [];

    public ListItem<ContentTopic>[] AllTopics
    {
        get => _allTopics;
        set => SetProperty(ref _allTopics, value);
    }

    #endregion AllTopics

    #region AllTopicItems

    private ContentTopicItem[] _allTopicItems = [];

    public ContentTopicItem[] AllTopicItems
    {
        get => _allTopicItems;
        set => SetProperty(ref _allTopicItems, value);
    }

    #endregion AllTopics

    #region SelectedTopic

    private ListItem<ContentTopic>? _selectedTopic;

    public ListItem<ContentTopic>? SelectedTopic
    {
        get => _selectedTopic;
        set => SetProperty(ref _selectedTopic, value,
            v => FilterTopicItemsAsync(TopicItemsFilterText)
                );
    }

    #endregion SelectedTopicItem

    #region TopicItemsFilterText

    private string _topicItemsFilterText = "";

    public string TopicItemsFilterText
    {
        get => _topicItemsFilterText;
        set => SetProperty(ref _topicItemsFilterText, value,
            FilterTopicItemsAsync
                );
    }

    #endregion TopicItemsFilterText

    #region CopyTopicItemAsyncCommand

    private AsyncCommand<ContentTopicItem?>? _copyTopicItemAsyncCommand;

    public AsyncCommand<ContentTopicItem?> CopyTopicItemAsyncCommand => _copyTopicItemAsyncCommand ??= CreateAsyncCommand<ContentTopicItem?>(CopyTopicItemAsync, "Unable to copy topic item");

    #endregion CopyTopicItemAsyncCommand

    #region TopicSelectedAsyncCommand

    private AsyncCommand<ListItem<ContentTopic>>? _topicSelectedAsyncCommand;

    public AsyncCommand<ListItem<ContentTopic>> TopicSelectedAsyncCommand => _topicSelectedAsyncCommand ??= CreateAsyncCommand<ListItem<ContentTopic>>(TopicSelectedAsync, "Unable to process topic");

    #endregion TopicSelectedAsyncCommand

    #region TopicItemSelectedAsyncCommand

    private AsyncCommand<ContentTopicItem?>? _topicItemSelectedAsyncCommand;

    public AsyncCommand<ContentTopicItem?> TopicItemSelectedAsyncCommand => _topicItemSelectedAsyncCommand ??= CreateAsyncCommand<ContentTopicItem?>(TopicItemSelectedAsync, "Unable to process topic item");

    #endregion TopicItemSelectedAsyncCommand

    #endregion

    #region Public Methods

    public async Task InitializeAsync()
    {
        await Task.Yield();

        // Force the value to be updated when command is called
        _isMenuOpen = false;

        ShowTopicsMenuAsyncCommand
            .Execute(null);
    }

    #endregion

    #region Private Variables

    private async Task ShowTopicsMenuAsync()
    {
        await DispatchAsync(() => IsMenuOpen = !IsMenuOpen);

        if (AllTopics.Length == 0 && TopicsFilterText == "")
        {
            await InitializeTopicsAsync();
        }
    }

    private async Task InitializeTopicsAsync()
    {
        await DispatchAsync(() => IsTopicsInitializing = true);
        await Task.Delay(250);

        await LoadTopicsAsync();
        await FilterTopicsAsync("");
        await Task.Delay(250);

        await DispatchAsync(() => IsTopicsInitializing = false);
    }

    private async Task FilterTopicsAsync(string filter)
    {
        var matchingItems = _allLoadedTopics
            .AsEnumerable();

        if (string.IsNullOrWhiteSpace(filter) == false)
        {
            matchingItems = matchingItems
                .Where(x => x.Topic.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        var items = matchingItems
            .Select(x => new ListItem<ContentTopic>(x, x.Topic))
            .ToArray();

        await DispatchAsync(() => AllTopics = items);
        await SelectTopicAsync();
    }

    private async Task SelectTopicAsync()
    {
        if (AllTopics.Length > 0 && AllTopics.Contains(SelectedTopic))
        {
            return;
        }

        var item = AllTopics.FirstOrDefault();

        await DispatchAsync(() => SelectedTopic = item);

        if (item != null)
        {
            await ApplyItemSelectionAsync(item);
        }

        await FilterTopicItemsAsync(TopicItemsFilterText);
    }

    private async Task LoadTopicsAsync()
    {
        var data = await _fileService.LoadDataAsync("Topics/topics.json");
        var items = data.DeserializeFromJson<ContentTopic[]>()
                    ?? [];

        _allLoadedTopics = items;
    }

    private Task ApplyItemSelectionAsync(ListItem<ContentTopic> item)
    {
        return DispatchAsync(() =>
            {
                AllTopics.ForEach(x => x.IsSelected = false);
                item.IsSelected = true;
            });
    }

    private async Task FilterTopicItemsAsync(string filter)
    {
        if (SelectedTopic == null)
        {
            return;
        }

        filter = (filter ?? "").ToLower();

        var matches = SelectedTopic.Item.Items
            .Where(x => x.Text.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Where(x => (IsOTVisible && x.Book.IsOldTestament()) ||
                        (IsNTVisible && x.Book.IsNewTestament()) ||
                        (IsBMVisible && x.Book.IsBookOfMormon()) ||
                        (IsDCVisible && x.Book.IsDoctrineAndCovenants()))
            .OrderByDescending(x => x.Book)
            .ToArray();

        await DispatchAsync(() => AllTopicItems = matches);
    }

    private Task ToggleOTVisibilityAsync()
    {
        IsOTVisible = !IsOTVisible;
        return Task.CompletedTask;
    }

    private Task ToggleNTVisibilityAsync()
    {
        IsNTVisible = !IsNTVisible;
        return Task.CompletedTask;
    }

    private Task ToggleBMVisibilityAsync()
    {
        IsBMVisible = !IsBMVisible;
        return Task.CompletedTask;
    }

    private Task ToggleDCVisibilityAsync()
    {
        IsDCVisible = !IsDCVisible;
        return Task.CompletedTask;
    }

    private async Task CopyTopicItemAsync(ContentTopicItem? item)
    {
        if (item == null)
        {
            return;
        }

        var text = item.ConvertToFormattedText();
        await CopyItemToClipboardAsync(text);
    }

    private async Task TopicSelectedAsync(ListItem<ContentTopic>? item)
    {
        await DispatchAsync(() =>
            {
                SelectedTopic = item;
                IsMenuOpen = false;
            });

        if (item != null)
        {
            await ApplyItemSelectionAsync(item);
        }
    }

    private static async Task TopicItemSelectedAsync(ContentTopicItem? item)
    {
        if (item == null)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(DisplayPage),
                new Dictionary<string, object>()
                {
                    ["ScriptureBook"] = item.Book,
                    ["SearchText"] = item.Verse,
                });
    }

    #endregion
}
