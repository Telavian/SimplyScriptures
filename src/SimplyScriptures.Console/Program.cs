using SimplyScriptures.Common.Extensions;
using SimplyScriptures.Console.Loader;

//**********************************

//var files = Directory.GetFiles(@"G:\SimplyScriptures\SimplyScriptures\SimplyScriptures.Maui\Resources\Raw\", "*.html", SearchOption.AllDirectories);

//foreach (var file in files)
//{
//    var html = File.ReadAllText(file);

//    var index = html.IndexOf("<body");
//    html = html.Substring(index);
//    html = html.Insert(0, "<!DOCTYPE html>\r\n<html lang=\"en\" style=\"height: 100%; width: 100%;\">\r\n<head>\r\n    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />\r\n    <meta charset=\"utf-8\" />\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />\r\n\r\n    <link rel=\"stylesheet\" type=\"text/css\" href=\"../../css/Scripture.css\">\r\n    <script type=\"text/javascript\" src=\"../../Javascript/DisplayPage.js\"></script>\r\n</head>\r\n\r\n");

//    File.WriteAllText(file, html);
//}

//**********************************

//var files = Directory.GetFiles(@"G:\SimplyScriptures\SimplyScriptures\SimplyScriptures.Blazor\wwwroot\Scriptures", "*.html",  SearchOption.AllDirectories);

//foreach (var file in files)
//{
//    var html = File.ReadAllText(file);
//    var doc = new HtmlDocument();
//    doc.LoadHtml(html);

//    ProcessNode(doc.DocumentNode);

//    void ProcessNode(HtmlNode node)
//    {
//        if (string.Equals(node.Name, "chapter", StringComparison.OrdinalIgnoreCase) ||
//            string.Equals(node.Name, "verse", StringComparison.OrdinalIgnoreCase))
//        {
//            return;
//        }

//        if (node.NodeType == HtmlNodeType.Text && node.InnerHtml.Trim().Length > 0)
//        {
//            node.InnerHtml = $"<span>{node.InnerHtml}</span>";
//        }
//        else
//        {
//            foreach (var child in node.ChildNodes)
//            {
//                ProcessNode(child);
//            }
//        }
//    }

//    doc.Save(new StreamWriter(file));
//}

//var json = File.ReadAllText(@"G:\SimplyScriptures\SimplyScriptures\SimplyScriptures.Blazor\wwwroot\Scriptures\DC\DC.json");
//var items = json.DeserializeFromJson<ContentItem[]>() ?? Array.Empty<ContentItem>();

//var docLocation = "";
//var doc = new HtmlDocument();
//var isError = false;

//foreach (var item in items)
//{
//    ProcessItem(item, 0, false);

//    void ProcessItem(ContentItem contentItem, int index, bool isChild)
//    {
//        var htmlPath = contentItem.Book.ToHtmlPath();
//        if (docLocation != htmlPath)
//        {
//            docLocation = htmlPath;
//            var html = File.ReadAllText(docLocation.Replace("./", "G:\\SimplyScriptures\\SimplyScriptures\\SimplyScriptures.Blazor\\wwwroot\\"));
//            doc.LoadHtml(html);
//        }

//        var match = doc.DocumentNode.SelectNodes(contentItem.XPath);

//        if (match == null || match.Count == 0)
//        {
//            Console.WriteLine($"*** '{contentItem.Name}': No matches");
//            isError = true;
//        }
//        else if (match.Count > 1)
//        {
//            Console.WriteLine($"*** '{contentItem.Name}': Multiple matches");
//            isError = true;
//        }
//        else
//        {
//            Console.WriteLine($"'{contentItem.Name}': '{match[0].InnerText.Trim()}'");
//        }

//        var children = contentItem.Children ?? Array.Empty<ContentItem>();
//        contentItem.Children = children;

//        contentItem.XPath = isChild == false 
//            ? "/html[1]/body[1]/book" 
//            : $"(/html[1]/body[1]//chapter)[{index}]";

//            var childIndex = 0;
//        foreach (var child in children)
//        {
//            //child.Parent = contentItem;
//            if (child.Book == ScriptureBook.None)
//            {
//                child.Book = contentItem.Book;
//            }

//            ProcessItem(child, childIndex + 1, true);
//            childIndex++;
//        }
//    }
//}

//json = JsonSerializer.Serialize(items, options);
//if (isError) Console.WriteLine("********** ERROR **********");
//Console.WriteLine();

//Console.WriteLine("Starting index");
//await new BookProcessor()
//    .ProcessAllBookIndexDataAsync()
//    ;

//**********************************

var matches = await new BookProcessor()
    .ProcessAllBookExactSearchAsync("\"upon my house shall it begin\"")
    ;
Console.WriteLine($"Matches found: {matches.Length}");

matches = await new BookProcessor()
    .ProcessAllBookPhraseSearchAsync("upon my house begin")
    ;
Console.WriteLine($"Matches found: {matches.Length}");

//matches = await new BookProcessor()
//    .ProcessAllBookPhraseSearchAsync("hear him")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

////matches = await new BookProcessor()
////    //.ProcessAllBookRegexSearchAsync("(?=upon)(?=.*my)(?=.*house)(?=.*shall)(?=.*it)(?=.*begin).*")
////    .ProcessAllBookScriptureSearchAsync("2nephi 25:26")
////    ;
////Console.WriteLine($"Matches found: {matches.Length}");

//matches = await new BookProcessor()
//    .ProcessAllBookScriptureSearchAsync("2 Nephi 25:26")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

//matches = await new BookProcessor()
//    .ProcessAllBookScriptureSearchAsync("Doctrine and Covenants 76:50–70")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

//var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
//File.WriteAllText(@$"c:\users\patrick\desktop\{book}.json", json);

//var lines = File.ReadAllLines(@"C:\Users\Patrick\Desktop\PDF_Text\LoF-DC-PoGP-export.htm")
//    .Select((x, i) => new { Line = x, Index = i })
//    .ToArray();

//for (var x = 1; x <= 78; x++)
//{
//    var regex = new Regex($@"\.s{x}\s|""s{x}""", RegexOptions.Compiled);

//    var matches = lines
//        .Where(x => regex.IsMatch(x.Line))
//        .ToArray();

//    if (matches.Length == 0)
//    {
//        continue;
//    }

//    var lastMatch = matches
//        .Select(y => y.Index)
//        .Max();

//    if (lastMatch > 1215)
//    {
//        Console.WriteLine($"{x}: {matches.Length} total");
//    }
//}

//var options = new JsonSerializerOptions()
//{
//    WriteIndented = true,
//    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
//    Converters = { new JsonStringEnumConverter() }
//};

//var text = File.ReadAllText(@"C:\Source\SimplyScriptures\SimplyScriptures\SimplyScriptures.Topics\wwwroot\topics.json");
//var items = JsonSerializer.Deserialize<ContentTopic[]>(text, options);

//text = JsonSerializer.Serialize(items, options);
//File.WriteAllText(@"C:\Source\SimplyScriptures\SimplyScriptures\SimplyScriptures.Topics\wwwroot\topics.json", text);

//var matches = await new BookProcessor()
//    .ProcessAllBookExactSearchAsync("\"upon my house shall it begin\"")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

//var matches = await new BookProcessor()
//    .ProcessAllBookPhraseSearchAsync("hear him")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

//matches = await new BookProcessor()
//    //.ProcessAllBookRegexSearchAsync("(?=upon)(?=.*my)(?=.*house)(?=.*shall)(?=.*it)(?=.*begin).*")
//    .ProcessAllBookRegexSearchAsync("upon.*?my.*?house.*?shall.*?it.*?begin")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

//var matches = await new BookProcessor()
//    .ProcessAllBookPhraseSearchAsync("1 kings 13")
//    ;
//Console.WriteLine($"Matches found: {matches.Length}");

Console.WriteLine("Finished...");
Console.ReadLine();
//await new BookProcessor()
//.ProcessAllBooksAsync()
//    .ConfigureAwait(false);
