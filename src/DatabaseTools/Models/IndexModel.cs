
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
            public string SchemaName { get; set; }
            public string IndexName { get; set; }
            public string IndexType { get; set; }
            public string FilterDefinition { get; set; }
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

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (this.IsPrimaryKey)
                {
                    sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{this.IndexName}')");
                    sb.AppendLine($"\tALTER TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} DROP CONSTRAINT {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd}");
                }
                else
                {
                    sb.AppendLine($"IF EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{this.IndexName}')");
                    sb.AppendLine($"\tDROP INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} ON {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                }
                sb.AppendLine("GO");
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                if (this.IsPrimaryKey)
                {
                    sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{this.IndexName}') AND EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{this.TableName}')");
                    sb.AppendLine("\t" + $"ALTER TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} ADD CONSTRAINT {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} PRIMARY KEY { this.IndexType}");
                    sb.AppendLine("\t" + "(");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append($"\t\t{quoteCharacterStart}{column}{quoteCharacterEnd}");

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

                    

                    sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE sys.indexes.name = '{this.IndexName}') AND EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = '{this.TableName}')");
                    sb.AppendLine("\t" + $"CREATE {strIndexType} INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} ON {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                    sb.AppendLine("\t" + "(");

                    bool blnHasColumns = false;

                    foreach (var column in this.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append($"\t\t{quoteCharacterStart}{column}{quoteCharacterEnd}");
                        blnHasColumns = true;
                    }

                    sb.AppendLine();
                    sb.AppendLine("\t" + ")");

                    if (this.IncludeColumns.Count > 0)
                    {
                        sb.Append("\tINCLUDE (");

                        bool blnHasIncludeColumns = false;

                        foreach (var includeColumn in this.IncludeColumns)
                        {
                            if (blnHasIncludeColumns)
                            {
                                sb.Append(", ");
                            }
                            sb.Append($"{quoteCharacterStart}{includeColumn}{quoteCharacterEnd}");
                            blnHasIncludeColumns = true;
                        }

                        sb.AppendLine(")");
                    }

                    if (!string.IsNullOrEmpty(this.FilterDefinition))
                    {
                        sb.AppendLine($"\tWHERE {this.FilterDefinition}");
                    }

                    if (this.FillFactor != 0)
                    {
                        sb.AppendLine($"\tWITH (FILLFACTOR = {this.FillFactor})");
                    }

                    sb.AppendLine("GO");
                }
            }
      

            #endregion

        }
    }


}