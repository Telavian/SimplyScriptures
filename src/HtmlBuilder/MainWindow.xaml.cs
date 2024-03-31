using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;
using HtmlBuilder.HtmlConverter;
using Microsoft.Win32;
using SimplyScriptures.Common.Extensions;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.FileDialogs;
using Telerik.Windows.Controls.SyntaxEditor.Palettes;
using Telerik.Windows.Controls.SyntaxEditor.Taggers;
using Telerik.Windows.SyntaxEditor.Core.Text;

namespace HtmlBuilder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    #region FilePath

    private string _filePath = "";

    public string FilePath
    {
        get => _filePath;
        set => SetField(ref _filePath, value);
    }

    #endregion

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        var tagger = new XmlTagger(SyntaxEditor);
        SyntaxEditor.TaggersRegistry.RegisterTagger(tagger);
        SyntaxEditor.Palette = new DarkPalette();
        SyntaxEditor.EditorFontFamily = new FontFamily("Consolas");
        SyntaxEditor.Document = new TextDocument("");
    }

    #region Protected Methods

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Private Methods

    private void LoadButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Html files (*.html)|*.html";
        dialog.Title = "Select file";
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        var result = dialog.ShowDialog();

        if (result != true)
        {
            return;
        }

        FilePath = dialog.FileName;
        using (var reader = new StreamReader(dialog.OpenFile()))
        {
            var html = reader.ReadToEnd();
            html = FormatHtml(html);

            SyntaxEditor.Document = new TextDocument(html);
        }
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        var text = SyntaxEditor.Document.CurrentSnapshot.GetText();
        File.WriteAllText(FilePath, text);
    }

    private void ProcessButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Htm files (*.htm)|*.htm";
        dialog.Title = "Select file";
        dialog.CheckFileExists = true;
        dialog.CheckPathExists = true;
        var result = dialog.ShowDialog();

        if (result != true)
        {
            return;
        }

        var converter = DetermineConverter(dialog.FileName);
        var html = converter.Convert(dialog.OpenFile());
        html = FormatHtml(html);

        SyntaxEditor.Document = new TextDocument(html);
    }

    private HtmlConverter.HtmlConverter DetermineConverter(string filename)
    {
        if (filename.IsRegexMatch("lof.*dc"))
        {
            return new DCHtmlConverter();
        }

        if (filename.IsRegexMatch("book.*mormon"))
        {
            return new BMHtmlConverter();
        }

        if (filename.IsRegexMatch("new.*testament"))
        {
            return new NTHtmlConverter();
        }

        if (filename.IsRegexMatch("old.*testament.*1"))
        {
            return new OT1HtmlConverter();
        }

        if (filename.IsRegexMatch("old.*testament.*2"))
        {
            return new OT2HtmlConverter();
        }

        if (filename.IsRegexMatch("old.*testament.*3"))
        {
            return new OT3HtmlConverter();
        }

        throw new Exception($"Invalid filename: {filename}");
    }

    private void PasteTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        var text = Clipboard.GetText();

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var startTag = "<verse>";
        var endTag = "</verse>";
        var index = 0;

        while (index != -1 && index < text.Length)
        {
            var start = text.IndexOfAny(new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }, index);

            if (start == -1)
            {
                break;
            }

            var end = start;
            while (end <= (text.Length - 1) && char.IsDigit(text[end]))
            {
                end++;
            }

            text = text.Insert(end, endTag);
            text = text.Insert(start, startTag);

            index = end + 1 + startTag.Length + endTag.Length;
        }

        text += "\r\n\r\n";
        SyntaxEditor.Insert(text);
    }

    private void BookTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("book");
    }

    private void ChapterTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("chapter");
    }

    private void VerseTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("verse");
    }

    private void HeadingTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("heading");
    }

    private void UnderTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("under");
    }

    private void DashedUnderTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("dunder");
    }

    private void DottedDashedUnderTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("ddunder");
    }

    private void ItalicTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("italic");
    }

    private void BoldTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("bold");
    }

    private void DotTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("dot");
    }

    private void BlockTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("block", true);
    }

    private void BreakTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        AddTag("break");
    }

    private void BreakIndentTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("bindent");
    }

    private void IndentTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("indent");
    }

    private void QuoteTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("quote");
    }

    private void BorderTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("border", true);
    }

    private void BoxTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("box", true);
    }

    private void ReferenceTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("reference", true);
    }

    private void NoteTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("note");
    }

    private void OriginalTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("original");
    }

    private void PrinterTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("printer");
    }

    private void ManuscriptTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("manuscript");
    }

    private void JSTTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("jst");
    }

    private void SectionHeaderTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectionheader");
    }

    private void SectionNumberTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectionnumber");
    }

    private void SectionTopicTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectiontopic");
    }

    private void SectionTimeTagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectiontime");
    }

    private void SectionReference1TagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectionreference1");
    }

    private void SectionReference2TagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectionreference2");
    }

    private void SectionReference3TagButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        EncloseSelection("sectionreference3");
    }

    private void FormatPoeticBlockButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        var selection = SyntaxEditor.Selection.GetSelectedText();
        var selectionStart = SyntaxEditor.Selection.StartPosition;

        var newText = selection
            .Replace("<block>", "")
            .Replace("</block>", "")
            .Replace("<quote>", "")
            .Replace("</quote>", "");

        var lines = newText
            .Split('\n')
            .Select(x => x.TrimEnd())
            .Where(x => string.IsNullOrWhiteSpace(x) == false)
            .Select(x => x.Replace("  ", " ").Replace("  ", " ").Replace("  ", " "))
            .Select(x =>
            {
                if (x.Contains("```"))
                {
                    return $"{x.Replace("```", "<indent>")}</indent>\n";
                }

                return $"{x}<break />\n";
            })
            .StringJoin("");

        var indentationAmount = lines.Length - lines.TrimStart().Length;
        var indentation = new string(' ', indentationAmount);

        newText = lines;
        newText = $"<block>\r\n{newText}\n{indentation}</block>";
        SyntaxEditor.ReplaceNextMatch(selection, selectionStart.Index - 5, newText, true, false);
    }

    private void PastePoeticBlockButton_OnClick(object sender, RadRoutedEventArgs e)
    {
        var text = Clipboard.GetText();
        var lines = text.Split('\n');
        var chunks = new List<string[]>();
        var finalText = "";

        var chunk = new List<string>();
        foreach (var line in lines)
        {
            switch (string.IsNullOrWhiteSpace(line))
            {
                case false:
                    chunk.Add(line);
                    break;
                default:
                    chunks.Add(chunk.ToArray());
                    chunk.Clear();
                    break;
            }
        }

        chunks.Add(chunk.ToArray());

        foreach (var c in chunks)
        {
            finalText += ProcessPoeticBlock(c) + "\n";
        }

        SyntaxEditor.Insert(finalText);
    }

    private string ProcessPoeticBlock(string[] lines)
    {
        var finalLines = new List<string>();

        var previousIndentation = int.MaxValue;
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            var currentIndentation = line.Length - line.TrimStart().Length;

            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            if (char.IsDigit(trimmedLine[0]))
            {
                var start = 0;
                while (char.IsDigit(trimmedLine[start]))
                {
                    start++;
                }

                var number = trimmedLine.Substring(0, start);
                trimmedLine = trimmedLine.Replace(number, $"<verse>{number}</verse>");
            }

            if (currentIndentation > previousIndentation)
            {
                trimmedLine = $"<indent>{trimmedLine}</indent>\n";
            }
            else
            {
                trimmedLine = $"{trimmedLine}<break />\n";
                previousIndentation = currentIndentation;
            }

            finalLines.Add(trimmedLine);
        }

        finalLines.Insert(0, "<block>\n");
        finalLines.Add("</block>\n");

        return finalLines
            .StringJoin("");
    }

    private void EncloseSelection(string tag, bool addNewline = false)
    {
        var end = SyntaxEditor.Selection.EndPosition;
        var start = SyntaxEditor.Selection.StartPosition;

        var newLine = addNewline
            ? "\r\n\r\n"
            : "";

        SyntaxEditor.Document.Insert(end.Index, $"</{tag}>{newLine}");
        SyntaxEditor.Document.Insert(start.Index, $"<{tag}>");
    }

    private void AddTag(string tag)
    {
        var position = SyntaxEditor.CaretPosition;
        SyntaxEditor.Document.Insert(position.Index, $"<{tag} />");
    }

    private string FormatHtml(string html)
    {
        html = RemoveDuplicateQuotes(html);

        var options = new HtmlParserOptions();
        var parser = new HtmlParser(options);
        using (var document = parser.ParseDocument(html))
        {
            using (var writer = new StringWriter())
            {
                document.ToHtml(writer, new PrettyMarkupFormatter());
                var formattedHtml = writer.ToString();

                return CleanFormattedHtml(formattedHtml);
            }
        }
    }

    private string RemoveDuplicateQuotes(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var body = document.DocumentNode
            .ChildNodes.First(x => x.Name == "html")
            .ChildNodes.First(x => x.Name == "body");

        foreach (var child in body.ChildNodes)
        {
            if (child.Name == "block" && child.ChildNodes.Count(x => x.Name == "quote") > 1)
            {
                var quoteNode = HtmlNode.CreateNode("<quote></quote>");

                foreach (var innerChild in child.ChildNodes.Where(x => x.Name == "quote").ToList())
                {
                    innerChild.Name = "block";
                    innerChild.Remove();
                    quoteNode.AddChild(innerChild);
                }

                child.AddChild(quoteNode);
            }
        }

        return body.OuterHtml;
    }

    private string CleanFormattedHtml(string html)
    {
        var lines = html.Split(new[] { '\n' })
            .Where(x => x.Trim().Length > 0);
        var finalLines = new List<string>();

        foreach (var line in lines)
        {
            var indentationAmount = Math.Max(line.Length - line.TrimStart().Length - 1, 0); // Force everything to be 1 less
            var indentation = new string(' ', indentationAmount);
            var parts = line.Split("  ")
                .Where(x => string.IsNullOrWhiteSpace(x) == false);

            foreach (var part in parts)
            {
                var formattedLine = $"{indentation}{part.Trim()}";

                if (formattedLine.EndsWith("</block>") || formattedLine.EndsWith("</heading>") ||
                    formattedLine.EndsWith("</chapter>"))
                {
                    formattedLine += "\r\n";
                }

                finalLines.Add(formattedLine);
            }
        }

        for (var x = 0; x < finalLines.Count; x++)
        {
            var line = finalLines[x].TrimEnd();
            var nextLine = x < finalLines.Count - 1
                ? finalLines[x + 1].TrimStart()
                : "";

            if (line.EndsWith("</verse>"))
            {
                line += nextLine;
                finalLines[x] = line;
                finalLines.RemoveAt(x + 1);
            }
        }

        var text = finalLines
            .StringJoin("\r\n");

        return text
            .Replace("<break></break>", "<break />");
    }

    #endregion
}
