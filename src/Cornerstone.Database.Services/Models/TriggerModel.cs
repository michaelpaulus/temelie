
using Cornerstone.Database.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Cornerstone.Database
{
    namespace Models
    {
        public class TriggerModel
        {

            #region Properties

            public string TriggerName { get; set; }
            public string SchemaName { get; set; }
            public string Definition { get; set; }
            public string TableName { get; set; }

            #endregion

            #region Methods

            public void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine($"DROP TRIGGER IF EXISTS {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TriggerName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public void AppendScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
             
                sb.AppendLine(Definition.Replace("\t", "    ").RemoveLeadingAndTrailingLines());
                sb.AppendLine("GO");
            }

            #endregion

        }
    }


}