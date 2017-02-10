
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

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine($"\tALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} DROP CONSTRAINT {quoteCharacterStart}{this.ForeignKeyName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
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

                sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine($"\tALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} WITH CHECK ADD CONSTRAINT {quoteCharacterStart}{this.ForeignKeyName}{quoteCharacterEnd} FOREIGN KEY ({strColumnNames})");
                sb.AppendLine($"\tREFERENCES {quoteCharacterStart}{this.ReferencedSchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.ReferencedTableName}{quoteCharacterEnd} ({strReferencedColumnNames})");

                if (this.UpdateAction != "NO ACTION")
                {
                    sb.AppendLine("\t" + string.Format("ON UPDATE {0}", this.UpdateAction));
                }

                if (this.DeleteAction != "NO ACTION")
                {
                    sb.AppendLine("\t" + string.Format("ON DELETE {0}", this.DeleteAction));
                }

                sb.AppendLine("GO");
            }

            #endregion

        }
    }


}