
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
            public bool IsNotForReplication { get; set; }
            public string DeleteAction { get; set; }
            public string UpdateAction { get; set; }
            public string ReferencedTableName { get; set; }

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

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacter)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine($"\tALTER TABLE {this.TableName} DROP CONSTRAINT {this.ForeignKeyName}");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacter)
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

                    strColumnNames += $"{quoteCharacter}{strColumnName}{quoteCharacter}";

                    if (strReferencedColumnNames.Length > 0)
                    {
                        strReferencedColumnNames += ",";
                    }

                    strReferencedColumnNames += $"{quoteCharacter}{strReferencedColumnName}{quoteCharacter}"; ;
                }

                sb.AppendLine();

                sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine($"\tALTER TABLE {quoteCharacter}{this.TableName}{quoteCharacter} WITH CHECK ADD CONSTRAINT {quoteCharacter}{this.ForeignKeyName}{quoteCharacter} FOREIGN KEY ({strColumnNames})");
                sb.AppendLine($"\tREFERENCES {quoteCharacter}{this.ReferencedTableName}{quoteCharacter} ({strReferencedColumnNames})");

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