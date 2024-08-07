﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlBuilder.Extensions;

namespace HtmlBuilder.HtmlConverter;

public class NTHtmlConverter : HtmlConverter
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

            return parent!.Name != "verse" && node.InheritsClass("s16")
                ? CreatePlaceholderNode("verse", node)
                : parent!.Name != "chapter" && node.InheritsClass("s14") ? CreatePlaceholderNode("chapter", node) : node;
        }

        if (node.HasClass("s12", "s13"))
        {
            return CreatePlaceholderNode("book", node);
        }

        if (node.HasClass("s21"))
        {
            return CreatePlaceholderNode("heading", node);
        }

        if (node.HasClass("s23"))
        {
            return CreatePlaceholderNode("chapter", node);
        }

        if (node.HasClass("s35"))
        {
            return CreatePlaceholderNode("verse", node);
        }

        if (node.HasClass("s20", "s25", "s28", "s29", "s31", "s36", "s39"))
        {
            return CreatePlaceholderNode("note", node);
        }

        if (node.HasClass())
        {
            return CreatePlaceholderNode("blockheading", node);
        }

        if (node.HasClass("s22"))
        {
            return CreatePlaceholderNode("referencenote", node);
        }

        if (node.HasClass("s26"))
        {
            return CreatePlaceholderNode("reference", node);
        }

        if (node.HasClass("s30"))
        {
            var outer = CreatePlaceholderNode("reference", node);
            var inner = CreatePlaceholderNode("dunder", node);
            outer.ChildNodes.Add(inner);
            return outer;
        }

        if (node.Name == "span")
        {
            if (node.HasClass("s33"))
            {
                return CreatePlaceholderNode("chapter", node);
            }

            if (node.HasClass("s15", "s16"))
            {
                return CreatePlaceholderNode("verse", node);
            }

            if (node.HasClass("s27"))
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

                    return string.IsNullOrWhiteSpace(href) ? null : HtmlNode.CreateNode($"<a{attributes}></a>");
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

                    if (node.HasClass("s8", "s18", "s24"))
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
