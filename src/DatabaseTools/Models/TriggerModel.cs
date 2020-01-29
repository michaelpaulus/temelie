
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

                sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.objects INNER JOIN sys.schemas ON sys.objects.schema_id = sys.schemas.schema_id WHERE sys.objects.name = '{TriggerName}' AND sys.schemas.name = '{SchemaName}')");
                sb.AppendLine($"    DROP TRIGGER {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TriggerName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public void AppendScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                Definition = Definition.Replace("'", "''").Replace("\t", "    ");

                sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.objects INNER JOIN sys.schemas ON sys.objects.schema_id = sys.schemas.schema_id WHERE sys.objects.name = '{TriggerName}' AND sys.schemas.name = '{SchemaName}')");
                sb.AppendLine(string.Format("EXEC sp_executesql @statement = N'{0}'", this.Definition));
                sb.AppendLine("GO");
            }

            #endregion

        }
    }


}