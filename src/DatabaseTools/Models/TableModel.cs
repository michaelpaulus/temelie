
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
        public class TableModel : DatabaseObjectModel
        {

            public TableModel()
            {
                this.Selected = true;
            }

            #region Properties

           public bool Selected { get; set; }
           
            public string TableName { get; set; }
            
            public int ProgressPercentage { get; set; }
           
            public string ErrorMessage { get; set; }

            public int TemporalType { get; set; }
            public string HistoryTableName { get; set; }

            public bool IsHistoryTable { get { return TemporalType == 1; } }

            private IList<ColumnModel> _columns;
            public IList<ColumnModel> Columns
            {
                get
                {
                    if (this._columns == null)
                    {
                        this._columns = new List<ColumnModel>();
                    }
                    return this._columns;
                }
            }

            #endregion

            #region Methods

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                sb.AppendLine($"\tDROP TABLE {quoteCharacterStart}dbo{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                sb.AppendLine("GO");
            }

            public void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd, bool includeIfNotExists)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.AppendLine(string.Format("-- {0}", this.TableName));

                if (this.TableName.StartsWith("default_", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (includeIfNotExists)
                    {
                        sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                    }
                    sb.AppendLine($"\tDROP TABLE {quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                    sb.AppendLine("GO");
                }
                sb.AppendLine();

                if (includeIfNotExists)
                {
                    sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                }
                sb.AppendLine($"\tCREATE TABLE {quoteCharacterStart}dbo{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                sb.AppendLine("\t" + "(");

                int intColumnCount = 0;

                foreach (Models.ColumnModel column in (
                    from i in this.Columns
                    orderby i.ColumnID
                    select i))
                {
                    if (intColumnCount != 0)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append("\t" + "\t" + column.ToString(quoteCharacterStart, quoteCharacterEnd ));

                    intColumnCount += 1;
                }

                if (TemporalType == 2)
                {
                    if (intColumnCount != 0)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append($"\t\tPERIOD FOR SYSTEM_TIME ({Columns.Where(c => c.GeneratedAlwaysType == 1).First().ColumnName}, {Columns.Where(c => c.GeneratedAlwaysType == 2).First().ColumnName})");
                }

                sb.AppendLine();
                sb.Append("\t" + ")");

                sb.AppendLine();

                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                this.AppendCreateScript(sb, quoteCharacterStart, quoteCharacterEnd, true);
            }

            #endregion

        }
    }

}