using System.Text;
using System.Xml.XPath;
using HtmlAgilityPack;
using SimplyScriptures.Console.Extensions.Html;

namespace SimplyScriptures.Console.Extensions;

public static class HtmlExtensions
{
    public static string GetFullInnerText(this HtmlNode node)
    {
        var processNodes = new Stack<HtmlNode>();
        processNodes.Push(node);

        var text = new StringBuilder();

        while (processNodes.Count > 0)
        {
            var currentNode = processNodes.Pop();

            switch (currentNode.HasChildNodes)
            {
                case false:
                    {
                        var actualText = currentNode.InnerText
                            .Replace($"{(char)8204}", "")
                            .Trim();

                        if (actualText.Length > 0)
                        {
                            text.Append(actualText.Trim());
                            text.Append(' ');
                        }

                        break;
                    }

                default:
                    {
                        foreach (var child in currentNode.ChildNodes.Reverse())
                        {
                            processNodes.Push(child);
                        }

                        break;
                    }
            }
        }

        return text.ToString();
    }

    /// <summary>
    /// Allows the use of regex matching for node selection
    /// <example><code>//div[regex-match(text(), 'x.*?y')]</code></example>
    /// </summary>
    public static IEnumerable<HtmlNode> SelectXPathNodes(this HtmlNode node, string xpath)
    {
        var navigator = node.CreateNavigator();
        return SelectXPathNodes(navigator!, xpath);
    }

    /// <summary>
    /// Allows the use of regex matching for node selection
    /// <example><code>//div[regex-match(text(), 'x.*?y')]</code></example>
    /// </summary>
    public static IEnumerable<HtmlNode> SelectXPathNodes(this XPathNavigator navigator, string xpath)
    {
        ArgumentNullException.ThrowIfNull(navigator);

        var expr = navigator.Compile(xpath);
        expr.SetContext(new HtmlXsltContext());

        var eval = navigator.Evaluate(expr);

        if (eval is XPathNodeIterator it)
        {
            while (it.MoveNext())
            {
                if (it.Current is HtmlNodeNavigator n && n.CurrentNode != null)
                {
                    yield return n.CurrentNode;
                }
            }
        }
    }
}
