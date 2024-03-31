using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Search.Models;

namespace SimplyScriptures.Common.Models;

public class BookInfo : IEquatable<BookInfo>
{
    public ScriptureBook Book { get; init; }

    public string HtmlPath { get; set; } = "";
    public string Html { get; set; } = "";
    public ContentItem[] ContentItems { get; set; } = [];

    #region Public Methods

    public bool Equals(BookInfo? other)
    {
        return other is not null && (ReferenceEquals(this, other) || Book == other.Book);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        return ReferenceEquals(this, obj) || obj.GetType() == GetType() && Equals((BookInfo)obj);
    }

    public override int GetHashCode()
    {
        return (int)Book;
    }

    #endregion
}
