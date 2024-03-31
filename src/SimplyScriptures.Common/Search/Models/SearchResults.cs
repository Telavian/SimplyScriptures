using SimplyScriptures.Common.Services.TextSearch.Models;

namespace SimplyScriptures.Common.Search.Models;

public class SearchResults
{
    public SearchMatch[] AllMatches { get; init; } = [];
    public SearchMatchMode MatchMode { get; set; }
}
