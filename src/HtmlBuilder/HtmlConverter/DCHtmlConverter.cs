using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;

namespace HtmlBuilder.HtmlConverter;

public class DCHtmlConverter : HtmlConverter
{
    public override int DefaultIndentation => 32;

    protected override HtmlNode? CreateNewNode(HtmlNode? parent, HtmlNode node, List<HtmlNode> blocks)
    {
        if (node.Name == "#text")
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
            {
                return null;
            }

            if (parent!.Name != "verse" && node.InheritsClass("s18", "s63"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (parent!.Name != "chapter" && node.InheritsClass("s72", "s74"))
            {
                return CreatePlaceholderNode("chapter", node);
            }

            return node;
        }

        if (node.HasClass("s22"))
        {
            return CreatePlaceholderNode("italic", node);
        }

        if (node.HasClass("s13", "s37"))
        {
            return CreatePlaceholderNode("titlephrase", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("original", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("printer", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("book", node);
        }

        if (node.HasClass("s24", "s53", "s66", "s78"))
        {
            return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s61"))
        {
            return CreatePlaceholderNode("verse", node);
        }

        if (node.HasClass("s6", "s21", "s38", "s60", "s62"))
        {
            return CreatePlaceholderNode("note", node);
        }

        if (node.HasClass("s19", "s57", "s73"))
        {
            return CreatePlaceholderNode("blockheading", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("referencenote", node);
        }

        if (node.HasClass("s49"))
        {
            return CreatePlaceholderNode("reference", node);
        }

        switch (node.Name)
        {
            case "h4":
            case "h2":
            case "h3":
            case "h1":
                return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s39", "s55"))
        {
            return CreatePlaceholderNode("highlightheading", node);
        }

        if (node.HasClass("s40"))
        {
            return CreatePlaceholderNode("manuscript", node);
        }

        if (node.HasClass("s50"))
        {
            return CreatePlaceholderNode("dunder", node);
        }

        switch (node.Name)
        {
            case "table":
                return CreatePlaceholderNode("sectionheader", node);
            case "tr":
            case "td":
                return null;
        }

        if (node.HasClass("s33"))
        {
            return CreatePlaceholderNode("sectiontitle", node);
        }

        if (node.HasClass("s32"))
        {
            return CreatePlaceholderNode("sectionnumber", node);
        }

        if (node.HasClass("s34"))
        {
            return CreatePlaceholderNode("sectiontime", node);
        }

        if (node.HasClass("s36", "s45"))
        {
            return CreatePlaceholderNode("sectioninfo", node);
        }

        if (node.HasClass("s35"))
        {
            return CreatePlaceholderNode("sectionreference", node);
        }

        if (node.HasClass())
        {
            var outer = CreatePlaceholderNode("reference", node);
            var inner = CreatePlaceholderNode("dunder", node);
            outer.ChildNodes.Add(inner);
            return outer;
        }

        if (node.Name == "span")
        {
            if (node.HasClass())
            {
                return CreatePlaceholderNode("chapter", node);
            }

            if (node.HasClass("s18", "s63", "s75", "s77"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (node.HasClass("s20", "s54", "s58", "s64", "s65"))
            {
                return CreatePlaceholderNode("superscript", node);
            }

            if (node.HasClass())
            {
                return CreatePlaceholderNode("inscription", node);
            }
        }

        switch (node.Name)
        {
            case "i":
                return CreatePlaceholderNode("italic", node);
            case "u":
                return CreatePlaceholderNode("dunder", node);
            case "br":
                return CreatePlaceholderNode("break", node);
            case "b":
                return CreatePlaceholderNode("bold", node);
            case "ul":
                return CreatePlaceholderNode("block", node);
            case "ol":
                return CreatePlaceholderNode("block", node);
            case "li":
                return CreatePlaceholderNode("dot", node);
            case "span":
                return CreatePlaceholderNode("#text", node);
            case "a":
                {
                    var href = node.GetAttributeValue("href", "");
                    var attributes = string.IsNullOrWhiteSpace(href) == false
                        ? $" href=\"{href}\""
                        : "";

                    if (string.IsNullOrWhiteSpace(href))
                    {
                        return null;
                    }

                    return HtmlNode.CreateNode($"<a{attributes}></a>");
                }

            case "p":
                {
                    if (node.HasClass("s70"))
                    {
                        return CreatePlaceholderNode("mainheading", node);
                    }

                    if (node.HasClass("s16"))
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (node.HasClass())
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (node.HasClass())
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (blocks.Count > 0 && (blocks.Last().InnerText.Trim().EndsWith(":") || CheckNodeIndented(node)))
                    {
                        return CreatePlaceholderNode("quote", node);
                    }

                    if (node.HasClass("s5"))
                    {
                        var outer = CreatePlaceholderNode("block", node);
                        var inner = CreatePlaceholderNode("dunder", node);
                        outer.ChildNodes.Add(inner);
                        return outer;
                    }

                    return CreatePlaceholderNode("block", node);
                }

            default:
                throw new Exception($"Unknown node: {node.OuterHtml}");
        }
    }

    private bool CheckNodeIndented(HtmlNode node)
    {
        var tempNode = CreatePlaceholderNode("temp", node);
        var padding = tempNode.GetNodeLeftPadding();

        return padding > DefaultIndentation;
    }
}
