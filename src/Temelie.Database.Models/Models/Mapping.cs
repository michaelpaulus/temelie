namespace Temelie.Database.Models;

public class Mapping
{

    protected string GetAttributeValue(System.Xml.XmlNode? node, string name)
    {
        if (node?.Attributes?[name] is { } attribute)
        {
            return attribute.Value;
        }
        return string.Empty;
    }

}
