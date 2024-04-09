using Cornerstone.Database.Extensions;

namespace Cornerstone.Database
{
    namespace Models
    {
        public class DefinitionModel : DatabaseObjectModel
        {

            #region Properties

            public string DefinitionName { get; set; }
            public string SchemaName { get; set; }
            public string Definition { get; set; }
            public string XType { get; set; }

            public string Type
            {
                get
                {
                    switch (this.XType)
                    {
                        case "P":
                            return "PROCEDURE";
                        case "V":
                            return "VIEW";
                    }
                    return "FUNCTION";
                }
            }

            public TableModel View { get; set; }

            #endregion

            #region Methods

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format("-- {0}", this.DefinitionName));
                sb.AppendLine($"DROP {this.Type} IF EXISTS {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                string strPattern = $"(CREATE\\s*{this.Type}\\s*[\\[]?)([\\[]?{SchemaName}[\\.]?[\\]]?[\\.]?[\\[]?)?({this.DefinitionName})([\\]]?)";

                string strDefinitionReplacement = $"CREATE {this.Type} {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}";

                Definition = System.Text.RegularExpressions.Regex.Replace(Definition, strPattern, strDefinitionReplacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

                sb.AppendLine(Definition.Replace("\t", "    ").RemoveLeadingAndTrailingLines());
                sb.AppendLine("GO");

                if (View != null)
                {
                    TableModel.AddExtendedProperties(View, sb);
                }

            }

            #endregion

        }
    }

}
