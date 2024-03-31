﻿using System.Xml.XPath;

namespace SimplyScriptures.Common.Extensions;

public static class XPathExtensions
{
    public static bool IsValidXPath(this string xpath)
    {
        try
        {
            XPathExpression.Compile(xpath);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
