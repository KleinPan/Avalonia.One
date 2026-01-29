using System.Text.RegularExpressions;

namespace One.Base.Helpers;

public class HTMLHelper
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.IgnoreCase);

    public static string HtmlFilter(string str)
    {
        if (string.IsNullOrEmpty(str)) return string.Empty;
        return HtmlTagRegex.Replace(str, "");
    }
}