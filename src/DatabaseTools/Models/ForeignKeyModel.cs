
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

            public override void AppendDropScript(System.Text.StringBuilder sb)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine("\t" + string.Format("ALTER TABLE {0} DROP CONSTRAINT {1}", this.TableName, this.ForeignKeyName));
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb)
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

                    strColumnNames += strColumnName;

                    if (strReferencedColumnNames.Length > 0)
                    {
                        strReferencedColumnNames += ",";
                    }

                    strReferencedColumnNames += strReferencedColumnName;
                }

                sb.AppendLine();

                sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.ForeignKeyName));
                sb.AppendLine("\t" + string.Format("ALTER TABLE {0} WITH CHECK ADD CONSTRAINT {1} FOREIGN KEY ({2})", this.TableName, this.ForeignKeyName, strColumnNames));
                sb.AppendLine("\t" + string.Format("REFERENCES {0} ({1})", this.ReferencedTableName, strReferencedColumnNames));

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