using System.Text;

namespace Cornerstone.Database
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
            public string PartitionSchemeName { get; set; }
            public string DataCompressionDesc { get; set; }
            public bool IsUnique { get; set; }
            public int FillFactor { get; set; }

            public bool IsPrimaryKey { get; set; }
            public int TotalBucketCount { get; set; }

            private IList<IndexColumnModel> _columns;
            public IList<IndexColumnModel> Columns
            {
                get
                {
                    if (this._columns == null)
                    {
                        this._columns = new List<IndexColumnModel>();
                    }
                    return this._columns;
                }
            }

            private IList<IndexColumnModel> _includeColumns;
            public IList<IndexColumnModel> IncludeColumns
            {
                get
                {
                    if (this._includeColumns == null)
                    {
                        this._includeColumns = new List<IndexColumnModel>();
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
                    sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{this.IndexName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                    sb.AppendLine($"    ALTER TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} DROP CONSTRAINT {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd}");
                }
                else
                {
                    sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{this.IndexName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                    sb.AppendLine($"    DROP INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} ON {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                }
                sb.AppendLine("GO");
            }

            public void AppendTableInlineCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (IsPrimaryKey)
                {
                    sb.AppendLine($"        CONSTRAINT {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} {(IsPrimaryKey ? "PRIMARY KEY" : "")} {this.IndexType}");
                    sb.AppendLine("        (");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append($"            {quoteCharacterStart}{column.ColumnName}{quoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                        intColumnCount += 1;
                    }
                    sb.AppendLine();
                    sb.AppendLine("        )");

                    AddOptions(sb, 2);
                }
                else
                {
                    string indexType = this.IndexType;

                    if (this.IsUnique)
                    {
                        indexType = "UNIQUE " + this.IndexType;
                    }

                    sb.AppendLine($"        INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} {indexType}");

                    if (Columns.Any())
                    {
                        sb.AppendLine("        (");

                        bool blnHasColumns = false;

                        foreach (var column in this.Columns)
                        {
                            if (blnHasColumns)
                            {
                                sb.AppendLine(",");
                            }
                            sb.Append($"            {quoteCharacterStart}{column.ColumnName}{quoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                            blnHasColumns = true;
                        }

                        sb.AppendLine();
                        sb.AppendLine("        )");

                        if (IncludeColumns.Any())
                        {
                            sb.Append("        INCLUDE (");

                            bool blnHasIncludeColumns = false;

                            foreach (var includeColumn in IncludeColumns)
                            {
                                if (blnHasIncludeColumns)
                                {
                                    sb.Append(", ");
                                }
                                sb.Append($"{quoteCharacterStart}{includeColumn.ColumnName}{quoteCharacterEnd}");
                                blnHasIncludeColumns = true;
                            }

                            sb.AppendLine("        )");
                        }
                    }

                    if (!string.IsNullOrEmpty(this.FilterDefinition))
                    {
                        sb.AppendLine($"        WHERE {this.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
                    }

                    AddOptions(sb, 2);
                }
            }

            public override void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                if (IndexType.Contains("HASH"))
                {
                    sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{this.IndexName}' AND
            schemas.name = '{SchemaName}'
    )");
                    sb.AppendLine("    " + $"ALTER TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} ADD INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} {this.IndexType}");
                    sb.AppendLine("    " + "(");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append($"        {quoteCharacterStart}{column.ColumnName}{quoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                        intColumnCount += 1;
                    }
                    sb.AppendLine();
                    sb.AppendLine("    " + ")");

                    AddOptions(sb);

                    sb.AppendLine("GO");
                }
                else if (this.IsPrimaryKey)
                {
                    sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{this.IndexName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                    sb.AppendLine("    " + $"ALTER TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} ADD CONSTRAINT {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} PRIMARY KEY {this.IndexType}");
                    sb.AppendLine("    " + "(");

                    int intColumnCount = 0;

                    foreach (var column in this.Columns)
                    {
                        if (intColumnCount > 0)
                        {
                            sb.AppendLine(",");
                        }

                        sb.Append($"    {quoteCharacterStart}{column.ColumnName}{quoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                        intColumnCount += 1;
                    }
                    sb.AppendLine();
                    sb.AppendLine("    " + ")");

                    AddOptions(sb);

                    sb.AppendLine("GO");
                }
                else
                {
                    string indexType = this.IndexType;

                    if (this.IsUnique)
                    {
                        indexType = "UNIQUE " + this.IndexType;
                    }

                    sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{this.IndexName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                    sb.AppendLine("    " + $"CREATE {indexType} INDEX {quoteCharacterStart}{this.IndexName}{quoteCharacterEnd} ON {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");

                    if (Columns.Any())
                    {
                        sb.AppendLine("    " + "(");

                        bool blnHasColumns = false;

                        foreach (var column in this.Columns)
                        {
                            if (blnHasColumns)
                            {
                                sb.AppendLine(",");
                            }
                            sb.Append($"    {quoteCharacterStart}{column.ColumnName}{quoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                            blnHasColumns = true;
                        }

                        sb.AppendLine();
                        sb.AppendLine("    " + ")");

                        if (IncludeColumns.Any())
                        {
                            sb.Append("    INCLUDE (");

                            bool blnHasIncludeColumns = false;

                            foreach (var includeColumn in IncludeColumns)
                            {
                                if (blnHasIncludeColumns)
                                {
                                    sb.Append(", ");
                                }
                                sb.Append($"{quoteCharacterStart}{includeColumn.ColumnName}{quoteCharacterEnd}");
                                blnHasIncludeColumns = true;
                            }

                            sb.AppendLine(")");
                        }
                    }

                    if (!string.IsNullOrEmpty(this.FilterDefinition))
                    {
                        sb.AppendLine($"    WHERE {this.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
                    }

                    AddOptions(sb);

                    sb.AppendLine("GO");
                }
            }

            private void AddOptions(StringBuilder sb, int indentCount = 1)
            {
                if (this.FillFactor != 0 || this.TotalBucketCount != 0 || !string.IsNullOrEmpty(DataCompressionDesc))
                {
                    var sbOptions = new StringBuilder();
                    if (FillFactor != 0)
                    {
                        sbOptions.Append($"FILLFACTOR = {this.FillFactor}");
                    }
                    if (TotalBucketCount != 0)
                    {
                        if (sbOptions.Length > 0)
                        {
                            sbOptions.Append(", ");
                        }
                        sbOptions.Append($"BUCKET_COUNT = {this.TotalBucketCount}");
                    }
                    if (!string.IsNullOrEmpty(DataCompressionDesc))
                    {
                        if (sbOptions.Length > 0)
                        {
                            sbOptions.Append(", ");
                        }
                        sbOptions.Append($"DATA_COMPRESSION = {DataCompressionDesc}");
                    }
                    sb.AppendLine($"{new string(' ', indentCount * 4)}WITH ({sbOptions.ToString()})");
                }

                if (!string.IsNullOrEmpty(PartitionSchemeName))
                {
                    var partitionColumns = Columns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
                    if (!partitionColumns.Any())
                    {
                        partitionColumns = IncludeColumns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
                    }
                    if (partitionColumns.Any())
                    {
                        sb.AppendLine($"{new string(' ', indentCount * 4)}ON {PartitionSchemeName} ({string.Join(", ", partitionColumns)})");
                    }
                    else
                    {
                        sb.AppendLine($"{new string(' ', indentCount * 4)}ON {PartitionSchemeName}");
                    }
                }

            }

            #endregion

        }
    }

}
