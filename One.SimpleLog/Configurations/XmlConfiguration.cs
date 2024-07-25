using System.IO;
using System.Xml.XPath;

namespace One.SimpleLog.Configurations;

internal class XmlConfiguration
{
    private readonly XPathNavigator? xPathNavigator ;

    internal XmlConfiguration(string configPath)
    {
        if (File.Exists(configPath))
            xPathNavigator = new XPathDocument(configPath).CreateNavigator();
    }

    internal string? Get(string nodeName, string attributeName)
    {
        var node = xPathNavigator?.SelectSingleNode(nodeName);
        if (node != null)
            if (node.MoveToAttribute(attributeName, ""))
                return node.Value;

        return null;
    }
}
