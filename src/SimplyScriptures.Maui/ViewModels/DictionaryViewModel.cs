using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Input;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Models;
using SimplyScriptures.Pages;

namespace SimplyScriptures.ViewModels;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DictionaryViewModel(IFileService fileService) : ViewModelBase
{
    #region Private Variables

    private readonly IFileService _fileService = fileService;
    private DictionaryWord[] _allLoadedWords = [];

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

    #region IsWordsInitializing

    private readonly bool _isWordsInitializing;

    public bool IsWordsInitializing
    {
        get => _isWordsInitializing;
        set => SetProperty(ref _isWordsInitializing, value);
    }

    #endregion IsWordsInitializing

    #region WordsFilterText

    private string _WordsFilterText = "";

    public string WordsFilterText
    {
        get => _WordsFilterText;
        set => SetProperty(ref _WordsFilterText, value,
            FilterWordsAsync
                );
    }

    #endregion WordsFilterText

    #region ShowDictionaryMenuAsyncCommand

    private AsyncRelayCommand? _showDictionaryMenuAsyncCommand;

    public AsyncRelayCommand ShowDictionaryMenuAsyncCommand => _showDictionaryMenuAsyncCommand ??= CreateAsyncCommand(ShowDictionaryMenuAsync, "Unable to show menu");

    #endregion ShowDictionaryMenuAsyncCommand

    #region AllWords

    private ListItem<DictionaryWord>[] _allWords = [];

    public ListItem<DictionaryWord>[] AllWords
    {
        get => _allWords;
        set => SetProperty(ref _allWords, value);
    }

    #endregion AllWords

    #region SelectedWord

    private readonly ListItem<DictionaryWord>? _selectedWord;

    public ListItem<DictionaryWord>? SelectedWord
    {
        get => _selectedWord;
        set => SetProperty(ref _selectedWord, value);
    }

    #endregion SelectedWord

    #region WordSelectedAsyncCommand

    private AsyncRelayCommand<ListItem<DictionaryWord>>? _wordSelectedAsyncCommand;

    public AsyncRelayCommand<ListItem<DictionaryWord>> WordSelectedAsyncCommand => _wordSelectedAsyncCommand ??= CreateAsyncCommand<ListItem<DictionaryWord>>(WordSelectedAsync, "Unable to process word");

    #endregion WordSelectedAsyncCommand
    #endregion
    #region Constructors

    #endregion

    #region Public Methods

    public Task InitializeAsync()
    {
        // Force the value to be updated when command is called
        _isMenuOpen = false;

        return ShowDictionaryMenuAsyncCommand
            .ExecuteAsync(null);
    }

    #endregion

    #region Private Variables

    private async Task ShowDictionaryMenuAsync()
    {
        await DispatchAsync(() => IsMenuOpen = !IsMenuOpen)
            ;

        if (AllWords.Length == 0 && WordsFilterText == "")
        {
            await InitializeWordsAsync()
                ;
        }
    }

    private async Task InitializeWordsAsync()
    {
        await DispatchAsync(() => IsWordsInitializing = true)
            ;

        await Task.Delay(250)
            ;

        await LoadWordsAsync()
            ;

        await FilterWordsAsync("")
            ;

        await Task.Delay(250)
            ;

        await DispatchAsync(() => IsWordsInitializing = false)
            ;
    }

    private async Task FilterWordsAsync(string filter)
    {
        var filteredItems = string.IsNullOrWhiteSpace(filter)
            ? _allLoadedWords.Take(100).ToArray()
            : _allLoadedWords
                .Where(x => x.Word.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => filter.GetDamerauLevenshteinDistance(x.Word))
                .Take(100)
                .ToArray();

        var wordItems = filteredItems
            .Select(x => new ListItem<DictionaryWord>(x, x.Word))
            .ToArray();

        await DispatchAsync(() => AllWords = wordItems)
        ;

        await SelectWordAsync()
            ;
    }

    private async Task SelectWordAsync()
    {
        if (AllWords.Length > 0 && AllWords.Contains(SelectedWord))
        {
            return;
        }

        var item = AllWords.FirstOrDefault();

        await DispatchAsync(() => SelectedWord = item)
            ;

        if (item != null)
        {
            await ApplyItemSelectionAsync(item)
                ;
        }
    }

    private async Task LoadWordsAsync()
    {
        var loadedWords = new List<DictionaryWord>();

        for (var x = 0; x < 10; x++)
        {
            var fileName = $"Dictionary/words{x + 1}.json";
            var data = await _fileService!.LoadDataAsync(fileName)
                ;

            using (var memStream = new MemoryStream(data))
            {
                var words = await memStream.DeserializeFromJsonAsync<DictionaryWord[]>()
                    .ConfigureAwait(false) ?? [];

                loadedWords
                    .AddRange(words);
            }

            await Task.Delay(15)
                ;
        }

        _allLoadedWords =
        [
            .. loadedWords
                        .OrderBy(x => x.Word)
,
        ];
    }

    private async Task WordSelectedAsync(ListItem<DictionaryWord>? item)
    {
        await DispatchAsync(() =>
            {
                SelectedWord = item;
                IsMenuOpen = false;
            })
            ;

        if (item != null)
        {
            await ApplyItemSelectionAsync(item)
                ;
        }
    }

    private Task ApplyItemSelectionAsync(ListItem<DictionaryWord> item)
    {
        return DispatchAsync(() =>
            {
                AllWords.ForEach(x => x.IsSelected = false);
                item.IsSelected = true;
            });
    }

    #endregion
}
