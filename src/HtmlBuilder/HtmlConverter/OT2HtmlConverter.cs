using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;

namespace HtmlBuilder.HtmlConverter;

public class OT2HtmlConverter : HtmlConverter
{
    public override int DefaultIndentation => 42;

    protected override HtmlNode? CreateNewNode(HtmlNode? parent, HtmlNode node, List<HtmlNode> blocks)
    {
        if (node.Name == "#text")
        {
            if (string.IsNullOrWhiteSpace(node.InnerText))
            {
                return null;
            }

            if (parent!.Name != "verse" && node.InheritsClass("s137", "s144"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (parent!.Name != "chapter" && node.InheritsClass("s143"))
            {
                return CreatePlaceholderNode("chapter", node);
            }

            return node;
        }

        if (node.HasClass("s135"))
        {
            return CreatePlaceholderNode("book", node);
        }

        if (node.HasClass("s138"))
        {
            return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s136"))
        {
            return CreatePlaceholderNode("chapter", node);
        }

        if (node.HasClass("s140", "s141", "s142", "s147"))
        {
            return CreatePlaceholderNode("note", node);
        }

        if (node.HasClass("s20") || node.HasClass("s22"))
        {
            return CreatePlaceholderNode("blockheading", node);
        }

        switch (node.Name)
        {
            case "h4":
            case "h2":
            case "h3":
            case "h1":
                return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s145"))
        {
            return CreatePlaceholderNode("referencenote", node);
        }

        if (node.HasClass("s148"))
        {
            return CreatePlaceholderNode("italic", node);
        }

        if (node.HasClass("s149"))
        {
            return CreatePlaceholderNode("symbol", node);
        }

        if (node.Name == "span")
        {
            if (node.HasClass("s137"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (node.HasClass("s139", "s146"))
            {
                return CreatePlaceholderNode("superscript", node);
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

                    return HtmlNode.CreateNode($"<a{attributes}></a>");
                }

            case "p":
                {
                    if (node.HasClass("s1"))
                    {
                        return CreatePlaceholderNode("mainheading", node);
                    }

                    if (node.HasClass("s2"))
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

                    if (node.HasClass("s7"))
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
