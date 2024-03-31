using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService;
using SimplyScriptures.Common.Services.TextSearch;
using SimplyScriptures.Common.Services.TextSearch.Models;
using SimplyScriptures.Console.Extensions;
using SimplyScriptures.Console.Models;

namespace SimplyScriptures.Console.Loader;

public class BookProcessor
{
    #region Private Methods

    private static readonly LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;
    private static readonly Analyzer _analyzer = new StandardAnalyzer(_luceneVersion);

    private static readonly TextSearchService _searchService;

    private static readonly string _rootPath = @"G:\SimplyScriptures\SimplyScriptures\SimplyScriptures.Blazor\wwwroot";

    #endregion

    #region Constructors

    static BookProcessor()
    {
        var fileService = new FileSystemFileService(_rootPath);
        _searchService = new TextSearchService(fileService, () => Task.CompletedTask);
    }

    #endregion

    #region Public Methods

    public async Task ProcessAllBookIndexDataAsync()
    {
        var books = Enum.GetValues<ScriptureBook>();
        var items = new List<SearchItem>();

        foreach (var book in books)
        {
            await ProcessBookIndexDataAsync(book, items)
                ;
        }

        var path = Path.Combine(_rootPath, @"Scriptures\_Index");
        using (var directory = FSDirectory.Open(path))
        {
            IndexBookData(items, directory);
        }
    }

    public async Task<SearchMatch[]> ProcessAllBookExactSearchAsync(string text)
    {
        await _searchService.InitializeAsync()
            ;

        var allMatches = new List<SearchMatch>();

        var matches = await _searchService.FindExactMatchesAsync(text)
            ;
        allMatches.AddRange(matches);

        return allMatches
            .ToArray();
    }

    public async Task<SearchMatch[]> ProcessAllBookPhraseSearchAsync(string text)
    {
        await _searchService.InitializeAsync()
            ;

        var allMatches = new List<SearchMatch>();

        var matches = await _searchService.FindPhraseMatchesAsync(text)
            ;
        allMatches.AddRange(matches);

        return allMatches
            .ToArray();
    }

    public async Task<SearchMatch[]> ProcessAllBookScriptureSearchAsync(string text)
    {
        await _searchService.InitializeAsync()
            ;

        var allMatches = new List<SearchMatch>();

        var matches = await _searchService.FindScriptureMatchesAsync(text)
            ;
        allMatches.AddRange(matches);

        return allMatches
            .ToArray();
    }

    #endregion

    #region Private Methods

    private static Task<string> LoadBookHtmlAsync(string path)
    {
        path = path.Replace("./", $"{_rootPath}/");
        return File.ReadAllTextAsync(path)
            ;
    }

    private async Task ProcessBookIndexDataAsync(ScriptureBook book, List<SearchItem> items)
    {
        var htmlFilename = book.ToHtmlPath(false);

        if (string.IsNullOrWhiteSpace(htmlFilename) || 
            (book != ScriptureBook.DC_Sections && book.ToString().StartsWith("DC_Section")) ||
            (book != ScriptureBook.DC_Lectures && book.ToString().StartsWith("DC_Lecture")))
        {
            return;
        }

        var html = await LoadBookHtmlAsync(htmlFilename)
            ;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var info = new BookPositionInfo();

        var startCount = items.Count();
        foreach (var child in doc.DocumentNode.ChildNodes)
        {
            ProcessBookNode(book, items, child, info);
        }

        var endCount = items.Count;
        System.Console.WriteLine($"Added {endCount - startCount} items for book {book}");
    }

    private void ProcessBookNode(ScriptureBook book, List<SearchItem> items, HtmlNode node, BookPositionInfo info)
    {
        if (node.Name == "head")
        {
            return;
        }

        var nodeText = node.GetFullInnerText().Trim();
        if (node.Name == "book")
        {
            info.Book = ParseScriptureBook(book, nodeText);
        }
        switch (node.Name)
        {
            case "sectionnumber" when nodeText.Length > 0:
                info.Book = ParseScriptureBook(book, nodeText);
                break;
            case "chapter":
                info.Chapter = int.Parse(nodeText);
                break;
            case "sectionnumber" when nodeText.Length > 0:
                info.Chapter = int.Parse(nodeText);
                break;
            case "verse":
                info.Verse = int.Parse(nodeText.Replace("(", "").Replace(")", ""));
                break;
        }

        var validChildrenCount = node.ChildNodes
            .Select(x => x.InnerText.Trim())
            .Count(x => x.Length > 0);

        var isTextHolder = node.ChildNodes.Count == 1 && node.ChildNodes[0].NodeType == HtmlNodeType.Text;

        if (node.Name != "html" && node.Name != "body" && node.NodeType != HtmlNodeType.Text && (validChildrenCount != 1 || isTextHolder))
        {
            var text = node.GetFullInnerText().Trim();

            switch (text.Length)
            {
                case >= short.MaxValue:
                    throw new Exception("Text is too long");
                case > 0 when info.Book != ScriptureBook.None:
                    {
                        var newItem = new SearchItem
                        {
                            Text = text,
                            XPath = BuildNodeXPath(book, node),
                            Book = info.Book,
                            Chapter = info.Chapter,
                            Verse = info.Verse
                        };
                        items.Add(newItem);
                        break;
                    }
            }
        }

        if (isTextHolder)
        {
            return;
        }

        foreach (var child in node.ChildNodes)
        {
            ProcessBookNode(book, items, child, info);
        }
    }

    private string BuildNodeXPath(ScriptureBook book, HtmlNode node)
    {
        var xpath = node.XPath;

        return xpath
            .Replace("body[1]", $"body[@book='{book}']")
            .Replace("#text", "text()");
    }

    private ScriptureBook ParseScriptureBook(ScriptureBook scripture, string name)
    {
        if (scripture.IsOldTestament1())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_About;
            }

            if (string.Equals("Genesis", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Genesis;
            }

            if (string.Equals("Exodus", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Exodus;
            }

            if (string.Equals("Leviticus", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Leviticus;
            }

            if (string.Equals("Numbers", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Numbers;
            }

            if (string.Equals("Deuteronomy", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Deuteronomy;
            }

            if (string.Equals("Joshua", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Joshua;
            }

            if (string.Equals("Judges", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Judges;
            }

            if (string.Equals("Ruth", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_Ruth;
            }

            if (string.Equals("Descendants of Terah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_DescendantsTerah;
            }

            if (string.Equals("Masoretic TimeLine", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_MasoreticTimeline;
            }

            if (string.Equals("Septuagint Timeline", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT1_SeptuagintTimeline;
            }
        }
        else if (scripture.IsOldTestament2())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_About;
            }

            if (string.Equals("1 Samuel", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_1Samuel;
            }

            if (string.Equals("2 Samuel", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_2Samuel;
            }

            if (string.Equals("1 Kings", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_1Kings;
            }

            if (string.Equals("2 Kings", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_2Kings;
            }

            if (string.Equals("1 Chronicles", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_1Chronicles;
            }

            if (string.Equals("2 Chronicles", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_2Chronicles;
            }

            if (string.Equals("Ezra", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Ezra;
            }

            if (string.Equals("Nehemiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Nehemiah;
            }

            if (string.Equals("Esther", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Esther;
            }

            if (string.Equals("Job", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Job;
            }

            if (string.Equals("Psalms", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Psalms;
            }

            if (string.Equals("Proverbs", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Proverbs;
            }

            if (string.Equals("Ecclesiastes", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_Ecclesiastes;
            }

            if (string.Equals("Chronology of Kings", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT2_ChronologyKings;
            }
        }
        else if (scripture.IsOldTestament3())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_About;
            }

            if (string.Equals("Isaiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Isaiah;
            }

            if (string.Equals("Jeremiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Jeremiah;
            }

            if (string.Equals("The Lamentations of Jeremiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Lamentations;
            }

            if (string.Equals("Ezekiel", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Ezekiel;
            }

            if (string.Equals("Daniel", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Daniel;
            }

            if (string.Equals("Hosea", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Hosea;
            }

            if (string.Equals("Joel", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Joel;
            }

            if (string.Equals("Amos", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Amos;
            }

            if (string.Equals("Obadiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Obadiah;
            }

            if (string.Equals("Jonah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Jonah;
            }

            if (string.Equals("Micah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Micah;
            }

            if (string.Equals("Nahum", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Nahum;
            }

            if (string.Equals("Habakkuk", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Habakkuk;
            }

            if (string.Equals("Zephaniah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Zephaniah;
            }

            if (string.Equals("Haggai", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Haggai;
            }

            if (string.Equals("Zechariah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Zechariah;
            }

            if (string.Equals("Malachi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_Malachi;
            }

            if (string.Equals("Chronology of Kings", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.OT3_ChronologyKings;
            }
        }
        else if (scripture.IsNewTestament())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_About;
            }

            if (string.Equals("The Testimony of Matthew", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Matthew;
            }

            if (string.Equals("The Testimony of Mark", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Mark;
            }

            if (string.Equals("The Testimony of Luke", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Luke;
            }

            if (string.Equals("The Testimony of John", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_John;
            }

            if (string.Equals("The Acts of the Apostles", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Acts;
            }

            if (string.Equals("Romans", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Romans;
            }

            if (string.Equals("1 Corinthians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_1Corinthians;
            }

            if (string.Equals("2 Corinthians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_2Corinthians;
            }

            if (string.Equals("Galatians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Galatians;
            }

            if (string.Equals("Ephesians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Ephesians;
            }

            if (string.Equals("Philippians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Philippians;
            }

            if (string.Equals("Colossians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Colossians;
            }

            if (string.Equals("1 Thessalonians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_1Thessalonians;
            }

            if (string.Equals("2 Thessalonians", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_2Thessalonians;
            }

            if (string.Equals("1 Timothy", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_1Timothy;
            }

            if (string.Equals("2 Timothy", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_2Timothy;
            }

            if (string.Equals("Titus", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Titus;
            }

            if (string.Equals("Philemon", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Philemon;
            }

            if (string.Equals("Hebrews", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Hebrews;
            }

            if (string.Equals("James", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_James;
            }

            if (string.Equals("1 Peter", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_1Peter;
            }

            if (string.Equals("2 Peter", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_2Peter;
            }

            if (string.Equals("1 John", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_1John;
            }

            if (string.Equals("2 John", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_2John;
            }

            if (string.Equals("3 John", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_3John;
            }

            if (string.Equals("Jude", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Jude;
            }

            if (string.Equals("Revelation", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.NT_Revelation;
            }
        }
        else if (scripture.IsBookOfMormon())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_About;
            }

            if (string.Equals("The First Book of Nephi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_1Nephi;
            }

            if (string.Equals("The Second Book of Nephi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_2Nephi;
            }

            if (string.Equals("The Book of Jacob", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Jacob;
            }

            if (string.Equals("The Book of Enos", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Enos;
            }

            if (string.Equals("The Book of Jarom", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Jarom;
            }

            if (string.Equals("The Book of Omni", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Omni;
            }

            if (string.Equals("The Words of Mormon", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Words;
            }

            if (string.Equals("The Book of Mosiah", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Mosiah;
            }

            if (string.Equals("The Book of Alma", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Alma;
            }

            if (string.Equals("The Book of Helaman", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Helaman;
            }

            if (string.Equals("The Third Book of Nephi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_3Nephi;
            }

            if (string.Equals("The FOURTH Book of Nephi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_4Nephi;
            }

            if (string.Equals("The Book of Mormon", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Mormon;
            }

            if (string.Equals("The Book of Ether", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Ether;
            }

            if (string.Equals("The Book of Moroni", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_Moroni;
            }

            if (string.Equals("The Testimony of Three Witnesses", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_3Witness;
            }

            if (string.Equals("The Testimony of Eight Witnesses", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.BM_8Witness;
            }

            // Special case
            if (string.Equals("The Book of Nephi", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.None;
            }
        }
        else if (scripture.IsDoctrineAndCovenants())
        {
            if (string.Equals("About This Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_About;
            }

            if (string.Equals("Preface to 1835 Edition", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Preface;
            }

            if (string.Equals("Lecture 1 – Faith Defined", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture1;
            }

            if (string.Equals("Lecture 2 – The Object of Faith", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture2;
            }

            if (string.Equals("Lecture 3 – The Character of God", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture3;
            }

            if (string.Equals("Lecture 4 – The Attributes of God", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture4;
            }

            if (string.Equals("Lecture 5 – The Godhead", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture5;
            }

            if (string.Equals("Lecture 6 – The Law of Sacrifice", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture6;
            }

            if (string.Equals("Lecture 7 – The Effects of Faith", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture7;
            }

            if (string.Equals("Lecture 7 – The Effects of Faith", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lecture7;
            }

            if (string.Equals("The Book of Abraham", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Abraham;
            }

            if (string.Equals("The Book of Moses", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Moses;
            }

            if (string.Equals("Matthew 25 – Inspired Version", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Matthew25;
            }

            if (string.Equals("Articles of Faith", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Articles;
            }

            if (string.Equals("1", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section1;
            }

            if (string.Equals("2", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section2;
            }

            if (string.Equals("3", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section3;
            }

            if (string.Equals("4", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section4;
            }

            if (string.Equals("5", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section5;
            }

            if (string.Equals("6", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section6;
            }

            if (string.Equals("7", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section7;
            }

            if (string.Equals("8", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section8;
            }

            if (string.Equals("9", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section9;
            }

            if (string.Equals("10", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section10;
            }

            if (string.Equals("11", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section11;
            }

            if (string.Equals("12", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section12;
            }

            if (string.Equals("13", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section13;
            }

            if (string.Equals("14", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section14;
            }

            if (string.Equals("15", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section15;
            }

            if (string.Equals("16", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section16;
            }

            if (string.Equals("17", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section17;
            }

            if (string.Equals("18", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section18;
            }

            if (string.Equals("19", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section19;
            }

            if (string.Equals("20", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section20;
            }

            if (string.Equals("21", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section21;
            }

            if (string.Equals("22", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section22;
            }

            if (string.Equals("23", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section23;
            }

            if (string.Equals("24", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section24;
            }

            if (string.Equals("25", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section25;
            }

            if (string.Equals("26", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section26;
            }

            if (string.Equals("27", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section27;
            }

            if (string.Equals("28", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section28;
            }

            if (string.Equals("29", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section29;
            }

            if (string.Equals("30", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section30;
            }

            if (string.Equals("31", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section31;
            }

            if (string.Equals("32", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section32;
            }

            if (string.Equals("33", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section33;
            }

            if (string.Equals("34", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section34;
            }

            if (string.Equals("35", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section35;
            }

            if (string.Equals("36", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section36;
            }

            if (string.Equals("37", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section37;
            }

            if (string.Equals("38", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section38;
            }

            if (string.Equals("39", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section39;
            }

            if (string.Equals("40", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section40;
            }

            if (string.Equals("41", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section41;
            }

            if (string.Equals("42", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section42;
            }

            if (string.Equals("43", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section43;
            }

            if (string.Equals("44", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section44;
            }

            if (string.Equals("45", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section45;
            }

            if (string.Equals("46", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section46;
            }

            if (string.Equals("47", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section47;
            }

            if (string.Equals("48", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section48;
            }

            if (string.Equals("49", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section49;
            }

            if (string.Equals("50", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section50;
            }

            if (string.Equals("51", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section51;
            }

            if (string.Equals("52", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section52;
            }

            if (string.Equals("53", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section53;
            }

            if (string.Equals("54", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section54;
            }

            if (string.Equals("55", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section55;
            }

            if (string.Equals("56", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section56;
            }

            if (string.Equals("57", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section57;
            }

            if (string.Equals("58", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section58;
            }

            if (string.Equals("59", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section59;
            }

            if (string.Equals("60", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section60;
            }

            if (string.Equals("61", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section61;
            }

            if (string.Equals("62", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section62;
            }

            if (string.Equals("63", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section63;
            }

            if (string.Equals("64", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section64;
            }

            if (string.Equals("65", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section65;
            }

            if (string.Equals("66", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section66;
            }

            if (string.Equals("67", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section67;
            }

            if (string.Equals("68", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section68;
            }

            if (string.Equals("69", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section69;
            }

            if (string.Equals("70", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section70;
            }

            if (string.Equals("71", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section71;
            }

            if (string.Equals("72", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section72;
            }

            if (string.Equals("73", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section73;
            }

            if (string.Equals("74", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section74;
            }

            if (string.Equals("75", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section75;
            }

            if (string.Equals("76", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section76;
            }

            if (string.Equals("77", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section77;
            }

            if (string.Equals("78", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section78;
            }

            if (string.Equals("79", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section79;
            }

            if (string.Equals("80", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section80;
            }

            if (string.Equals("81", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section81;
            }

            if (string.Equals("82", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section82;
            }

            if (string.Equals("83", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section83;
            }

            if (string.Equals("84", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section84;
            }

            if (string.Equals("85", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section85;
            }

            if (string.Equals("86", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section86;
            }

            if (string.Equals("87", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section87;
            }

            if (string.Equals("88", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section88;
            }

            if (string.Equals("89", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section89;
            }

            if (string.Equals("90", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section90;
            }

            if (string.Equals("91", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section91;
            }

            if (string.Equals("92", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section92;
            }

            if (string.Equals("93", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section93;
            }

            if (string.Equals("94", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section94;
            }

            if (string.Equals("95", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section95;
            }

            if (string.Equals("96", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section96;
            }

            if (string.Equals("97", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section97;
            }

            if (string.Equals("98", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section98;
            }

            if (string.Equals("99", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section99;
            }

            if (string.Equals("100", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section100;
            }

            if (string.Equals("101", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section101;
            }

            if (string.Equals("102", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section102;
            }

            if (string.Equals("103", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section103;
            }

            if (string.Equals("104", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section104;
            }

            if (string.Equals("105", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section105;
            }

            if (string.Equals("106", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section106;
            }

            if (string.Equals("107", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section107;
            }

            if (string.Equals("108", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section108;
            }

            if (string.Equals("109", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section109;
            }

            if (string.Equals("110", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section110;
            }

            if (string.Equals("111", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section111;
            }

            if (string.Equals("112", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section112;
            }

            if (string.Equals("113", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section113;
            }

            if (string.Equals("114", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section114;
            }

            if (string.Equals("115", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section115;
            }

            if (string.Equals("116", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section116;
            }

            if (string.Equals("117", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section117;
            }

            if (string.Equals("118", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section118;
            }

            if (string.Equals("119", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section119;
            }

            if (string.Equals("120", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section120;
            }

            if (string.Equals("121", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section121;
            }

            if (string.Equals("122", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section122;
            }

            if (string.Equals("123", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section123;
            }

            if (string.Equals("124", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section124;
            }

            if (string.Equals("125", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section125;
            }

            if (string.Equals("126", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section126;
            }

            if (string.Equals("127", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section127;
            }

            if (string.Equals("128", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section128;
            }

            if (string.Equals("129", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section129;
            }

            if (string.Equals("130", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section130;
            }

            if (string.Equals("131", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section131;
            }

            if (string.Equals("132", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section132;
            }

            if (string.Equals("133", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section133;
            }

            if (string.Equals("134", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section134;
            }

            if (string.Equals("135", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section135;
            }

            if (string.Equals("136", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section136;
            }

            if (string.Equals("137", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section137;
            }

            if (string.Equals("138", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Section138;
            }

            // Special case
            if (string.Equals("Lectures on Faith", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Lectures;
            }

            if (string.Equals("Reception of Sections by Year", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Reception;
            }

            if (string.Equals("Order of Sections in 1835 Doctrine and Covenants", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_Order;
            }

            if (string.Equals("Pearl of Great Price", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.None;
            }

            if (string.Equals("Appendix", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.None;
            }

            if (string.Equals("Joseph Smith History – 1832 Account", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_1832Account;
            }

            if (string.Equals("Joseph Smith History – 1835 Account", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_1835Account;
            }

            if (string.Equals("Joseph Smith History – 1838 Account", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_1838Account;
            }

            if (string.Equals("Joseph Smith History – 1842 Account", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_1842Account;
            }

            if (string.Equals("Account of Moroni’s Visit in 1823", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_MoroniVisit;
            }

            if (string.Equals("Letter to the Church from Liberty Jail", name, StringComparison.OrdinalIgnoreCase))
            {
                return ScriptureBook.DC_LetterLibertyJail;
            }
        }

        throw new Exception($"Unable to determine scripture book: {scripture}, {name}");
    }

    private static void IndexBookData(IReadOnlyCollection<SearchItem> bookItems, FSDirectory directory)
    {
        System.Console.WriteLine($"Indexing {bookItems.Count} documents");
        var config = new IndexWriterConfig(_luceneVersion, _analyzer);
        config.OpenMode = OpenMode.CREATE;

        using (var writer = new IndexWriter(directory, config))
        {
            foreach (var item in bookItems)
            {
                var text = item.Text;
                text = Regex.Replace(text, @"\d{1,}", @" $& ");

                var doc = new Document
                {
                    new TextField(nameof(SearchItem.Text), text, Field.Store.YES),
                    new StringField($"{nameof(SearchItem.XPath)}", item.XPath, Field.Store.YES),
                    new StringField($"{nameof(SearchItem.Book)}", item.Book.ToString(), Field.Store.YES),
                    new Int32Field($"{nameof(SearchItem.Chapter)}", item.Chapter, Field.Store.YES),
                    new Int32Field($"{nameof(SearchItem.Verse)}", item.Verse, Field.Store.YES),
                };
                writer.AddDocument(doc);
            }

            writer.Flush(true, true);
            writer.Commit();
        }
    }

    #endregion
}
