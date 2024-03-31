using System.Collections;
using System.Text;
using System.Xml.XPath;
using System.Xml.Xsl;
using HtmlAgilityPack;

namespace SimplyScriptures.Console.Extensions.Html;

internal abstract class HtmlXsltFunction : IXsltContextFunction
{
    protected HtmlXsltFunction(HtmlXsltContext context, string? prefix, string? name, XPathResultType[] argTypes)
    {
        Context = context;
        Prefix = prefix ?? "";
        Name = name ?? "";
        ArgTypes = argTypes;
    }

    public HtmlXsltContext Context { get; private set; }
    public string Prefix { get; private set; }
    public string Name { get; private set; }
    public XPathResultType[] ArgTypes { get; private set; }

    public virtual int Maxargs => Minargs;

    public virtual int Minargs => 1;

    public virtual XPathResultType ReturnType => XPathResultType.String;

    public abstract object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext);

    public static IXsltContextFunction? GetBuiltIn(HtmlXsltContext context, string? prefix, string? name, XPathResultType[] argTypes)
    {
        return name switch
        {
            "regex-match" => new RegexMatch(context, name),
            _ => null,// TODO: create other functions here
        };
    }

    public static string? ConvertToString(object? argument, bool outer, string? separator)
    {
        switch (argument)
        {
            case null:
                return null;
            case string s:
                return s;
            case XPathNodeIterator it:
                {
                    if (!it.MoveNext())
                    {
                        return null;
                    }

                    var sb = new StringBuilder();
                    do
                    {
                        if (it.Current is HtmlNodeNavigator n && n.CurrentNode != null)
                        {
                            if (sb.Length > 0 && separator != null)
                            {
                                sb.Append(separator);
                            }

                            sb.Append(outer ? n.CurrentNode.OuterHtml : n.CurrentNode.InnerHtml);
                        }
                    }
                    while (it.MoveNext());
                    return sb.ToString();
                }

            case IEnumerable enumerable:
                {
                    var sb = (StringBuilder?)null;
                    foreach (var arg in enumerable)
                    {
                        sb ??= new StringBuilder();

                        if (sb.Length > 0 && separator != null)
                        {
                            sb.Append(separator);
                        }

                        var s2 = ConvertToString(arg, outer, separator);
                        if (s2 != null)
                        {
                            sb.Append(s2);
                        }
                    }

                    return sb?.ToString();
                }

            default:
                return $"{argument}";
        }
    }
}
