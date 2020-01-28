
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
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
                        case "FN":
                        case "IF":
                            return "FUNCTION";
                    }
                    return string.Empty;
                }
            }

            #endregion

            #region Methods

            public override void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format("-- {0}", this.DefinitionName));
                sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.objects INNER JOIN sys.schemas ON sys.objects.schema_id = sys.schemas.schema_id WHERE sys.objects.name = '{DefinitionName}' AND sys.schemas.name = '{SchemaName}')");
                sb.AppendLine($"    DROP {this.Type} {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format("-- {0}", this.DefinitionName));
                sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.objects INNER JOIN sys.schemas ON sys.objects.schema_id = sys.schemas.schema_id WHERE sys.objects.name = '{this.DefinitionName}' AND sys.schemas.name = '{SchemaName}')");

                string strPattern = $"(CREATE\\s*{this.Type}\\s*[\\[]?)([\\[]?{SchemaName}[\\.]?[\\]]?[\\.]?[\\[]?)?({this.DefinitionName})([\\]]?)";

                string strDefinitionReplacement = $"CREATE {this.Type} {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}";

                this.Definition = System.Text.RegularExpressions.Regex.Replace(this.Definition, strPattern, strDefinitionReplacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

                sb.AppendLine(string.Format("EXEC sp_executesql @statement = N'{0}'", this.Definition.Replace("'", "''")));
                sb.AppendLine("GO");
            }

            #endregion

        }
    }


}