using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Search.Models;

namespace SimplyScriptures.Common.Models;

public class BookInfo : IEquatable<BookInfo>
{
    public ScriptureBook Book { get; init; }

    public string HtmlPath { get; set; } = "";
    public string Html { get; set; } = "";
    public ContentItem[] ContentItems { get; set; } = Array.Empty<ContentItem>();

    #region Public Methods

    public bool Equals(BookInfo? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Book == other.Book;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((BookInfo)obj);
    }

    public override int GetHashCode()
    {
        return (int)Book;
    }

    #endregion
}
