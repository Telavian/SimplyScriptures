using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;

namespace HtmlBuilder.HtmlConverter;

public class BMHtmlConverter : HtmlConverter
{
    public override int DefaultIndentation => 45;

    protected override HtmlNode? CreateNewNode(HtmlNode? parent, HtmlNode node, List<HtmlNode> blocks)
    {
        if (node.Name == "#text")
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
            {
                return null;
            }

            if (parent!.Name != "verse" && node.InheritsClass("s16", "s20", "s26"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (parent!.Name != "chapter" && node.InheritsClass("s14"))
            {
                return CreatePlaceholderNode("chapter", node);
            }

            return node;
        }

        if (node.HasClass("s18"))
        {
            return CreatePlaceholderNode("italic", node);
        }

        if (node.HasClass("s7"))
        {
            return CreatePlaceholderNode("original", node);
        }

        if (node.HasClass("s50"))
        {
            return CreatePlaceholderNode("printer", node);
        }

        if (node.HasClass("s15", "s17"))
        {
            return CreatePlaceholderNode("book", node);
        }

        if (node.HasClass("s21"))
        {
            return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s19", "s51"))
        {
            return CreatePlaceholderNode("chapter", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("verse", node);
        }

        if (node.HasClass("s8", "s14", "s36", "s38"))
        {
            return CreatePlaceholderNode("note", node);
        }

        if (node.HasClass("s23", "s30"))
        {
            return CreatePlaceholderNode("blockheading", node);
        }

        if (node.HasClass("s25", "s33"))
        {
            return CreatePlaceholderNode("referencenote", node);
        }

        if (node.HasClass("s28"))
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

        if (node.HasClass("s41"))
        {
            return CreatePlaceholderNode("highlightheading", node);
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
            if (node.HasClass("s22", "s32"))
            {
                return CreatePlaceholderNode("chapter", node);
            }

            if (node.HasClass("s20", "s27"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (node.HasClass("s34", "s35", "s37"))
            {
                return CreatePlaceholderNode("superscript", node);
            }

            if (node.HasClass("s47"))
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
                    if (node.HasClass("s1"))
                    {
                        return CreatePlaceholderNode("mainheading", node);
                    }

                    if (node.HasClass("s2", "s16"))
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (node.HasClass("s3"))
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (node.HasClass("s4"))
                    {
                        return CreatePlaceholderNode("subheading", node);
                    }

                    if (blocks.Count > 0 && (blocks.Last().InnerText.Trim().EndsWith(":") || CheckNodeIndented(node)))
                    {
                        return CreatePlaceholderNode("quote", node);
                    }

                    if (node.HasClass("s6"))
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
