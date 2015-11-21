
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
        public class IndexModel : DatabaseObjectModel
        {
            #region Properties

            public string TableName { get; set; }
            public string IndexName { get; set; }
            public string IndexType { get; set; }
            public bool IsUnique { get; set; }
            public int FillFactor { get; set; }

            public bool IsPrimaryKey { get; set; }

            private IList<string> _columns;
            public IList<string> Columns
            {
                get
                {
                    if (this._columns == null)
                    {
                        this._columns = new List<string>();
                    }
                    return this._columns;
                }
            }

            private IList<string> _includeColumns;
            public IList<string> IncludeColumns
            {
                get
                {
                    if (this._includeColumns == null)
                    {
                        this._includeColumns = new List<string>();
                    }
                    return this._includeColumns;
                }
            }


            #endregion

            #region Methods

            public override void AppendDropScript(System.Text.StringBuilder sb)
            {
                if (this.IsPrimaryKey)
                {
                    sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.IndexName));
                    sb.AppendLine("\t" + string.Format("ALTER TABLE dbo.{0} DROP CONSTRAINT {1}", this.TableName, this.IndexName));
                }
                else
                {
                    sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{0}')", this.IndexName));
                    sb.AppendLine("\t" + string.Format("DROP INDEX {1} ON dbo.{0}", this.TableName, this.IndexName));
                }
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                if (this.IsPrimaryKey)
                {
                    sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.IndexName));
                    sb.AppendLine("\t" + string.Format("ALTER TABLE dbo.{0} ADD CONSTRAINT {1} PRIMARY KEY {2}", this.TableName, this.IndexName, this.IndexType));
                    sb.AppendLine("\t" + "(");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append("\t" + "\t" + column);

                        intColumnCount += 1;
                    }
                    sb.AppendLine();
                    sb.AppendLine("\t" + ")");
                    sb.AppendLine("GO");
                }
                else
                {
                    string strIndexType = this.IndexType;

                    if (this.IsUnique)
                    {
                        strIndexType = "UNIQUE " + this.IndexType;
                    }

                    string strRelationalIndexOptions = string.Empty;
                    if (this.FillFactor != 0)
                    {
                        strRelationalIndexOptions = string.Format(" WITH (FILLFACTOR = {0})", this.FillFactor);
                    }

                    sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{0}')", this.IndexName));
                    sb.AppendLine("\t" + string.Format("CREATE {0} INDEX {1} ON dbo.{2}{3}", strIndexType, this.IndexName, this.TableName, strRelationalIndexOptions));
                    sb.AppendLine("\t" + "(");

                    bool blnHasColumns = false;

                    foreach (var column in this.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append("\t" + "\t" + column);
                        blnHasColumns = true;
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t" + ")");

                    if (this.IncludeColumns.Count > 0)
                    {
                        sb.Append("\t" + "INCLUDE (");

                        bool blnHasIncludeColumns = false;

                        foreach (var includeColumn in this.IncludeColumns)
                        {
                            if (blnHasIncludeColumns)
                            {
                                sb.Append(", ");
                            }
                            sb.Append(includeColumn);
                            blnHasIncludeColumns = true;
                        }

                        sb.AppendLine(")");
                    }

                    sb.AppendLine("GO");
                }
            }

            #endregion

        }
    }


}