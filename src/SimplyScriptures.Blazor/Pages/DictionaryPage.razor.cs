using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using CurrieTechnologies.Razor.Clipboard;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Models;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Pages.Common;
using SimplyScriptures.Services.ApplicationState.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimplyScriptures.Pages;

[SuppressMessage("ReSharper", "AsyncApostle.AsyncAwaitMayBeElidedHighlighting")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DictionaryPageBase : ViewModelBase
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

    public static DictionaryWord[] AllWords { get; private set; } = [];

    #region IsDisplayInverted

    private bool _isDisplayInverted = false;

    public bool IsDisplayInverted
    {
        get => _isDisplayInverted;
        set => UpdateProperty(ref _isDisplayInverted, value,
            v => SaveSettingAsync(nameof(IsDisplayInverted), v));
    }

    #endregion IsDisplayInverted

    #region IsWordsLoading

    private bool _isWordsLoading = true;

    public bool IsWordsLoading
    {
        get => _isWordsLoading;
        set => UpdateProperty(ref _isWordsLoading, value);
    }

    #endregion IsWordsLoading

    #region IsDictionaryOpen

    private bool _isDictionaryOpen = true;

    public bool IsDictionaryOpen
    {
        get => _isDictionaryOpen;
        set => UpdateProperty(ref _isDictionaryOpen, value);
    }

    #endregion IsDictionaryOpen

    #region SelectedWord

    private DictionaryWord? _selectedWord = null;

    public DictionaryWord? SelectedWord
    {
        get => _selectedWord;
        set => UpdateProperty(ref _selectedWord, value,
            v => SelectedWordValue = v);
    }

    #endregion SelectedWord

    #region SelectedWordValue

    private object? _selectedWordValue = null;

    public object? SelectedWordValue
    {
        get => _selectedWordValue;
        set => UpdateProperty(ref _selectedWordValue, value,
            v => SelectedWord = v as DictionaryWord);
    }

    #endregion SelectedWordValue

    #region WordsFilterText

    private string _WordsFilterText = "";

    public string WordsFilterText
    {
        get => _WordsFilterText;
        set => UpdateProperty(ref _WordsFilterText, value,
            f => FilterWordsAsync());
    }

    #endregion WordsFilterText

    #region FilteredWords

    private DictionaryWord[] _filteredWords = [];

    public DictionaryWord[] FilteredWords
    {
        get => _filteredWords;
        set => UpdateProperty(ref _filteredWords, value);
    }

    #endregion FilteredWords

    #region WordSelectedAsyncCommand

    private Func<DictionaryWord, Task>? _wordSelectedAsyncCommand;

    public Func<DictionaryWord, Task> WordSelectedAsyncCommand => _wordSelectedAsyncCommand ??= CreateEventCallbackAsyncCommand<DictionaryWord>(WordSelectedAsync, "Unable to select word");

    #endregion WordSelectedAsyncCommand

    #region ToggleDictionaryVisibilityAsyncCommand

    private Func<Task>? _toggleDictionaryVisibilityAsyncCommand;

    public Func<Task> ToggleDictionaryVisibilityAsyncCommand => _toggleDictionaryVisibilityAsyncCommand ??= CreateEventCallbackAsyncCommand(ToggleDictionaryVisibilityAsync, "Unable to toggle dictionary visibility");

    #endregion ToggleDictionaryVisibilityAsyncCommand

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
            await LoadWordsAsync();
            await ProcessPageParametersAsync();
            await AlertIfMobileAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected MarkupString ProcessItemText(DictionaryWord? item)
    {
        var html = item?.ConvertToFullHtml() ?? "";
        return new MarkupString(html);
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
            var word = queryParameters.Get("w");

            await ProcessPageParametersAsync(word ?? "");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to process page parameters: {ex.Message}");
        }
    }

    private async Task ProcessPageParametersAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        SelectedWord = AllWords
            .FirstOrDefault(x => x.Word.Equals(word, StringComparison.OrdinalIgnoreCase));

        await RefreshAsync();
    }

    private Task FilterWordsAsync()
    {
        var filterText = (WordsFilterText ?? "").Trim();
        var filteredItems = string.IsNullOrWhiteSpace(filterText)
            ? AllWords.Take(100).ToArray()
            : AllWords
                .Where(x => x.Word.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => filterText.GetDamerauLevenshteinDistance(x.Word))
                .Take(100)
                .ToArray();

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            SelectedWord = FilteredWords
                .FirstOrDefault(x => x.Word.Equals(filterText, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            if (SelectedWord != null && FilteredWords.Contains(SelectedWord) == false)
            {
                SelectedWord = FilteredWords
                    .FirstOrDefault();
            }
        }

        FilteredWords = filteredItems;
        return RefreshAsync();
    }

    private async Task LoadWordsAsync()
    {
        try
        {
            await LoadAllWordsAsync();

            FilteredWords = AllWords.Take(100).ToArray();
            WordsFilterText = "";
            IsWordsLoading = false;
            SelectedWord = FilteredWords.FirstOrDefault();

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to load words: {ex.Message}");
        }
    }

    private async Task LoadAllWordsAsync()
    {
        if (AllWords.Length > 0)
        {
            return;
        }

        var loadedWords = new List<DictionaryWord>();

        for (var x = 0; x < 10; ++x)
        {
            var fileName = $"./Scriptures/Dictionary/words{x + 1}.json";
            var data = await _fileService!.LoadDataAsync(fileName);

            using (var memStream = new MemoryStream(data))
            {
                var words = await JsonSerializer.DeserializeAsync<DictionaryWord[]>(memStream)
                    .ConfigureAwait(false) ?? [];

                loadedWords
                    .AddRange(words);
            }

            await RefreshAsync();
            await Task.Delay(15);
        }

        AllWords =
        [
            .. loadedWords.OrderBy(x => x.Word),
        ];
    }

    private void SetPageState()
    {
        _isDisplayInverted = _applicationState!.IsDisplayInverted;
    }

    private Task WordSelectedAsync(DictionaryWord item)
    {
        SelectedWord = item;

        return RefreshAsync();
    }

    private Task ToggleDictionaryVisibilityAsync()
    {
        IsDictionaryOpen = !IsDictionaryOpen;

        return RefreshAsync();
    }

    #endregion
}
