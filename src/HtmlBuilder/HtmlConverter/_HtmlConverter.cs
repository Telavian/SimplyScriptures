using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;
using SimplyScriptures.Common.Extensions;

namespace HtmlBuilder.HtmlConverter;

public abstract class HtmlConverter
{
    public abstract int DefaultIndentation { get; }

    public string Convert(Stream data)
    {
        var document = new HtmlDocument();
        document.Load(data);

        var htmlNode = document.DocumentNode.ChildNodes.FindFirst("html");
        var bodyNode = htmlNode.ChildNodes.FindFirst("body");

        var blocks = new List<HtmlNode>();

        foreach (var child in bodyNode.ChildNodes)
        {
            ProcessNode(null, child, blocks);
        }

        return ConvertBlocksToHtml(blocks);
    }

    protected abstract HtmlNode? CreateNewNode(HtmlNode? parent, HtmlNode node, List<HtmlNode> blocks);

    protected static HtmlNode CreatePlaceholderNode(string name, HtmlNode node)
    {
        var styleAttributes = node.GetAttributeValue("style", "");
        var singleItems = styleAttributes.Split(";");

        var attributes = " ";
        foreach (var singleItem in singleItems)
        {
            var parts = singleItem.Split(":");
            if (parts.Length != 2)
            {
                continue;
            }

            attributes += $"{parts[0]}=\"{parts[1].Trim()}\"";
        }

        return HtmlNode.CreateNode($"<{name}{attributes}></{name}>");
    }

    private void ProcessNode(HtmlNode? parent, HtmlNode node, List<HtmlNode> blocks)
    {
        var newNode = CreateNewNode(parent, node, blocks);

        if (newNode == null)
        {
            if (node.HasChildNodes == false)
            {
                return;
            }

            foreach (var child in node.ChildNodes)
            {
                ProcessNode(parent, child, blocks);
            }

            return;
        }

        if (newNode.Name == "#text")
        {
            switch (node.Name)
            {
                case "#text":
                    parent!.AddChild(node);
                    break;
                case "span" when string.IsNullOrWhiteSpace(node.InnerText):
                    parent!.AddChild(HtmlNode.CreateNode("&#32;"));
                    break;
                default:
                    {
                        foreach (var child in node.ChildNodes)
                        {
                            parent!.AddChild(child);
                        }

                        break;
                    }
            }

            return;
        }

        var isTopNode = newNode.CheckTopNode();
        if (isTopNode)
        {
            TrimEmptyBlocks(blocks);
            blocks.Add(newNode);

            // Use inner child for further operations
            if (newNode.HasChildNodes && newNode.ChildNodes.Count == 1)
            {
                newNode = newNode.ChildNodes[0];
            }
        }
        else
        {
            switch (newNode.Name)
            {
                case "quote":
                    {
                        TrimEmptyBlocks(blocks);

                        var lastBlock = blocks.Last();
                        var isContainerNode = lastBlock.CheckContainerNode();
                        switch (isContainerNode)
                        {
                            case false:
                                blocks.Add(newNode);
                                break;
                            default:
                                if (lastBlock.HasChildNodes && newNode.CheckMoreIndented(lastBlock.ChildNodes.Last()))
                                {
                                    lastBlock.ChildNodes.Last().AddChild(newNode);
                                }
                                else
                                {
                                    lastBlock.AddChild(newNode);
                                }

                                break;
                        }

                        break;
                    }

                default:
                    if (parent == null)
                    {
                        var outer = CreatePlaceholderNode("block", node);
                        outer.AddChild(newNode);

                        newNode = outer;
                    }
                    else if (parent!.ChildNodes.LastOrDefault().CheckCombinableNode(newNode))
                    {
                        newNode = parent!.ChildNodes.Last();
                        newNode.AddChild(HtmlNode.CreateNode("&#32;"));
                    }
                    else
                    {
                        parent!.AddChild(newNode);
                    }

                    break;
            }
        }

        if (node.Name == "#text")
        {
            newNode.AddChild(node);
        }

        foreach (var child in node.ChildNodes)
        {
            ProcessNode(newNode, child, blocks);
        }
    }

    private static void TrimEmptyBlocks(List<HtmlNode> blocks)
    {
        if (blocks.Count == 0)
        {
            return;
        }

        var lastItem = blocks.Last();

        if (lastItem.Name == "block" && lastItem.HasChildNodes == false)
        {
            blocks.RemoveAt(blocks.Count - 1);
        }
    }

    private static string ConvertBlocksToHtml(IReadOnlyCollection<HtmlNode> blocks)
    {
        var html = blocks
            .Select(x => ConvertToString(null, x))
            .Where(x =>
            {
                var node = HtmlNode.CreateNode(x);
                if (node == null || node.HasChildNodes == false)
                {
                    return false;
                }

                var validChildren = node.ChildNodes
                    .Where(x => x.Name != "#text" || string.IsNullOrWhiteSpace(x.InnerText) == false)
                    .ToArray();

                if (node.Name == "block")
                {
                    switch (validChildren.Length)
                    {
                        case 0:
                            return false;
                        case 1 when validChildren[0].Name == "break":
                            return false;
                    }
                }

                return true;
            })
            .StringJoin("\r\n\r\n");

        return $"<html><head>\r\n{_styles}\r\n</head><body>\r\n{html}\r\n<body></html>";
    }

    private static string ConvertToString(HtmlNode? parent, HtmlNode node, int depth = 0)
    {
        var attributes = node.Name != "a"
         ? ""
         : node.Attributes
             .Where(x => string.IsNullOrWhiteSpace(x.Value) == false)
             .Select(x => $"{x.Name}=\"{x.Value}\"")
             .StringJoin(" ");

        if (attributes.Length > 0)
        {
            attributes = $" {attributes}";
        }

        if (node.Name == "#text")
        {
            return string.IsNullOrWhiteSpace(node.InnerText)
                ? ""
                : parent!.Name switch
                {
                    "verse" or "chapter" => node.InnerText.Trim(),
                    _ => node.InnerText,
                };
        }

        var text = "";
        if (node.Name == "a" && node.HasAttributes == false)
        {
            foreach (var child in node.ChildNodes)
            {
                text += ConvertToString(node, child, depth + 1);
            }

            return text;
        }

        if (node.HasChildNodes == false)
        {
            return "";
        }

        text = $"<{node.Name}{attributes}>";

        foreach (var child in node.ChildNodes)
        {
            text += ConvertToString(node, child, depth + 1);
        }

        if (depth == 0)
        {
            text = text.Trim();
            text += "\r\n";
        }

        text += $"</{node.Name}>";
        return text;
    }

    private const string _styles = @"<style>
@font-face {
	font-family: 'CalibriRegular';
	src: url('https://simplyscriptures.com/fonts/calibri-regular.woff2');
}

@font-face {
	font-family: 'Cambria';
	src: url('https://simplyscriptures.com/fonts/Cambria.woff2');
}

@font-face {
	font-family: 'EBGaramondItalic';
	src: url('https://simplyscriptures.com/fonts/EBGaramond-Italic.woff2');
}

@font-face {
	font-family: 'EBGaramondRegular';
	src: url('https://simplyscriptures.com/fonts/EBGaramond-Regular.woff2');
}

@font-face {
	font-family: 'ETBemboBold';
	src: url('https://simplyscriptures.com/fonts/ETBembo-Bold.woff2');
}

@font-face {
	font-family: 'ETBemboItalic';
	src: url('https://simplyscriptures.com/fonts/ETBembo-Italic.woff2');
}

@font-face {
	font-family: 'ETBembo';
	src: url('https://simplyscriptures.com/fonts/ETBembo.woff2');
}

@font-face {
	font-family: 'GillSansBold';
	src: url('https://simplyscriptures.com/fonts/Gill sans mt bold.woff2');
}

@font-face {
	font-family: 'GillSansItalic';
	src: url('https://simplyscriptures.com/fonts/Gill sans mt italic.woff2');
}

@font-face {
	font-family: 'GillSans';
	src: url('https://simplyscriptures.com/fonts/Gill Sans mt.woff2');
}

@font-face {
	font-family: 'Helvetica';
	src: url('https://simplyscriptures.com/fonts/Helvetica.woff2');
}

@font-face {
	font-family: 'Symbol';
	src: url('https://simplyscriptures.com/fonts/symbol.woff2');
}

@font-face {
	font-family: 'TimesNewRoman';
	src: url('https://simplyscriptures.com/fonts/times-new-roman.woff2');
}


body {
	margin-left: 150px;
	margin-right: 150px;
	font-size: 1.35em;
	font-family: 'ETBembo';
}

mainheading {
	display: flex;
	justify-content: center;
	font-weight: bold;
	font-size: 5em;
	font-family: 'ETBembo';
    font-weight: 500;
	padding-top: 150px;
}

subheading {
	display: flex;
	justify-content: center;
	font-weight: bold;
	font-size: 3em;
	font-family: 'ETBembo';
    font-weight: 500;
	padding-bottom: 50px;
}

blockheading {
	font-size: 1.1em;
	display: block;
	font-weight: 500;
	margin-bottom: 10px;
	font-family: 'GillSans';
	text-transform: uppercase;
}

heading {
	font-size: 1.1em;
	display: block;
	font-weight: 500;
	margin-bottom: 10px;
	font-family: 'GillSansItalic';
    font-style: italic;
}

highlightheading {
	display: flex;
	justify-content: center;	
    font-size: 2em;
	display: block;	
	margin-bottom: 10px;
	font-family: 'GillSans';
    background-color: #D9E2F3;
}

inscription {
    font-size: 1.5em;
    font-weight: bold;
    text-transform: uppercase;
}

titlephrase {    
    text-transform: uppercase;
}

book {
	display: flex;
	justify-content: center;
	font-family: 'ETBembo';
	font-size: 3em;
	margin-top: 5em;
}

chapter {
	font-size: 2em;
	float: left;
	margin-left: -1.25em;
	margin-top: -0.25em;
	color: #A6A6A6;
	font-family: 'GillSans';
}

verse {
	font-size: 0.5em;
	color: #808080;
	vertical-align: text-top;
	display: inline-block;
	font-family: 'GillSans';
}

block {
	display: block;
	margin-bottom: 1em;
}

block::after {	
	display: block;
	/* content: '\A'; */
	/* white-space: pre; */
}

break::before {	
	content: '\A'; 
	white-space: pre;	
}

break {
	
}

bindent::before {	
	content: '\A'; 
	white-space: pre;	
}

bindent {
	display: block;
	margin-left: 2em;
	margin-top: -1em;
}

indent {
	display: block;
	margin-left: 2em;
	/* margin-top: -1em; */
}

quote::before {	
	content: '\A'; 
	white-space: pre;	
}

quote {	
	display: block;
	margin-left: 2em;
	margin-top: -1em;
    /* margin-bottom: 0.5em; */
}

border {
}

box {
}

superscript {
	font-size: 0.5em;
	color: #808080;
	vertical-align: text-top;
	display: inline-block;
	font-family: 'GillSans';
}

reference {
    background-color: #F2F2F2;
}

referencenote {
    background-color: #F2F2F2;
    float: right;    
    color: red;
    font-weight: bold;
    /* color: #808080; */
    margin-right: -300px;
    width: 275px !important;
    clear: both;
}

under {
	text-decoration-line: underline;
	text-decoration-style: solid;
	text-decoration-color: #AAAAAA;
}

dunder {
	text-decoration-line: underline;
	text-decoration-style: dotted;
	text-decoration-color: #AAAAAA;
}

ddunder {
	text-decoration-line: underline;
	text-decoration-style: dashed;
	text-decoration-color: #AAAAAA;
}

italic {
	font-style: italic;
}

bold {
	font-weight: bold;
}

dot::before {	
	content: '\A \2022'; 
	white-space: pre;
}

dot {	
	display: block;
	margin-left: 2em;
	margin-top: -1em;    
}

symbol {
    font-size: 2em;
    font-family: 'CalibriRegular';
}

note {
    float: right;    
    color: red;
    font-weight: bold;
    /* color: #808080; */
    margin-right: -300px;
    width: 275px !important;    
    clear: both;
}

original {
}

printer {
}

manuscript {
}

jst {
}

sectionheader {
}

sectiontitle {
}

sectionnumber {
}

sectiontime {
}

sectioninfo {
}

sectionreference {
}

sectionreference1 {
}

sectionreference2 {
}

sectionreference3 {
}
</style>";
}
