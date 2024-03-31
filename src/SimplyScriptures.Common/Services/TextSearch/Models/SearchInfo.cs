using System.Text.RegularExpressions;
using SimplyScriptures.Common.Enums;

namespace SimplyScriptures.Common.Services.TextSearch.Models;

public class SearchInfo
{
    public string[] SearchKeywords { get; }
    public SearchMode Mode { get; }
    public Regex? KeywordReplacerRegex { get; }

    public SearchInfo(SearchMode mode, string[] searchKeywords)
    {
        Mode = mode;
        SearchKeywords = searchKeywords;

        if (SearchKeywords.Length > 0)
        {
            var keywordText = string.Join("|", SearchKeywords);
            KeywordReplacerRegex = new Regex(keywordText, RegexOptions.IgnoreCase);
        }
    }
}
