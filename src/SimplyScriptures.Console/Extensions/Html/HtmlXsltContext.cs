using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace SimplyScriptures.Console.Extensions.Html;

internal class HtmlXsltContext : XsltContext
{
    public HtmlXsltContext()
        : base(new NameTable())
    {
    }

    public override int CompareDocument(string baseUri, string nextbaseUri)
    {
        throw new NotImplementedException();
    }

    public override bool PreserveWhitespace(XPathNavigator node)
    {
        throw new NotImplementedException();
    }

    protected virtual IXsltContextFunction CreateHtmlXsltFunction(string? prefix, string? name, XPathResultType[] argTypes)
    {
        return HtmlXsltFunction.GetBuiltIn(this, prefix, name, argTypes)!;
    }

    public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] argTypes)
    {
        return CreateHtmlXsltFunction(prefix, name, argTypes);
    }

    public override IXsltContextVariable ResolveVariable(string prefix, string name)
    {
        throw new NotImplementedException();
    }

    public override bool Whitespace => true;
}
