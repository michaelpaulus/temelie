
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
        public class ForeignKeyModel : DatabaseObjectModel
        {
            #region Properties

            public string ForeignKeyName { get; set; }
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public bool IsNotForReplication { get; set; }
            public string DeleteAction { get; set; }
            public string UpdateAction { get; set; }
            public string ReferencedTableName { get; set; }
            public string ReferencedSchemaName { get; set; }

            private IList<ForeignKeyDetailModel> _detail;
            public IList<ForeignKeyDetailModel> Detail
            {
                get
                {
                    if (this._detail == null)
                    {
                        this._detail = new List<ForeignKeyDetailModel>();
                    }
                    return this._detail;
                }
            }

            #endregion

            #region Methods

            public override void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                sb.Append($@"IF EXISTS (SELECT 1 FROM sys.foreign_keys INNER JOIN sys.schemas ON sys.foreign_keys.schema_id = sys.schemas.schema_id WHERE sys.foreign_keys.name = '{ForeignKeyName}' AND sys.schemas.name = '{SchemaName}')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys INNER JOIN sys.schemas ON foreign_keys.schema_id = schemas.schema_id WHERE foreign_keys.name = '{ForeignKeyName}' AND schemas.name = '{SchemaName}' AND foreign_keys.delete_referential_action_desc = '{DeleteAction}' AND foreign_keys.update_referential_action_desc = '{UpdateAction}'");
                var i = 0;

                foreach (var detail in Detail)
                {
                    i++;
                    sb.Append($@" AND
            EXISTS (SELECT 1 FROM sys.foreign_key_columns INNER JOIN sys.tables ON foreign_key_columns.parent_object_id = tables.object_id INNER JOIN sys.columns ON tables.object_id = columns.object_id AND columns.column_id = foreign_key_columns.parent_column_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE foreign_key_columns.constraint_object_id = foreign_keys.object_id AND schemas.name = '{SchemaName}' AND tables.name = '{TableName}' AND columns.name = '{detail.Column}' AND foreign_key_columns.constraint_column_id = {i}) AND
            EXISTS(SELECT 1 FROM sys.foreign_key_columns INNER JOIN sys.tables ON foreign_key_columns.referenced_object_id = tables.object_id INNER JOIN sys.columns ON tables.object_id = columns.object_id AND columns.column_id = foreign_key_columns.referenced_column_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE foreign_key_columns.constraint_object_id = foreign_keys.object_id AND schemas.name = '{ReferencedSchemaName}' AND tables.name = '{ReferencedTableName}' AND columns.name = '{detail.ReferencedColumn}' AND foreign_key_columns.constraint_column_id = {i})");
                }

                sb.AppendLine();

                sb.AppendLine($@"    )
    BEGIN
        ALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} DROP CONSTRAINT {quoteCharacterStart}{this.ForeignKeyName}{quoteCharacterEnd}
    END
END
GO");

            }

            public override void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                string strColumnNames = string.Empty;
                string strReferencedColumnNames = string.Empty;

                foreach (var item in this.Detail)
                {
                    string strColumnName = item.Column;
                    string strReferencedColumnName = item.ReferencedColumn;

                    if (strColumnNames.Length > 0)
                    {
                        strColumnNames += ",";
                    }

                    strColumnNames += $"{quoteCharacterStart}{strColumnName}{quoteCharacterEnd}";

                    if (strReferencedColumnNames.Length > 0)
                    {
                        strReferencedColumnNames += ",";
                    }

                    strReferencedColumnNames += $"{quoteCharacterStart}{strReferencedColumnName}{quoteCharacterEnd}"; ;
                }

                sb.AppendLine();

                sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys INNER JOIN sys.schemas ON foreign_keys.schema_id = schemas.schema_id WHERE foreign_keys.name = '{ForeignKeyName}' AND schemas.name = '{SchemaName}')");
                sb.AppendLine($"    ALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} WITH CHECK ADD CONSTRAINT {quoteCharacterStart}{this.ForeignKeyName}{quoteCharacterEnd} FOREIGN KEY ({strColumnNames})");
                sb.AppendLine($"    REFERENCES {quoteCharacterStart}{this.ReferencedSchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.ReferencedTableName}{quoteCharacterEnd} ({strReferencedColumnNames})");

                if (this.UpdateAction != "NO_ACTION")
                {
                    sb.AppendLine("    " + string.Format("ON UPDATE {0}", this.UpdateAction.Replace("_", " ")));
                }

                if (this.DeleteAction != "NO_ACTION")
                {
                   sb.AppendLine("    " + string.Format("ON DELETE {0}", this.DeleteAction.Replace("_", " ")));
                }

                sb.AppendLine("GO");
            }

            #endregion

        }
    }


}