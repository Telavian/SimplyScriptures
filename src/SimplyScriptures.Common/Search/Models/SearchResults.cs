using SimplyScriptures.Common.Services.TextSearch.Models;

namespace SimplyScriptures.Common.Search.Models;

public class SearchResults
{
    public SearchMatch[] AllMatches { get; init; } = Array.Empty<SearchMatch>();
    public SearchMatchMode MatchMode { get; set; }
}
