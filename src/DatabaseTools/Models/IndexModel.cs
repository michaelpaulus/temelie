
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

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacter)
            {
                if (this.IsPrimaryKey)
                {
                    sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.IndexName));
                    sb.AppendLine($"\tALTER TABLE {quoteCharacter}dbo{quoteCharacter}.{quoteCharacter}{this.TableName}{quoteCharacter} DROP CONSTRAINT {quoteCharacter}{this.IndexName}{quoteCharacter}");
                }
                else
                {
                    sb.AppendLine(string.Format("IF EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{0}')", this.IndexName));
                    sb.AppendLine($"\tDROP INDEX {quoteCharacter}{this.IndexName}{quoteCharacter} ON {quoteCharacter}dbo{quoteCharacter}.{quoteCharacter}{this.TableName}{quoteCharacter}");
                }
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacter)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                if (this.IsPrimaryKey)
                {
                    sb.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM sysobjects WHERE sysobjects.name = '{0}')", this.IndexName));
                    sb.AppendLine("\t" + $"ALTER TABLE {quoteCharacter}dbo{quoteCharacter}.{quoteCharacter}{this.TableName}{quoteCharacter} ADD CONSTRAINT {quoteCharacter}{this.IndexName}{quoteCharacter} PRIMARY KEY { this.IndexType}");
                    sb.AppendLine("\t" + "(");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append($"\t\t{quoteCharacter}{column}{quoteCharacter}");

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
                    sb.AppendLine("\t" + $"CREATE {strIndexType} INDEX {quoteCharacter}{this.IndexName}{quoteCharacter} ON {quoteCharacter}dbo{quoteCharacter}.{quoteCharacter}{this.TableName}{quoteCharacter}{strRelationalIndexOptions}");
                    sb.AppendLine("\t" + "(");

                    bool blnHasColumns = false;

                    foreach (var column in this.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append($"\t\t{quoteCharacter}{column}{quoteCharacter}");
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
                            sb.Append($"{quoteCharacter}{includeColumn}{quoteCharacter}");
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