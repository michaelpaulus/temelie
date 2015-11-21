
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
            #region Properties

            private bool _selected = true;
            public bool Selected
            {
                get
                {
                    return _selected;
                }
                set
                {
                    if (this._selected != value)
                    {
                        _selected = value;
                        this.OnPropertyChanged("Selected");
                    }
                }
            }

            private string _tableName;
            public string TableName
            {
                get
                {
                    return _tableName;
                }
                set
                {
                    if (this._tableName != value)
                    {
                        _tableName = value;
                        this.OnPropertyChanged("TableName");
                    }
                }
            }

            private int _progressPercentage;
            public int ProgressPercentage
            {
                get
                {
                    return this._progressPercentage;
                }
                set
                {
                    if (this._progressPercentage != value)
                    {
                        _progressPercentage = value;
                        this.OnPropertyChanged("ProgressPercentage");
                    }
                }
            }

            private string _errorMessage;
            public string ErrorMessage
            {
                get
                {
                    return _errorMessage;
                }
                set
                {
                    if (this._errorMessage != value)
                    {
                        _errorMessage = value;
                        this.OnPropertyChanged("ErrorMessage");
                    }
                }
            }

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

            public override void AppendDropScript(System.Text.StringBuilder sb)
            {
                sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{0}')", this.TableName));
                sb.AppendLine("\t" + string.Format("DROP TABLE dbo.{0}", this.TableName));
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

            public override void AppendCreateScript(System.Text.StringBuilder sb)
            {
                this.AppendCreateScript(sb, "", true);
            }

            public void Initialize(DataRow row)
            {
                this.TableName = this.GetStringValue(row, "table_name");
            }

            public void Initialize(IList<Models.ColumnModel> columns)
            {
                foreach (var column in (
                    from i in columns
                    where i.TableName.EqualsIgnoreCase(this.TableName)
                    select i))
                {
                    this.Columns.Add(column);
                }
            }

            public void Initialize(DataRow row, IList<Models.ColumnModel> columns)
            {
                this.Initialize(row);
                this.Initialize(columns);
            }

            #endregion

        }
    }

}