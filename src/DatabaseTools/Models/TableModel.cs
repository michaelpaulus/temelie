
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

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacter)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                sb.AppendLine($"\tDROP TABLE {quoteCharacter}dbo{quoteCharacter}.{quoteCharacter}{this.TableName}{quoteCharacter}");
                sb.AppendLine("GO");
            }

            public void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacter, bool includeIfNotExists)
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
                    sb.AppendLine("\t" + string.Format("DROP TABLE {1}{0}{1}", this.TableName, quoteCharacter));
                    sb.AppendLine("GO");
                }
                sb.AppendLine();

                if (includeIfNotExists)
                {
                    sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                }
                sb.AppendLine("\t" + string.Format("CREATE TABLE {1}dbo{1}.{1}{0}{1}", this.TableName, quoteCharacter));
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

                    sb.Append("\t" + "\t" + column.ToString(quoteCharacter));

                    intColumnCount += 1;
                }

                sb.AppendLine();
                sb.AppendLine("\t" + ")");
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacter)
            {
                this.AppendCreateScript(sb, quoteCharacter, true);
            }

            #endregion

        }
    }

}