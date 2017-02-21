
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            public string SchemaName { get; set; }

            public int ProgressPercentage { get; set; }
           
            public string ErrorMessage { get; set; }

            public int TemporalType { get; set; }
            public string HistoryTableName { get; set; }
            public bool IsMemoryOptimized { get; set; }
            public string DurabilityDesc { get; set; }

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

            private IList<IndexModel> _indexes; 
            public IList<IndexModel> Indexes
            {
                get
                {
                    if (this._indexes == null)
                    {
                        this._indexes = new List<IndexModel>();
                    }
                    return this._indexes;
                }
            }

            #endregion

            #region Methods

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.schemas ON sys.tables.schema_id = sys.schemas.schema_id WHERE sys.tables.name = '{this.TableName}' AND sys.schemas.name = '{this.SchemaName}')");
                sb.AppendLine($"\tDROP TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
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
                        sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.schemas ON sys.tables.schema_id = sys.schemas.schema_id WHERE sys.tables.name = '{this.TableName}' AND sys.schemas.name = '{this.SchemaName}')");
                    }
                    sb.AppendLine($"\tDROP TABLE {quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                    sb.AppendLine("GO");
                }
                sb.AppendLine();

                if (includeIfNotExists)
                {
                    sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.schemas ON sys.tables.schema_id = sys.schemas.schema_id WHERE sys.tables.name = '{this.TableName}' AND sys.schemas.name = '{this.SchemaName}')");
                }
                sb.AppendLine($"\tCREATE TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
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

                if (Columns.Where(c => c.GeneratedAlwaysType == 1).Any() &&
                    Columns.Where(c => c.GeneratedAlwaysType == 2).Any())
                {
                    sb.AppendLine(",");
                    sb.Append($"\t\tPERIOD FOR SYSTEM_TIME ({Columns.Where(c => c.GeneratedAlwaysType == 1).First().ColumnName}, {Columns.Where(c => c.GeneratedAlwaysType == 2).First().ColumnName})");
                }

                if (IsMemoryOptimized)
                {
                    var pkIndex = (from i in Indexes where i.IsPrimaryKey select i).FirstOrDefault();
                    if (pkIndex != null)
                    {
                        sb.AppendLine(",");
                        pkIndex.AppendTableInlineCreateScript(sb, quoteCharacterStart, quoteCharacterEnd);
                    }
                }
                else
                {
                    sb.AppendLine();
                }

                sb.Append("\t" + ")");

                sb.AppendLine();

                AddOptions(sb);

                sb.AppendLine("GO");
            }

            private void AddOptions(StringBuilder sb)
            {
                if (IsMemoryOptimized)
                {
                    var sbOptions = new StringBuilder();
                    sbOptions.Append($"MEMORY_OPTIMIZED = ON, DURABILITY = {DurabilityDesc}");
                    sb.AppendLine($"\tWITH ({sbOptions.ToString()})");
                }
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                this.AppendCreateScript(sb, quoteCharacterStart, quoteCharacterEnd, true);
            }

            #endregion

        }
    }

}