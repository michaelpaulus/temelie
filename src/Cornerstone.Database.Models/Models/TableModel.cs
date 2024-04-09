using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cornerstone.Database
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

            [JsonIgnore]
            public bool Selected { get; set; }

            public string TableName { get; set; }
            public string SchemaName { get; set; }

            [JsonIgnore]
            public int ProgressPercentage { get; set; }

            [JsonIgnore]
            public string ErrorMessage { get; set; }

            public int TemporalType { get; set; }
            public string HistoryTableName { get; set; }
            public bool IsMemoryOptimized { get; set; }
            public string DurabilityDesc { get; set; }
            public bool IsExternal { get; set; }
            public string DataSourceName { get; set; }
            public bool IsHistoryTable { get { return TemporalType == 1; } }
            public bool IsView { get; set; }

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

            public Dictionary<string, string> ExtendedProperties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            public string PartitionSchemeName { get; set; }
            public string PartitionSchemeColumns { get; set; }

            public string Options { get; set; }

            public IndexModel PrimaryKey { get; set; }

            #endregion

            #region Methods

            public override void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
                sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{this.TableName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                sb.AppendLine($"    DROP{(IsExternal ? " EXTERNAL " : " ")}TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
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
                        sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM 
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{this.TableName}' AND
            schemas.name = '{this.SchemaName}'
    )");
                    }
                    sb.AppendLine($"    DROP{(IsExternal ? " EXTERNAL " : " ")}TABLE {quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                    sb.AppendLine("GO");
                }
                sb.AppendLine();

                if (includeIfNotExists)
                {
                    sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{this.TableName}' AND
            schemas.name = '{SchemaName}'
    )");
                }
                sb.AppendLine($"    CREATE{(IsExternal ? " EXTERNAL " : " ")}TABLE {quoteCharacterStart}{SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd}");
                sb.AppendLine("    " + "(");

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

                    sb.Append("    " + "    " + column.ToString(quoteCharacterStart, quoteCharacterEnd));

                    intColumnCount += 1;
                }

                if (Columns.Where(c => c.GeneratedAlwaysType == 1).Any() &&
                    Columns.Where(c => c.GeneratedAlwaysType == 2).Any())
                {
                    sb.AppendLine(",");
                    sb.Append($"        PERIOD FOR SYSTEM_TIME ({Columns.Where(c => c.GeneratedAlwaysType == 1).First().ColumnName}, {Columns.Where(c => c.GeneratedAlwaysType == 2).First().ColumnName})");
                }

                if (IsMemoryOptimized)
                {
                    if (PrimaryKey != null)
                    {
                        sb.AppendLine(",");
                        PrimaryKey.AppendTableInlineCreateScript(sb, quoteCharacterStart, quoteCharacterEnd);
                    }
                }
                else
                {
                    sb.AppendLine();
                }

                sb.AppendLine("    )");

                AddOptions(sb);

                if (!string.IsNullOrEmpty(PartitionSchemeName))
                {
                    sb.AppendLine($"    ON {PartitionSchemeName} ({PartitionSchemeColumns})");
                }

                sb.AppendLine("GO");

                AddExtendedProperties(this, sb);

            }

            public void AppendJsonScript(System.Text.StringBuilder sb)
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                });
                json = json.Replace("\\u0027", "'").Replace("\\u003C", "<").Replace("\\u003E", ">").Replace("\\u002B", "+");
                sb.Append(json);
            }

            public static void AddExtendedProperties(TableModel table, StringBuilder sb)
            {
                string type = table.IsView ? "view" : "table";

                foreach (var prop in table.ExtendedProperties)
                {
                    sb.AppendLine($@"
IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('{prop.Key}', 'schema', '{table.SchemaName}', '{type}', '{table.TableName}', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'{prop.Key}',
                                     @level0type = N'schema',
                                     @level0name = '{table.SchemaName}',
                                     @level1type = N'{type}',
                                     @level1name = '{table.TableName}';
END
GO

EXEC sys.sp_addextendedproperty @name = N'{prop.Key}',
                                @value = N'{prop.Value}',
                                @level0type = N'schema',
                                @level0name = '{table.SchemaName}',
                                @level1type = N'{type}',
                                @level1name = '{table.TableName}';
GO
");
                }

                foreach (var column in table.Columns)
                {
                    foreach (var prop in column.ExtendedProperties)
                    {
                        sb.AppendLine($@"
IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('{prop.Key}', 'schema', '{table.SchemaName}', '{type}', '{table.TableName}', 'column', '{column.ColumnName}')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'{prop.Key}',
                                     @level0type = N'schema',
                                     @level0name = '{table.SchemaName}',
                                     @level1type = N'{type}',
                                     @level1name = '{table.TableName}',
                                     @level2type = N'column',
                                     @level2name = '{column.ColumnName}';
END
GO

EXEC sys.sp_addextendedproperty @name = N'{prop.Key}',
                                @value = N'{prop.Value}',
                                @level0type = N'schema',
                                @level0name = '{table.SchemaName}',
                                @level1type = N'{type}',
                                @level1name = '{table.TableName}',
                                @level2type = N'column',
                                @level2name = '{column.ColumnName}';
GO
");
                    }
                }

            }

            private void AddOptions(StringBuilder sb)
            {
                var options = Options;
                if (IsMemoryOptimized)
                {
                    if (!string.IsNullOrEmpty(options))
                    {
                        options += ", ";
                    }
                    options += $"MEMORY_OPTIMIZED = ON, DURABILITY = {DurabilityDesc}";
                }
                if (!string.IsNullOrEmpty(DataSourceName))
                {
                    if (!string.IsNullOrEmpty(options))
                    {
                        options += ", ";
                    }
                    options += $"DATA_SOURCE = {DataSourceName}";
                }
                if (!string.IsNullOrEmpty(options))
                {
                    sb.AppendLine($"    WITH ({options})");
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
