using HtmlAgilityPack;
using System;
using System.Linq;

namespace HtmlBuilder.Extensions;

public static class NodeExtensions
{
    public static void AddChild(this HtmlNode node, HtmlNode child)
    {
        if (node.Name == "#text")
        {
            throw new Exception("Can't add a child to a text node");
        }

        node.ChildNodes.Add(child);
    }

    public static bool InheritsClass(this HtmlNode node, params string[] classes)
    {
        foreach (var className in classes)
        {
            var currentNode = node;
            while (currentNode != null)
            {
                if (currentNode.HasClass(className))
                {
                    return true;
                }

                currentNode = currentNode.ParentNode;
            }
        }

        return false;
    }

    public static bool HasClass(this HtmlNode node, params string[] classes)
    {
        return classes
            .Any(c => node.HasClass(c));
    }

    public static bool CheckContainerNode(this HtmlNode node)
    {
        var name = node.Name;
        return name == "block" || name == "quote";
    }

    public static bool CheckTopNode(this HtmlNode node)
    {
        if (node.Name == "block" || node.Name == "book" || node.Name == "chapter" || node.Name.Contains("heading", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        switch (node.Name)
        {
            case "note":
                return true;
            default:
                return false;
        }
    }

    public static bool CheckCombinableNode(this HtmlNode? last, HtmlNode newNode)
    {
        if (last == null || last.Name != newNode.Name)
        {
            return false;
        }

        switch (last.Name)
        {
            case "dunder":
            case "ddunder":
                return true;
            default:
                return false;
        }
    }

    public static bool CheckMoreIndented(this HtmlNode newNode, HtmlNode last)
    {
        var newPadding = newNode.GetNodeLeftPadding();
        var lastPadding = last.GetNodeLeftPadding();

        return lastPadding > 0 && newPadding > 0 && newPadding > lastPadding;
    }

    public static int GetNodeLeftPadding(this HtmlNode node)
    {
        var padding = node.GetNodeAttribute("padding-left", "pt");
        var indent = node.GetNodeAttribute("text-indent", "pt");

        switch (padding)
        {
            case 0:
                return 0;
            default:
                return padding + indent;
        }
    }

    public static int GetNodeAttribute(this HtmlNode node, string name, string unit)
    {
        var padding = node.GetAttributeValue(name, "")
            .Trim()
            .Replace($"{unit};", "")
            .Replace($"{unit}", "");

        if (string.IsNullOrWhiteSpace(padding))
        {
            return 0;
        }

        return int.Parse(padding);
    }
}
