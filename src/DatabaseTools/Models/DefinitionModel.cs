
using DatabaseTools.Extensions;
using Newtonsoft.Json;
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

            public TableModel View { get; set; }

            #endregion

            #region Methods

            public override void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format("-- {0}", this.DefinitionName));
                sb.AppendLine($"DROP {this.Type} IF EXISTS {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                string strPattern = $"(CREATE\\s*{this.Type}\\s*[\\[]?)([\\[]?{SchemaName}[\\.]?[\\]]?[\\.]?[\\[]?)?({this.DefinitionName})([\\]]?)";

                string strDefinitionReplacement = $"CREATE {this.Type} {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.DefinitionName}{quoteCharacterEnd}";

                this.Definition = System.Text.RegularExpressions.Regex.Replace(this.Definition, strPattern, strDefinitionReplacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                Definition = Definition.Replace("\t", "    ");

                sb.AppendLine(this.Definition);
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