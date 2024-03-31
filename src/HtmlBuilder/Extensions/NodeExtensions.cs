using System;
using System.Linq;
using HtmlAgilityPack;

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
            .Any(node.HasClass);
    }

    public static bool CheckContainerNode(this HtmlNode node)
    {
        var name = node.Name;
        return name is "block" or "quote";
    }

    public static bool CheckTopNode(this HtmlNode node)
    {
        return node.Name == "block" || node.Name == "book" || node.Name == "chapter" || node.Name.Contains("heading", StringComparison.OrdinalIgnoreCase)
|| node.Name switch
            {
                "note" => true,
                _ => false,
            };
    }

    public static bool CheckCombinableNode(this HtmlNode? last, HtmlNode newNode)
    {
        return last != null && last.Name == newNode.Name
&& last.Name switch
            {
                "dunder" or "ddunder" => true,
                _ => false,
            };
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

        return padding switch
        {
            0 => 0,
            _ => padding + indent,
        };
    }

    public static int GetNodeAttribute(this HtmlNode node, string name, string unit)
    {
        var padding = node.GetAttributeValue(name, "")
            .Trim()
            .Replace($"{unit};", "")
            .Replace($"{unit}", "");

        return string.IsNullOrWhiteSpace(padding) ? 0 : int.Parse(padding);
    }
}
