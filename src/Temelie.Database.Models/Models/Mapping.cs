namespace Temelie.Database
{
    namespace Models
    {
        public class Mapping
        {

            protected string GetAttributeValue(System.Xml.XmlNode node, string name)
            {
                if (node.Attributes != null && node.Attributes[name] != null)
                {
                    return node.Attributes[name].Value;
                }
                return string.Empty;
            }

        }
    }

}
