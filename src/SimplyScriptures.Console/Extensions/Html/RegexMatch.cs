using System.Text.RegularExpressions;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SimplyScriptures.Console.Extensions.Html;

internal class RegexMatch : HtmlXsltFunction
{
    private readonly Dictionary<string, Regex> _regexLookup = new Dictionary<string, Regex>();

    public RegexMatch(HtmlXsltContext context, string name)
        : base(context, null, name, Array.Empty<XPathResultType>())
    {
    }

    public override XPathResultType ReturnType => XPathResultType.Boolean;
    public override int Minargs => 2;

    public override object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
    {
        if (args.Length < 2)
        {
            return false;
        }

        var input = ConvertToString(args[0], false, null) ?? "";
        var pattern = ConvertToString(args[1], false, null) ?? "";

        _regexLookup.TryGetValue(pattern, out var regex);

        if (regex == null)
        {
            regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _regexLookup[pattern] = regex;
        }

        return regex.IsMatch(input);
    }
}
