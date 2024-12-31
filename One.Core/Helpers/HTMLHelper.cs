using One.Base.ExtensionMethods;

using System.Text.RegularExpressions;

namespace One.Base.Helpers;

public class HTMLHelper
{
    public static string HtmlFilter(string str)
    {
        if (str.IsNullOrEmptyStr())
        {
            return string.Empty;
        }
        Regex regex = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
        return regex.Replace(str, "");
    }
}