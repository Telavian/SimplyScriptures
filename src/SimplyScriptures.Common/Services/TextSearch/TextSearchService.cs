using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using rm.Trie;
using SimplyScriptures.Common.Enums;
using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Common.Services.FileService.Interfaces;
using SimplyScriptures.Common.Services.TextSearch.Models;
using Directory = System.IO.Directory;

namespace SimplyScriptures.Common.Services.TextSearch;

public class TextSearchService
{
    #region Private Variables

    private static readonly LuceneVersion _luceneVersion = LuceneVersion.LUCENE_48;

    private static FSDirectory? _searchDirectory;

    private static readonly Analyzer _analyzer = new StandardAnalyzer(_luceneVersion);

    private static DirectoryReader? _reader;

    private static IndexSearcher? _searcher;

    private readonly IFileService _fileService;

    private readonly Func<Task> _refresh;

    private bool _isInitialized;

    private readonly int _totalResults;

    private readonly SemaphoreSlim _initializationLock = new SemaphoreSlim(1);
    private readonly SemaphoreSlim _searchLock = new SemaphoreSlim(1);

    private static readonly HashSet<string> _stopWords = StandardAnalyzer.STOP_WORDS_SET
        .Select(x => x.ToLower())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, ScriptureBook> _scriptureBookLookup = InitializeScriptureBookLookup();

    #endregion

    #region Constructors

    public TextSearchService(IFileService fileService, Func<Task> refresh, int totalResults = 5000)
    {
        _fileService = fileService;
        _refresh = refresh;
        _totalResults = totalResults;
    }

    #endregion

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        try
        {
            await _initializationLock.WaitAsync()
                ;
            
            if (_isInitialized)
            {
                return;
            }

            LogMessage("Starting search initialization");
            var timer = Stopwatch.StartNew();

            await InitializeSearchIndexAsync()
                ;
            
            LogMessage($"Search initialization complete: {timer.ElapsedMilliseconds} ms");
            _isInitialized = true;
            await RefreshAsync()
                ;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public Task<SearchMatch[]> FindExactMatchesAsync(string text)
    {
        var lower = text;
        if (string.IsNullOrWhiteSpace(lower))
        {
            return Task.FromResult(Array.Empty<SearchMatch>());
        }

        lower = lower.ToLower();

        return ExecuteSearchAsync(SearchMode.Exact, () =>
        {
            lower = lower.Trim('\"');
            var phraseQueries = new PhraseQuery();
            var num = 0;
            var strArrays = lower.Split(Array.Empty<char>());

            foreach (var str in strArrays)
            {
                if (_stopWords.Contains(str) == false)
                {
                    phraseQueries.Add(new Term("Text", str), num);
                }
                num++;
            }

            var topDoc = _searcher!.Search(phraseQueries, _totalResults);
            var data = lower.Trim().Trim('\"');

            return (topDoc, data, data.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries));
        });
    }

    public Task<SearchMatch[]> FindPhraseMatchesAsync(string text)
    {
        var lower = text;
        if (string.IsNullOrWhiteSpace(lower))
        {
            return Task.FromResult(Array.Empty<SearchMatch>());
        }

        lower = lower.ToLower();

        return ExecuteSearchAsync(SearchMode.Phrase, () =>
        {
            var booleanQueries = new BooleanQuery();
            var strArrays = lower.Split(Array.Empty<char>());

            foreach (var str in strArrays)
            {
                if (!_stopWords.Contains(str))
                {
                    booleanQueries.Add(new TermQuery(new Term("Text", str)), Occur.MUST);
                }
            }

            var topDoc = _searcher!.Search(booleanQueries, _totalResults);
            var chunks = lower.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);

            return new ValueTuple<TopDocs, string, string[]>(topDoc, ConvertToRegexWildcardSearch(chunks), chunks);
        });
    }

    public async Task<SearchMatch[]> FindScriptureMatchesAsync(string text)
    {
        SearchMatch[] searchMatchArray;
        if (!string.IsNullOrWhiteSpace(text))
        {
            text = text.Replace("D&C ", "DC", StringComparison.OrdinalIgnoreCase);
            var parts = text
                .ToLower()
                .TrimToAlphaNumeric()
                .SplitAlphaNumeric();
            var query = BuildScriptureQuery(parts);

            if (query != null)
            {
                searchMatchArray = await ExecuteSearchAsync(SearchMode.Scripture, () =>
                    {
                        var docs = _searcher!.Search(query, _totalResults);
                        return (docs, "", text.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries));
                    })
                    ;
            }
            else
            {
                searchMatchArray = Array.Empty<SearchMatch>();
            }
        }
        else
        {
            searchMatchArray = Array.Empty<SearchMatch>();
        }

        return searchMatchArray;
    }

    #region Private Methods

    private string FindSmallestMatch(string text, string regex)
    {
        var index = 0;
        if (string.IsNullOrWhiteSpace(regex))
        {
            return text;
        }
        var regex1 = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
        var items = new List<string>();

        for (var i = regex1.Match(text, 0); i.Success; i = regex1.Match(text, index))
        {
            var expandedMatch = GetExpandedMatch(text, i.Index, i.Length);
            items.Add(expandedMatch.Trim());
            index = i.Index + 1;
        }

        return items.MinBy(x => x.Length) ?? "";
    }

    private string GetExpandedMatch(string text, int matchIndex, int matchLength)
    {
        var num = text.PreviousWordIndex(matchIndex);
        num = text.PreviousWordIndex(num);
        var num1 = text.NextWordIndex(matchIndex + matchLength);
        num1 = text.NextWordIndex(num1);
        return text.Substring(num, num1 - num + 1).Replace("  ", " ").Replace("  ", " ").Trim(',').Trim();
    }

    private static Dictionary<string, ScriptureBook> InitializeScriptureBookLookup()
    {
        var items = new Dictionary<string, ScriptureBook>(StringComparer.OrdinalIgnoreCase);
        AddLookupItems(items, new[] { "genesis", "gen", "ge" }, ScriptureBook.OT1_Genesis);
        AddLookupItems(items, new[] { "exodus", "ex" }, ScriptureBook.OT1_Exodus);
        AddLookupItems(items, new[] { "leviticus", "lev", "le" }, ScriptureBook.OT1_Leviticus);
        AddLookupItems(items, new[] { "numbers", "num", "numb", "nu" }, ScriptureBook.OT1_Numbers);
        AddLookupItems(items, new[] { "deuteronomy", "deut", "de" }, ScriptureBook.OT1_Deuteronomy);
        AddLookupItems(items, new[] { "joshua", "josh" }, ScriptureBook.OT1_Joshua);
        AddLookupItems(items, new[] { "judges", "judge", "ju" }, ScriptureBook.OT1_Judges);
        AddLookupItems(items, new[] { "ruth", "ru" }, ScriptureBook.OT1_Ruth);
        AddLookupItems(items, new[] { "1samuel", "1sam", "1s" }, ScriptureBook.OT2_1Samuel);
        AddLookupItems(items, new[] { "2samuel", "2sam", "2s" }, ScriptureBook.OT2_2Samuel);
        AddLookupItems(items, new[] { "1kings", "1king", "1k" }, ScriptureBook.OT2_1Kings);
        AddLookupItems(items, new[] { "2kings", "2king", "2k" }, ScriptureBook.OT2_2Kings);
        AddLookupItems(items, new[] { "1chronicles", "1chronicle", "1chron" }, ScriptureBook.OT2_1Chronicles);
        AddLookupItems(items, new[] { "2chronicles", "2chronicle", "2chron" }, ScriptureBook.OT2_2Chronicles);
        AddLookupItems(items, new[] { "ezra" }, ScriptureBook.OT2_Ezra);
        AddLookupItems(items, new[] { "nehemiah", "neh", "ne" }, ScriptureBook.OT2_Nehemiah);
        AddLookupItems(items, new[] { "esther", "esth", "es" }, ScriptureBook.OT2_Esther);
        AddLookupItems(items, new[] { "job" }, ScriptureBook.OT2_Job);
        AddLookupItems(items, new[] { "psalms", "psalm", "ps" }, ScriptureBook.OT2_Psalms);
        AddLookupItems(items, new[] { "proverbs", "proverb", "pro", "pr" }, ScriptureBook.OT2_Proverbs);
        AddLookupItems(items, new[] { "ecclesiastes", "ecclesiaste", "eccles", "eccl", "ecc", "ec" }, ScriptureBook.OT2_Ecclesiastes);
        AddLookupItems(items, new[] { "isaiah", "isa", "is" }, ScriptureBook.OT3_Isaiah);
        AddLookupItems(items, new[] { "jeremiah", "jer", "je" }, ScriptureBook.OT3_Jeremiah);
        AddLookupItems(items, new[] { "lamentations", "lamentation", "lam", "la" }, ScriptureBook.OT3_Lamentations);
        AddLookupItems(items, new[] { "ezekiel", "ezek", "ez" }, ScriptureBook.OT3_Ezekiel);
        AddLookupItems(items, new[] { "daniel", "dan", "da" }, ScriptureBook.OT3_Daniel);
        AddLookupItems(items, new[] { "hosea", "hose", "ho" }, ScriptureBook.OT3_Hosea);
        AddLookupItems(items, new[] { "joel", "joe" }, ScriptureBook.OT3_Joel);
        AddLookupItems(items, new[] { "amos", "am" }, ScriptureBook.OT3_Amos);
        AddLookupItems(items, new[] { "obadiah", "obad", "ob" }, ScriptureBook.OT3_Obadiah);
        AddLookupItems(items, new[] { "jonah", "jon" }, ScriptureBook.OT3_Jonah);
        AddLookupItems(items, new[] { "micah", "mica", "mic", "mi" }, ScriptureBook.OT3_Micah);
        AddLookupItems(items, new[] { "nahum", "nah", "na" }, ScriptureBook.OT3_Nahum);
        AddLookupItems(items, new[] { "habakkuk", "habakuk", "habak" }, ScriptureBook.OT3_Habakkuk);
        AddLookupItems(items, new[] { "zephaniah", "zeph" }, ScriptureBook.OT3_Zephaniah);
        AddLookupItems(items, new[] { "haggai", "hagg", "hag", "ha" }, ScriptureBook.OT3_Haggai);
        AddLookupItems(items, new[] { "zechariah", "zech", "ze" }, ScriptureBook.OT3_Zechariah);
        AddLookupItems(items, new[] { "malachi", "mal" }, ScriptureBook.OT3_Malachi);
        AddLookupItems(items, new[] { "matthew", "matt", "mat", "ma", "m" }, ScriptureBook.NT_Matthew);
        AddLookupItems(items, new[] { "mark", "mar" }, ScriptureBook.NT_Mark);
        AddLookupItems(items, new[] { "luke", "lu", "l" }, ScriptureBook.NT_Luke);
        AddLookupItems(items, new[] { "john", "jo", "j" }, ScriptureBook.NT_John);
        AddLookupItems(items, new[] { "acts", "act", "ac", "a" }, ScriptureBook.NT_Acts);
        AddLookupItems(items, new[] { "romans", "roman", "rom", "ro" }, ScriptureBook.NT_Romans);
        AddLookupItems(items, new[] { "1corinthians", "1corinthian", "1corinth", "1cor", "1c" }, ScriptureBook.NT_1Corinthians);
        AddLookupItems(items, new[] { "2corinthians", "2corinthian", "2corinth", "2cor", "2c" }, ScriptureBook.NT_2Corinthians);
        AddLookupItems(items, new[] { "galatians", "galatian", "gala", "gal", "ga" }, ScriptureBook.NT_Galatians);
        AddLookupItems(items, new[] { "ephesians", "ephesian", "eph", "ep" }, ScriptureBook.NT_Ephesians);
        AddLookupItems(items, new[] { "philippians", "philippian", "philip", "ph" }, ScriptureBook.NT_Philippians);
        AddLookupItems(items, new[] { "Colossians", "Colossian", "Coloss", "col", "co" }, ScriptureBook.NT_Colossians);
        AddLookupItems(items, new[] { "1thessalonians", "1thessalonian", "1thess", "1th", "1t" }, ScriptureBook.NT_1Thessalonians);
        AddLookupItems(items, new[] { "2thessalonians", "2thessalonian", "2thess", "2th", "2t" }, ScriptureBook.NT_2Thessalonians);
        AddLookupItems(items, new[] { "1timothy", "1tim" }, ScriptureBook.NT_1Timothy);
        AddLookupItems(items, new[] { "2timothy", "2tim" }, ScriptureBook.NT_2Timothy);
        AddLookupItems(items, new[] { "titus", "tit" }, ScriptureBook.NT_Titus);
        AddLookupItems(items, new[] { "philemon", "phil" }, ScriptureBook.NT_Philemon);
        AddLookupItems(items, new[] { "hebrews", "hebrew", "heb", "he" }, ScriptureBook.NT_Hebrews);
        AddLookupItems(items, new[] { "james", "jame", "ja" }, ScriptureBook.NT_James);
        AddLookupItems(items, new[] { "1peter", "1pete", "1pet", "1p" }, ScriptureBook.NT_1Peter);
        AddLookupItems(items, new[] { "2peter", "2pete", "2pet", "2p" }, ScriptureBook.NT_2Peter);
        AddLookupItems(items, new[] { "1john", "1joh", "1jo", "1j" }, ScriptureBook.NT_1John);
        AddLookupItems(items, new[] { "2john", "2joh", "2jo", "2j" }, ScriptureBook.NT_2John);
        AddLookupItems(items, new[] { "3john", "3joh", "3jo", "3j" }, ScriptureBook.NT_3John);
        AddLookupItems(items, new[] { "jude", "jud" }, ScriptureBook.NT_Jude);
        AddLookupItems(items, new[] { "revelations", "revelation", "revel", "rev", "r" }, ScriptureBook.NT_Revelation);
        AddLookupItems(items, new[] { "1nephi", "1neph", "1nep", "1ne", "1n" }, ScriptureBook.BM_1Nephi);
        AddLookupItems(items, new[] { "2nephi", "2neph", "2nep", "2ne", "2n" }, ScriptureBook.BM_2Nephi);
        AddLookupItems(items, new[] { "jacob", "jac" }, ScriptureBook.BM_Jacob);
        AddLookupItems(items, new[] { "enos", "eno", "en" }, ScriptureBook.BM_Enos);
        AddLookupItems(items, new[] { "jarom", "jarm", "jar" }, ScriptureBook.BM_Jarom);
        AddLookupItems(items, new[] { "omni", "omn", "om" }, ScriptureBook.BM_Omni);
        AddLookupItems(items, new[] { "words of mormon", "words", "wom", "wm" }, ScriptureBook.BM_Words);
        AddLookupItems(items, new[] { "mosiah", "mosi", "mos" }, ScriptureBook.BM_Mosiah);
        AddLookupItems(items, new[] { "alma", "alm", "al" }, ScriptureBook.BM_Alma);
        AddLookupItems(items, new[] { "helaman", "hela" }, ScriptureBook.BM_Helaman);
        AddLookupItems(items, new[] { "3nephi", "3neph", "3nep", "3ne", "3n" }, ScriptureBook.BM_3Nephi);
        AddLookupItems(items, new[] { "4nephi", "4neph", "4nep", "4ne", "4n" }, ScriptureBook.BM_4Nephi);
        AddLookupItems(items, new[] { "mormon", "morm", "mor" }, ScriptureBook.BM_Mormon);
        AddLookupItems(items, new[] { "ether", "eth", "et" }, ScriptureBook.BM_Ether);
        AddLookupItems(items, new[] { "moroni", "moron", "moro", "mo" }, ScriptureBook.BM_Moroni);
        AddLookupItems(items, new[] { "lecture1", "lect1", "l1" }, ScriptureBook.DC_Lecture1);
        AddLookupItems(items, new[] { "lecture2", "lect2", "l2" }, ScriptureBook.DC_Lecture2);
        AddLookupItems(items, new[] { "lecture3", "lect3", "l3" }, ScriptureBook.DC_Lecture3);
        AddLookupItems(items, new[] { "lecture4", "lect4", "l4" }, ScriptureBook.DC_Lecture4);
        AddLookupItems(items, new[] { "lecture5", "lect5", "l5" }, ScriptureBook.DC_Lecture5);
        AddLookupItems(items, new[] { "lecture6", "lect6", "l6" }, ScriptureBook.DC_Lecture6);
        AddLookupItems(items, new[] { "lecture7", "lect7", "l7" }, ScriptureBook.DC_Lecture7);
        AddLookupItems(items, new[] { "section1", "sect1", "sec1", "s1", "d&c1", "dc1", "d1" }, ScriptureBook.DC_Section1);
        AddLookupItems(items, new[] { "section2", "sect2", "sec2", "s2", "d&c2", "dc2", "d2" }, ScriptureBook.DC_Section2);
        AddLookupItems(items, new[] { "section3", "sect3", "sec3", "s3", "d&c3", "dc3", "d3" }, ScriptureBook.DC_Section3);
        AddLookupItems(items, new[] { "section4", "sect4", "sec4", "s4", "d&c4", "dc4", "d4" }, ScriptureBook.DC_Section4);
        AddLookupItems(items, new[] { "section5", "sect5", "sec5", "s5", "d&c5", "dc5", "d5" }, ScriptureBook.DC_Section5);
        AddLookupItems(items, new[] { "section6", "sect6", "sec6", "s6", "d&c6", "dc6", "d6" }, ScriptureBook.DC_Section6);
        AddLookupItems(items, new[] { "section7", "sect7", "sec7", "s7", "d&c7", "dc7", "d7" }, ScriptureBook.DC_Section7);
        AddLookupItems(items, new[] { "section8", "sect8", "sec8", "s8", "d&c8", "dc8", "d8" }, ScriptureBook.DC_Section8);
        AddLookupItems(items, new[] { "section9", "sect9", "sec9", "s9", "d&c9", "dc9", "d9" }, ScriptureBook.DC_Section9);
        AddLookupItems(items, new[] { "section10", "sect10", "sec10", "s10", "d&c10", "dc10", "d10" }, ScriptureBook.DC_Section10);
        AddLookupItems(items, new[] { "section11", "sect11", "sec11", "s11", "d&c11", "dc11", "d11" }, ScriptureBook.DC_Section11);
        AddLookupItems(items, new[] { "section12", "sect12", "sec12", "s12", "d&c12", "dc12", "d12" }, ScriptureBook.DC_Section12);
        AddLookupItems(items, new[] { "section13", "sect13", "sec13", "s13", "d&c13", "dc13", "d13" }, ScriptureBook.DC_Section13);
        AddLookupItems(items, new[] { "section14", "sect14", "sec14", "s14", "d&c14", "dc14", "d14" }, ScriptureBook.DC_Section14);
        AddLookupItems(items, new[] { "section15", "sect15", "sec15", "s15", "d&c15", "dc15", "d15" }, ScriptureBook.DC_Section15);
        AddLookupItems(items, new[] { "section16", "sect16", "sec16", "s16", "d&c16", "dc16", "d16" }, ScriptureBook.DC_Section16);
        AddLookupItems(items, new[] { "section17", "sect17", "sec17", "s17", "d&c17", "dc17", "d17" }, ScriptureBook.DC_Section17);
        AddLookupItems(items, new[] { "section18", "sect18", "sec18", "s18", "d&c18", "dc18", "d18" }, ScriptureBook.DC_Section18);
        AddLookupItems(items, new[] { "section19", "sect19", "sec19", "s19", "d&c19", "dc19", "d19" }, ScriptureBook.DC_Section19);
        AddLookupItems(items, new[] { "section20", "sect20", "sec20", "s20", "d&c20", "dc20", "d20" }, ScriptureBook.DC_Section20);
        AddLookupItems(items, new[] { "section21", "sect21", "sec21", "s21", "d&c21", "dc21", "d21" }, ScriptureBook.DC_Section21);
        AddLookupItems(items, new[] { "section22", "sect22", "sec22", "s22", "d&c22", "dc22", "d22" }, ScriptureBook.DC_Section22);
        AddLookupItems(items, new[] { "section23", "sect23", "sec23", "s23", "d&c23", "dc23", "d23" }, ScriptureBook.DC_Section23);
        AddLookupItems(items, new[] { "section24", "sect24", "sec24", "s24", "d&c24", "dc24", "d24" }, ScriptureBook.DC_Section24);
        AddLookupItems(items, new[] { "section25", "sect25", "sec25", "s25", "d&c25", "dc25", "d25" }, ScriptureBook.DC_Section25);
        AddLookupItems(items, new[] { "section26", "sect26", "sec26", "s26", "d&c26", "dc26", "d26" }, ScriptureBook.DC_Section26);
        AddLookupItems(items, new[] { "section27", "sect27", "sec27", "s27", "d&c27", "dc27", "d27" }, ScriptureBook.DC_Section27);
        AddLookupItems(items, new[] { "section28", "sect28", "sec28", "s28", "d&c28", "dc28", "d28" }, ScriptureBook.DC_Section28);
        AddLookupItems(items, new[] { "section29", "sect29", "sec29", "s29", "d&c29", "dc29", "d29" }, ScriptureBook.DC_Section29);
        AddLookupItems(items, new[] { "section30", "sect30", "sec30", "s30", "d&c30", "dc30", "d30" }, ScriptureBook.DC_Section30);
        AddLookupItems(items, new[] { "section31", "sect31", "sec31", "s31", "d&c31", "dc31", "d31" }, ScriptureBook.DC_Section31);
        AddLookupItems(items, new[] { "section32", "sect32", "sec32", "s32", "d&c32", "dc32", "d32" }, ScriptureBook.DC_Section32);
        AddLookupItems(items, new[] { "section33", "sect33", "sec33", "s33", "d&c33", "dc33", "d33" }, ScriptureBook.DC_Section33);
        AddLookupItems(items, new[] { "section34", "sect34", "sec34", "s34", "d&c34", "dc34", "d34" }, ScriptureBook.DC_Section34);
        AddLookupItems(items, new[] { "section35", "sect35", "sec35", "s35", "d&c35", "dc35", "d35" }, ScriptureBook.DC_Section35);
        AddLookupItems(items, new[] { "section36", "sect36", "sec36", "s36", "d&c36", "dc36", "d36" }, ScriptureBook.DC_Section36);
        AddLookupItems(items, new[] { "section37", "sect37", "sec37", "s37", "d&c37", "dc37", "d37" }, ScriptureBook.DC_Section37);
        AddLookupItems(items, new[] { "section38", "sect38", "sec38", "s38", "d&c38", "dc38", "d38" }, ScriptureBook.DC_Section38);
        AddLookupItems(items, new[] { "section39", "sect39", "sec39", "s39", "d&c39", "dc39", "d39" }, ScriptureBook.DC_Section39);
        AddLookupItems(items, new[] { "section40", "sect40", "sec40", "s40", "d&c40", "dc40", "d40" }, ScriptureBook.DC_Section40);
        AddLookupItems(items, new[] { "section41", "sect41", "sec41", "s41", "d&c41", "dc41", "d41" }, ScriptureBook.DC_Section41);
        AddLookupItems(items, new[] { "section42", "sect42", "sec42", "s42", "d&c42", "dc42", "d42" }, ScriptureBook.DC_Section42);
        AddLookupItems(items, new[] { "section43", "sect43", "sec43", "s43", "d&c43", "dc43", "d43" }, ScriptureBook.DC_Section43);
        AddLookupItems(items, new[] { "section44", "sect44", "sec44", "s44", "d&c44", "dc44", "d44" }, ScriptureBook.DC_Section44);
        AddLookupItems(items, new[] { "section45", "sect45", "sec45", "s45", "d&c45", "dc45", "d45" }, ScriptureBook.DC_Section45);
        AddLookupItems(items, new[] { "section46", "sect46", "sec46", "s46", "d&c46", "dc46", "d46" }, ScriptureBook.DC_Section46);
        AddLookupItems(items, new[] { "section47", "sect47", "sec47", "s47", "d&c47", "dc47", "d47" }, ScriptureBook.DC_Section47);
        AddLookupItems(items, new[] { "section48", "sect48", "sec48", "s48", "d&c48", "dc48", "d48" }, ScriptureBook.DC_Section48);
        AddLookupItems(items, new[] { "section49", "sect49", "sec49", "s49", "d&c49", "dc49", "d49" }, ScriptureBook.DC_Section49);
        AddLookupItems(items, new[] { "section50", "sect50", "sec50", "s50", "d&c50", "dc50", "d50" }, ScriptureBook.DC_Section50);
        AddLookupItems(items, new[] { "section51", "sect51", "sec51", "s51", "d&c51", "dc51", "d51" }, ScriptureBook.DC_Section51);
        AddLookupItems(items, new[] { "section52", "sect52", "sec52", "s52", "d&c52", "dc52", "d52" }, ScriptureBook.DC_Section52);
        AddLookupItems(items, new[] { "section53", "sect53", "sec53", "s53", "d&c53", "dc53", "d53" }, ScriptureBook.DC_Section53);
        AddLookupItems(items, new[] { "section54", "sect54", "sec54", "s54", "d&c54", "dc54", "d54" }, ScriptureBook.DC_Section54);
        AddLookupItems(items, new[] { "section55", "sect55", "sec55", "s55", "d&c55", "dc55", "d55" }, ScriptureBook.DC_Section55);
        AddLookupItems(items, new[] { "section56", "sect56", "sec56", "s56", "d&c56", "dc56", "d56" }, ScriptureBook.DC_Section56);
        AddLookupItems(items, new[] { "section57", "sect57", "sec57", "s57", "d&c57", "dc57", "d57" }, ScriptureBook.DC_Section57);
        AddLookupItems(items, new[] { "section58", "sect58", "sec58", "s58", "d&c58", "dc58", "d58" }, ScriptureBook.DC_Section58);
        AddLookupItems(items, new[] { "section59", "sect59", "sec59", "s59", "d&c59", "dc59", "d59" }, ScriptureBook.DC_Section59);
        AddLookupItems(items, new[] { "section60", "sect60", "sec60", "s60", "d&c60", "dc60", "d60" }, ScriptureBook.DC_Section60);
        AddLookupItems(items, new[] { "section61", "sect61", "sec61", "s61", "d&c61", "dc61", "d61" }, ScriptureBook.DC_Section61);
        AddLookupItems(items, new[] { "section62", "sect62", "sec62", "s62", "d&c62", "dc62", "d62" }, ScriptureBook.DC_Section62);
        AddLookupItems(items, new[] { "section63", "sect63", "sec63", "s63", "d&c63", "dc63", "d63" }, ScriptureBook.DC_Section63);
        AddLookupItems(items, new[] { "section64", "sect64", "sec64", "s64", "d&c64", "dc64", "d64" }, ScriptureBook.DC_Section64);
        AddLookupItems(items, new[] { "section65", "sect65", "sec65", "s65", "d&c65", "dc65", "d65" }, ScriptureBook.DC_Section65);
        AddLookupItems(items, new[] { "section66", "sect66", "sec66", "s66", "d&c66", "dc66", "d66" }, ScriptureBook.DC_Section66);
        AddLookupItems(items, new[] { "section67", "sect67", "sec67", "s67", "d&c67", "dc67", "d67" }, ScriptureBook.DC_Section67);
        AddLookupItems(items, new[] { "section68", "sect68", "sec68", "s68", "d&c68", "dc68", "d68" }, ScriptureBook.DC_Section68);
        AddLookupItems(items, new[] { "section69", "sect69", "sec69", "s69", "d&c69", "dc69", "d69" }, ScriptureBook.DC_Section69);
        AddLookupItems(items, new[] { "section70", "sect70", "sec70", "s70", "d&c70", "dc70", "d70" }, ScriptureBook.DC_Section70);
        AddLookupItems(items, new[] { "section71", "sect71", "sec71", "s71", "d&c71", "dc71", "d71" }, ScriptureBook.DC_Section71);
        AddLookupItems(items, new[] { "section72", "sect72", "sec72", "s72", "d&c72", "dc72", "d72" }, ScriptureBook.DC_Section72);
        AddLookupItems(items, new[] { "section73", "sect73", "sec73", "s73", "d&c73", "dc73", "d73" }, ScriptureBook.DC_Section73);
        AddLookupItems(items, new[] { "section74", "sect74", "sec74", "s74", "d&c74", "dc74", "d74" }, ScriptureBook.DC_Section74);
        AddLookupItems(items, new[] { "section75", "sect75", "sec75", "s75", "d&c75", "dc75", "d75" }, ScriptureBook.DC_Section75);
        AddLookupItems(items, new[] { "section76", "sect76", "sec76", "s76", "d&c76", "dc76", "d76" }, ScriptureBook.DC_Section76);
        AddLookupItems(items, new[] { "section77", "sect77", "sec77", "s77", "d&c77", "dc77", "d77" }, ScriptureBook.DC_Section77);
        AddLookupItems(items, new[] { "section78", "sect78", "sec78", "s78", "d&c78", "dc78", "d78" }, ScriptureBook.DC_Section78);
        AddLookupItems(items, new[] { "section79", "sect79", "sec79", "s79", "d&c79", "dc79", "d79" }, ScriptureBook.DC_Section79);
        AddLookupItems(items, new[] { "section80", "sect80", "sec80", "s80", "d&c80", "dc80", "d80" }, ScriptureBook.DC_Section80);
        AddLookupItems(items, new[] { "section81", "sect81", "sec81", "s81", "d&c81", "dc81", "d81" }, ScriptureBook.DC_Section81);
        AddLookupItems(items, new[] { "section82", "sect82", "sec82", "s82", "d&c82", "dc82", "d82" }, ScriptureBook.DC_Section82);
        AddLookupItems(items, new[] { "section83", "sect83", "sec83", "s83", "d&c83", "dc83", "d83" }, ScriptureBook.DC_Section83);
        AddLookupItems(items, new[] { "section84", "sect84", "sec84", "s84", "d&c84", "dc84", "d84" }, ScriptureBook.DC_Section84);
        AddLookupItems(items, new[] { "section85", "sect85", "sec85", "s85", "d&c85", "dc85", "d85" }, ScriptureBook.DC_Section85);
        AddLookupItems(items, new[] { "section86", "sect86", "sec86", "s86", "d&c86", "dc86", "d86" }, ScriptureBook.DC_Section86);
        AddLookupItems(items, new[] { "section87", "sect87", "sec87", "s87", "d&c87", "dc87", "d87" }, ScriptureBook.DC_Section87);
        AddLookupItems(items, new[] { "section88", "sect88", "sec88", "s88", "d&c88", "dc88", "d88" }, ScriptureBook.DC_Section88);
        AddLookupItems(items, new[] { "section89", "sect89", "sec89", "s89", "d&c89", "dc89", "d89" }, ScriptureBook.DC_Section89);
        AddLookupItems(items, new[] { "section90", "sect90", "sec90", "s90", "d&c90", "dc90", "d90" }, ScriptureBook.DC_Section90);
        AddLookupItems(items, new[] { "section91", "sect91", "sec91", "s91", "d&c91", "dc91", "d91" }, ScriptureBook.DC_Section91);
        AddLookupItems(items, new[] { "section92", "sect92", "sec92", "s92", "d&c92", "dc92", "d92" }, ScriptureBook.DC_Section92);
        AddLookupItems(items, new[] { "section93", "sect93", "sec93", "s93", "d&c93", "dc93", "d93" }, ScriptureBook.DC_Section93);
        AddLookupItems(items, new[] { "section94", "sect94", "sec94", "s94", "d&c94", "dc94", "d94" }, ScriptureBook.DC_Section94);
        AddLookupItems(items, new[] { "section95", "sect95", "sec95", "s95", "d&c95", "dc95", "d95" }, ScriptureBook.DC_Section95);
        AddLookupItems(items, new[] { "section96", "sect96", "sec96", "s96", "d&c96", "dc96", "d96" }, ScriptureBook.DC_Section96);
        AddLookupItems(items, new[] { "section97", "sect97", "sec97", "s97", "d&c97", "dc97", "d97" }, ScriptureBook.DC_Section97);
        AddLookupItems(items, new[] { "section98", "sect98", "sec98", "s98", "d&c98", "dc98", "d98" }, ScriptureBook.DC_Section98);
        AddLookupItems(items, new[] { "section99", "sect99", "sec99", "s99", "d&c99", "dc99", "d99" }, ScriptureBook.DC_Section99);
        AddLookupItems(items, new[] { "section100", "sect100", "sec100", "s100", "d&c100", "dc100", "d100" }, ScriptureBook.DC_Section100);
        AddLookupItems(items, new[] { "section101", "sect101", "sec101", "s101", "d&c101", "dc101", "d101" }, ScriptureBook.DC_Section101);
        AddLookupItems(items, new[] { "section102", "sect102", "sec102", "s102", "d&c102", "dc102", "d102" }, ScriptureBook.DC_Section102);
        AddLookupItems(items, new[] { "section103", "sect103", "sec103", "s103", "d&c103", "dc103", "d103" }, ScriptureBook.DC_Section103);
        AddLookupItems(items, new[] { "section104", "sect104", "sec104", "s104", "d&c104", "dc104", "d104" }, ScriptureBook.DC_Section104);
        AddLookupItems(items, new[] { "section105", "sect105", "sec105", "s105", "d&c105", "dc105", "d105" }, ScriptureBook.DC_Section105);
        AddLookupItems(items, new[] { "section106", "sect106", "sec106", "s106", "d&c106", "dc106", "d106" }, ScriptureBook.DC_Section106);
        AddLookupItems(items, new[] { "section107", "sect107", "sec107", "s107", "d&c107", "dc107", "d107" }, ScriptureBook.DC_Section107);
        AddLookupItems(items, new[] { "section108", "sect108", "sec108", "s108", "d&c108", "dc108", "d108" }, ScriptureBook.DC_Section108);
        AddLookupItems(items, new[] { "section109", "sect109", "sec109", "s109", "d&c109", "dc109", "d109" }, ScriptureBook.DC_Section109);
        AddLookupItems(items, new[] { "section110", "sect110", "sec110", "s110", "d&c110", "dc110", "d110" }, ScriptureBook.DC_Section110);
        AddLookupItems(items, new[] { "section111", "sect111", "sec111", "s111", "d&c111", "dc111", "d111" }, ScriptureBook.DC_Section111);
        AddLookupItems(items, new[] { "section112", "sect112", "sec112", "s112", "d&c112", "dc112", "d112" }, ScriptureBook.DC_Section112);
        AddLookupItems(items, new[] { "section113", "sect113", "sec113", "s113", "d&c113", "dc113", "d113" }, ScriptureBook.DC_Section113);
        AddLookupItems(items, new[] { "section114", "sect114", "sec114", "s114", "d&c114", "dc114", "d114" }, ScriptureBook.DC_Section114);
        AddLookupItems(items, new[] { "section115", "sect115", "sec115", "s115", "d&c115", "dc115", "d115" }, ScriptureBook.DC_Section115);
        AddLookupItems(items, new[] { "section116", "sect116", "sec116", "s116", "d&c116", "dc116", "d116" }, ScriptureBook.DC_Section116);
        AddLookupItems(items, new[] { "section117", "sect117", "sec117", "s117", "d&c117", "dc117", "d117" }, ScriptureBook.DC_Section117);
        AddLookupItems(items, new[] { "section118", "sect118", "sec118", "s118", "d&c118", "dc118", "d118" }, ScriptureBook.DC_Section118);
        AddLookupItems(items, new[] { "section119", "sect119", "sec119", "s119", "d&c119", "dc119", "d119" }, ScriptureBook.DC_Section119);
        AddLookupItems(items, new[] { "section120", "sect120", "sec120", "s120", "d&c120", "dc120", "d120" }, ScriptureBook.DC_Section120);
        AddLookupItems(items, new[] { "section121", "sect121", "sec121", "s121", "d&c121", "dc121", "d121" }, ScriptureBook.DC_Section121);
        AddLookupItems(items, new[] { "section122", "sect122", "sec122", "s122", "d&c122", "dc122", "d122" }, ScriptureBook.DC_Section122);
        AddLookupItems(items, new[] { "section123", "sect123", "sec123", "s123", "d&c123", "dc123", "d123" }, ScriptureBook.DC_Section123);
        AddLookupItems(items, new[] { "section124", "sect124", "sec124", "s124", "d&c124", "dc124", "d124" }, ScriptureBook.DC_Section124);
        AddLookupItems(items, new[] { "section125", "sect125", "sec125", "s125", "d&c125", "dc125", "d125" }, ScriptureBook.DC_Section125);
        AddLookupItems(items, new[] { "section126", "sect126", "sec126", "s126", "d&c126", "dc126", "d126" }, ScriptureBook.DC_Section126);
        AddLookupItems(items, new[] { "section127", "sect127", "sec127", "s127", "d&c127", "dc127", "d127" }, ScriptureBook.DC_Section127);
        AddLookupItems(items, new[] { "section128", "sect128", "sec128", "s128", "d&c128", "dc128", "d128" }, ScriptureBook.DC_Section128);
        AddLookupItems(items, new[] { "section129", "sect129", "sec129", "s129", "d&c129", "dc129", "d129" }, ScriptureBook.DC_Section129);
        AddLookupItems(items, new[] { "section130", "sect130", "sec130", "s130", "d&c130", "dc130", "d130" }, ScriptureBook.DC_Section130);
        AddLookupItems(items, new[] { "section131", "sect131", "sec131", "s131", "d&c131", "dc131", "d131" }, ScriptureBook.DC_Section131);
        AddLookupItems(items, new[] { "section132", "sect132", "sec132", "s132", "d&c132", "dc132", "d132" }, ScriptureBook.DC_Section132);
        AddLookupItems(items, new[] { "section133", "sect133", "sec133", "s133", "d&c133", "dc133", "d133" }, ScriptureBook.DC_Section133);
        AddLookupItems(items, new[] { "section134", "sect134", "sec134", "s134", "d&c134", "dc134", "d134" }, ScriptureBook.DC_Section134);
        AddLookupItems(items, new[] { "section135", "sect135", "sec135", "s135", "d&c135", "dc135", "d135" }, ScriptureBook.DC_Section135);
        AddLookupItems(items, new[] { "section136", "sect136", "sec136", "s136", "d&c136", "dc136", "d136" }, ScriptureBook.DC_Section136);
        AddLookupItems(items, new[] { "section137", "sect137", "sec137", "s137", "d&c137", "dc137", "d137" }, ScriptureBook.DC_Section137);
        AddLookupItems(items, new[] { "section138", "sect138", "sec138", "s138", "d&c138", "dc138", "d138" }, ScriptureBook.DC_Section138);
        AddLookupItems(items, new[] { "moses", "mose" }, ScriptureBook.DC_Moses);
        AddLookupItems(items, new[] { "abraham", "abram", "ab" }, ScriptureBook.DC_Abraham);
        AddLookupItems(items, new[] { "matthew25", "matt25", "mat25", "ma25" }, ScriptureBook.DC_Matthew25);
        AddLookupItems(items, new[] { "articles", "article", "art", "ar" }, ScriptureBook.DC_Articles);
        AddLookupItems(items, new[] { "Joseph Smith History", "jsh" }, ScriptureBook.DC_1838Account);
        AddLookupItems(items, new[] { "Apostolic Charge" }, ScriptureBook.DC);
        //AddLookupItems(strs, new[] { "Article On Marriage" }, ScriptureBook.DC_ArticleOnMarriage);
        //AddLookupItems(strs, new[] { "Articles Of The Church Of Christ" }, ScriptureBook.DC_ArticlesOfTheChurchOfChrist);
        //AddLookupItems(strs, new[] { "Endowment Needed To Preach" }, ScriptureBook.DC_en);
        //AddLookupItems(strs, new[] { "The Twelve Under Condemnation" }, ScriptureBook.DC_TheTwelveUnderCondemnation);
        //AddLookupItems(strs, new[] { "Wentworth Letter" }, ScriptureBook.DC_WentworthLetter);
        AddLookupItems(items, new[] { "Articles of Faith" }, ScriptureBook.DC_Articles);
        //AddLookupItems(strs, new[] { "Meeting minutes" }, ScriptureBook.DC_MeetingMinutes);
        //AddLookupItems(strs, new[] { "The Church Of The Latter Day Saints" }, ScriptureBook.DC_TheChurchOfTheLatterDaySaints);
        //AddLookupItems(strs, new[] { "Revelation on the Twelve" }, ScriptureBook.DC_RevelationOnTheTwelve);
        //AddLookupItems(strs, new[] { "Revelation to Twelve" }, ScriptureBook.DC_RevelationToTwelve);
        //AddLookupItems(strs, new[] { "Limitations On The Twelve" }, ScriptureBook.DC_LimitationsOnTheTwelve);
        return items;
    }

    private async Task InitializeSearchIndexAsync()
    {
        var timer = Stopwatch.StartNew();
        var indexDirectory = Path.Combine(_fileService.DataRootDirectory, "UM-Index");
        var data = await _fileService.LoadDataAsync("./Scriptures/_Index/FullIndex.bin")
            ;

        LogMessage($"Data load: {timer.ElapsedMilliseconds}");
        timer.Restart();

        using (MemoryStream memoryStream = new MemoryStream(data))
        {
            memoryStream.Position = 0L;
            await ProcessIndexStreamAsync(memoryStream)
                ;
        }

        LogMessage($"Process index: {timer.ElapsedMilliseconds}");
        timer.Restart();

        try
        {
            await Task.Yield();
            _searchDirectory = FSDirectory.Open(indexDirectory, new SingleInstanceLockFactory());
            _reader = DirectoryReader.Open(_searchDirectory);
            _searcher = new IndexSearcher(_reader);
        }
        catch (Exception ex)
        {
            throw new Exception("Unable to build search index", ex);
        }

        LogMessage($"Load index: {timer.ElapsedMilliseconds}");
        timer.Restart();
    }

    private void PreprocessScriptureBookName(string name, IList<string> allParts)
    {
        var items = name.Split(Array.Empty<char>());
        if (allParts.Count < items.Length)
        {
            return;
        }

        var anyItems = items
            .Where((t, i) => !string.Equals(allParts[i], t, StringComparison.OrdinalIgnoreCase))
            .Any();

        if (anyItems)
        {
            return;
        }

        for (var j = 0; j < items.Length; j++)
        {
            allParts.RemoveAt(0);
        }

        allParts.Insert(0, name);
    }

    private void PreprocessScriptureBookNames(IList<string> allParts)
    {
        PreprocessScriptureBookName("Words of Mormon", allParts);
        PreprocessScriptureBookName("Joseph Smith History", allParts);
        PreprocessScriptureBookName("Doctrine and Covenants", allParts);
        PreprocessScriptureBookName("Doctrine & Covenants", allParts);
        PreprocessScriptureBookName("15 Elders To Receive Endowment", allParts);
        PreprocessScriptureBookName("1835 History Written", allParts);
        PreprocessScriptureBookName("1838 History Written", allParts);
        PreprocessScriptureBookName("Abraham 1:1–2:18 Written", allParts);
        PreprocessScriptureBookName("Apostolic Charge", allParts);
        PreprocessScriptureBookName("Apostolic Instructions", allParts);
        PreprocessScriptureBookName("Appointed Time For Redemption Of Zion", allParts);
        PreprocessScriptureBookName("Article On Marriage", allParts);
        PreprocessScriptureBookName("Articles Of Faith", allParts);
        PreprocessScriptureBookName("Articles Of The Church Of Christ", allParts);
        PreprocessScriptureBookName("Blessing From Oliver Cowdery", allParts);
        PreprocessScriptureBookName("Book Of Abraham Published", allParts);
        PreprocessScriptureBookName("Brigham Tries To Sell The Temple", allParts);
        PreprocessScriptureBookName("Chastened For Not Obeying", allParts);
        PreprocessScriptureBookName("Church Moves Out Of Nauvoo", allParts);
        PreprocessScriptureBookName("Committee To Arrange The Doctrine", allParts);
        PreprocessScriptureBookName("Condemnation", allParts);
        PreprocessScriptureBookName("Consecration", allParts);
        PreprocessScriptureBookName("Continuation Of Visions", allParts);
        PreprocessScriptureBookName("Covenant Of Tithing", allParts);
        PreprocessScriptureBookName("Depend On No Man", allParts);
        PreprocessScriptureBookName("Doctrine & Covenants Canonized", allParts);
        PreprocessScriptureBookName("Endowment Needed To Preach", allParts);
        PreprocessScriptureBookName("High Priesthood Conferred On Elders", allParts);
        PreprocessScriptureBookName("Highest Office In The Church", allParts);
        PreprocessScriptureBookName("If Both Not Built, Must Run Away", allParts);
        PreprocessScriptureBookName("Instructions For Kirtland", allParts);
        PreprocessScriptureBookName("Jews To Gather In America", allParts);
        PreprocessScriptureBookName("Joseph And Sidney Flee Kirtland", allParts);
        PreprocessScriptureBookName("Joseph Escapes Custody", allParts);
        PreprocessScriptureBookName("Joseph Settles In Nauvoo", allParts);
        PreprocessScriptureBookName("Joseph To Not Prophesy Anymore", allParts);
        PreprocessScriptureBookName("King Follett Discourse Delivered", allParts);
        PreprocessScriptureBookName("Lectures On Faith", allParts);
        PreprocessScriptureBookName("Limitations On The Twelve", allParts);
        PreprocessScriptureBookName("Many Will Seek For High Positions", allParts);
        PreprocessScriptureBookName("Meeting Minutes", allParts);
        PreprocessScriptureBookName("Nauvoo Expositor Published", allParts);
        PreprocessScriptureBookName("Nauvoo House Cornerstones Laid", allParts);
        PreprocessScriptureBookName("Nauvoo House", allParts);
        PreprocessScriptureBookName("Nauvoo Temple Cornerstones Laid", allParts);
        PreprocessScriptureBookName("Nauvoo Temple Dedicated", allParts);
        PreprocessScriptureBookName("Nauvoo Temple Destroyed By Fire", allParts);
        PreprocessScriptureBookName("Nauvoo Temple Preliminary Dedication", allParts);
        PreprocessScriptureBookName("Nauvoo Temple Walls Collapsed By Wind", allParts);
        PreprocessScriptureBookName("Necessary To Build Two Buildings", allParts);
        PreprocessScriptureBookName("No More Baptisms For The Dead", allParts);
        PreprocessScriptureBookName("Oliver Ordained A President", allParts);
        PreprocessScriptureBookName("Orders Of Priesthood", allParts);
        PreprocessScriptureBookName("Presiding Patriarch", allParts);
        PreprocessScriptureBookName("Prophecy Of A Tower", allParts);
        PreprocessScriptureBookName("Quorum Of The Seventy Called", allParts);
        PreprocessScriptureBookName("Revelation on the Twelve", allParts);
        PreprocessScriptureBookName("Revelation to Twelve", allParts);
        PreprocessScriptureBookName("School Of The Elders Held", allParts);
        PreprocessScriptureBookName("Second Commission", allParts);
        PreprocessScriptureBookName("Sidney: Prophet, Seer, And Revelator", allParts);
        PreprocessScriptureBookName("Something New Must Be Done", allParts);
        PreprocessScriptureBookName("Spirit Of Speculation", allParts);
        PreprocessScriptureBookName("Surrender At Far West", allParts);
        PreprocessScriptureBookName("The Church Of The Latter Day Saints", allParts);
        PreprocessScriptureBookName("The Twelve Under Condemnation", allParts);
        PreprocessScriptureBookName("Twelve Apostles Called", allParts);
        PreprocessScriptureBookName("Twelve Vote To Sell Both Temples", allParts);
        PreprocessScriptureBookName("Violence In Missouri", allParts);
        PreprocessScriptureBookName("Wentworth Letter", allParts);
    }

    private async Task ProcessIndexStreamAsync(Stream indexStream)
    {
        await Task.Yield();
        var location = Path.Combine(_fileService.DataRootDirectory, "UM-Index");
        var zipArchive = new ZipArchive(indexStream);

        if (Directory.Exists(location))
        {
            Directory.Delete(location, true);
        }

        zipArchive.ExtractToDirectory(location, true);
    }

    private async Task<SearchMatch[]> ProcessSearchMatchesAsync(TopDocs matches, string regex, SearchInfo search)
    {
        var searchMatches = matches.ScoreDocs
            .Select<ScoreDoc, SearchMatch?>(match =>
            {
                var documents = _reader!.Document(match.Doc);
                var field = documents.GetField("Text");
                var stringValue = field?.GetStringValue() ?? "";

                var text = ((string)stringValue)
                    .Trim()
                    .Replace("\n", " ")
                    .Replace("  ", " ");
                var smallest = FindSmallestMatch(text, regex);

                if (string.IsNullOrWhiteSpace(smallest))
                {
                    return null!;
                }

                var formatted = BuildFormattedMatchText(smallest, search);
                var scriptureBook = Enum.Parse<ScriptureBook>(documents.GetField("Book").GetStringValue());
                var xpath = documents.GetField("XPath").GetStringValue();
                var chapter = documents.GetField("Chapter").GetInt32Value();
                var num = chapter ?? 1;
                chapter = documents.GetField("Verse").GetInt32Value();

                return new SearchMatch
                {
                    Mode = search.Mode,
                    Book = scriptureBook,
                    Chapter = num,
                    Verse = chapter ?? 1,
                    Text = smallest.LimitTo(100),
                    FormattedText = formatted.LimitTo(100),
                    Keywords = search.SearchKeywords,
                    Score = match.Score,
                    XPath = xpath
                };
            });

        var matchResults = searchMatches
            .Where(x => x != null)
            .Select(x => x)
            .OrderByDescending(x => x!.Score);

        var list = new List<SearchMatch>();

        foreach (SearchMatch? match in matchResults)
        {
            list.Add(match!);
        }

        var array = list.ToArray();
        var uniqueMatches = DetermineUniqueMatches(search.Mode, array);

        await RefreshAsync()
            ;

        return uniqueMatches;
    }

    private async Task RefreshAsync()
    {
        await Task.Delay(1)
            ;

        await _refresh()
            ;

        await Task.Delay(1)
            ;
    }

    private static void AddLookupItems(Dictionary<string, ScriptureBook> lookup, string[] keys, ScriptureBook value)
    {
        foreach (var key in keys)
        {
            lookup.Add(key, value);
        }
    }

    private static string BuildFormattedMatchText(string text, SearchInfo search)
    {
        return search.KeywordReplacerRegex == null
            ? text
            : search.KeywordReplacerRegex.Replace(text, "<b>$&</b>");
    }

    private BooleanQuery? BuildScriptureBookQuery(IList<string> allParts, BooleanQuery query)
    {
        PreprocessScriptureBookNames(allParts);

        if (allParts.Count >= 2)
        {
            if (allParts[0].IsNumber() && allParts[1].IsAlpha())
            {
                var str = string.Concat(allParts[0], allParts[1]);
                allParts.RemoveAt(0);
                allParts.RemoveAt(0);
                allParts.Insert(0, str);
            }
            else if (allParts[0].IsAlpha() && allParts[1].IsNumber())
            {
                if (string.Equals(allParts[0], "Doctrine & Covenants") || string.Equals(allParts[0], "Doctrine and Covenants"))
                {
                    allParts[0] = "dc";
                }

                if (string.Equals(allParts[0], "Lectures On Faith"))
                {
                    allParts[0] = "lecture";
                }

                if (allParts[0].StartsWith("lec") || allParts[0] == "l" || allParts[0].StartsWith("sec") || allParts[0] == "s" || allParts[0] == "d&c" || allParts[0] == "dc")
                {
                    var str1 = string.Concat(allParts[0], allParts[1]);
                    allParts.RemoveAt(0);
                    allParts.RemoveAt(0);
                    allParts.Insert(0, str1);
                }
            }
        }

        var scriptureBook = ConvertToScriptureBook(allParts[0]);
        if (scriptureBook == ScriptureBook.None)
        {
            return null;
        }

        query.Add(new PhraseQuery
        {
            new Term(nameof(SearchItem.Book), scriptureBook.ToString())
        }, Occur.MUST);

        if (allParts.Count > 0)
        {
            allParts.RemoveAt(0);
        }

        return query;
    }

    private BooleanQuery? BuildScriptureChapterQuery(IList<string> allParts, BooleanQuery? query)
    {
        if (query == null || allParts.Count > 0 && !allParts[0].IsNumber())
        {
            return null;
        }

        var num = (allParts.Count == 0 ? 1 : int.Parse(allParts[0]));

        query.Add(NumericRangeQuery.NewInt32Range(nameof(SearchItem.Chapter), 1, num, num, true, true), Occur.MUST);
        if (allParts.Count > 0)
        {
            allParts.RemoveAt(0);
        }

        return query;
    }

    private Query? BuildScriptureQuery(string[] parts)
    {
        if (parts.Length == 0)
        {
            return null;
        }

        var list = parts.ToList();
        var booleanQueries = new BooleanQuery();
        booleanQueries = BuildScriptureBookQuery(list, booleanQueries);

        if (booleanQueries != null)
        {
            var str = booleanQueries.ToString();
            if (!str.Contains("DC_Section") && !str.Contains("DC_Lecture"))
            {
                booleanQueries = BuildScriptureChapterQuery(list, booleanQueries);
            }
        }

        booleanQueries = BuildScriptureVerseQuery(list, booleanQueries);
        if (booleanQueries != null && list.Count <= 0)
        {
            return booleanQueries;
        }
        return null;
    }

    private BooleanQuery? BuildScriptureVerseQuery(IList<string> allParts, BooleanQuery? query)
    {
        int num;
        if (query == null || allParts.Count > 0 && !allParts[0].IsNumber())
        {
            return null;
        }

        var num1 = (allParts.Count == 0 ? 1 : int.Parse(allParts[0]));
        if (allParts.Count > 0)
        {
            allParts.RemoveAt(0);
        }

        if (allParts.Count <= 0 || !allParts[0].IsNumber() || !int.TryParse(allParts[0], out num) || num <= num1)
        {
            query.Add(NumericRangeQuery.NewInt32Range(nameof(SearchItem.Verse), num1, num1, true, true), Occur.MUST);
        }
        else
        {
            query.Add(NumericRangeQuery.NewInt32Range(nameof(SearchItem.Verse), num1, num, true, true), Occur.MUST);
            allParts.RemoveAt(0);
        }

        return query;
    }

    private static string ConvertToRegexWildcardSearch(string[] words)
    {
        switch (words.Length)
        {
            case 0:
                return "";
            default:
                return words.StringJoin(".*?");
        }
    }

    private ScriptureBook ConvertToScriptureBook(string name)
    {
        return _scriptureBookLookup.TryGetValue(name, out var scriptureBook) == false
            ? ScriptureBook.None
            : scriptureBook;
    }

    private static SearchMatch[] DetermineUniqueMatches(SearchMode mode, SearchMatch[] matches)
    {
        var trie = new Trie();
        var searchMatches = new List<SearchMatch>();

        foreach (var searchMatch in matches.OrderByDescending(x => x.XPath.Length))
        {
            if (trie.HasPrefix(searchMatch.XPath))
            {
                continue;
            }
            var flag = searchMatch.XPath.Contains("/verse");
            var flag1 = searchMatch.XPath.Contains("/chapter");

            if (mode == SearchMode.Scripture && !flag && !flag1 || mode != SearchMode.Scripture && flag | flag1)
            {
                continue;
            }

            trie.AddWord(searchMatch.XPath);
            searchMatches.Add(searchMatch);
        }

        return searchMatches.ToArray();
    }

    private async Task<SearchMatch[]> ExecuteSearchAsync(SearchMode mode, Func<ValueTuple<TopDocs, string, string[]>> action)
    {
        await _searchLock.WaitAsync();

        try
        {
            Stopwatch.StartNew();
            await RefreshAsync()
                ;

            var valueTuple = action();
            var searchInfo = new SearchInfo(mode, valueTuple.Item3);
            return await ProcessSearchMatchesAsync(valueTuple.Item1, valueTuple.Item2, searchInfo)
                ;
        }
        finally
        {
            _searchLock.Release();
        }
    }

    private static void LogMessage(string message)
    {
        Console.WriteLine(message);
        Debug.WriteLine(message);
    }

    #endregion
}