using System.Data;
using System.Data.Common;
using System.Text.Json;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Services;

namespace Temelie.Database.Providers;

public abstract class DatabaseProviderBase : IDatabaseProvider
{
    private readonly IEnumerable<IDatabaseModelProvider> _databaseModelProviders;

    public DatabaseProviderBase(IEnumerable<IDatabaseModelProvider> databaseModelProviders)
    {
        _databaseModelProviders = databaseModelProviders;
    }

    public abstract string Name { get; }
    public abstract string QuoteCharacterStart { get; }
    public abstract string QuoteCharacterEnd { get; }
    public abstract string DefaultConnectionString { get; }

    #region Helper Methods

    private IEnumerable<ColumnModel> GetColumns(DataTable dataTable)
    {
        IList<ColumnModel> list = new List<ColumnModel>();
        foreach (DataRow row in dataTable.Rows)
        {
            var column = new ColumnModel();
            InitializeColumn(column, row);
            list.Add(column);
        }
        //the database sometimes skips column numbers, we don't really care about that here,
        //  we need reproducable numbers over all db's in order
        foreach (var tableGroup in list.GroupBy(i => new { i.SchemaName, i.TableName }))
        {
            var index = 1;
            foreach (var column in tableGroup.OrderBy(i => i.ColumnId).ToList())
            {
                column.ColumnId = index;
                index += 1;
            }
        }

        return list;
    }

    private void InitializeColumn(ColumnModel column, DataRow row)
    {

        column.TableName = GetStringValue(row, "table_name");
        column.SchemaName = GetStringValue(row, "schema_name");
        column.ColumnName = GetStringValue(row, "column_name");
        column.Precision = GetInt32Value(row, "precision");
        column.Scale = GetInt32Value(row, "scale");
        column.ColumnType = GetStringValue(row, "column_type");
        column.IsNullable = GetBoolValue(row, "is_nullable");
        column.IsIdentity = GetBoolValue(row, "is_identity");
        column.IsComputed = GetBoolValue(row, "is_computed");
        column.IsHidden = GetBoolValue(row, "is_hidden");
        column.GeneratedAlwaysType = GetInt32Value(row, "generated_always_type");
        column.ComputedDefinition = GetStringValue(row, "computed_definition");
        column.ColumnId = GetInt32Value(row, "column_id");
        column.IsPrimaryKey = GetBoolValue(row, "is_primary_key");
        column.ColumnDefault = GetStringValue(row, "column_default");
        column.CollationName = GetStringValue(row, "COLLATION_NAME");
        column.CharacterSetName = GetStringValue(row, "CHARACTER_SET_NAME");

        var extendedProperites = GetStringValue(row, "extended_properties");
        if (!string.IsNullOrEmpty(extendedProperites))
        {
            foreach (var item in JsonSerializer.Deserialize<IEnumerable<ExtendedProperty>>(extendedProperites, ModelsJsonSerializerOptions.Default))
            {
                column.ExtendedProperties.Add(item.Name.ToLower(), item.Value);
            }
        }

        var targetColumnType = GetColumnType(new ColumnTypeModel() { ColumnType = column.ColumnType, Precision = column.Precision, Scale = column.Scale });
        if (targetColumnType != null)
        {
            column.ColumnType = targetColumnType.ColumnType;
            column.Precision = targetColumnType.Precision.GetValueOrDefault();
            column.Scale = targetColumnType.Scale.GetValueOrDefault();
        }

        foreach (var databaseModelProvider in _databaseModelProviders)
        {
            databaseModelProvider.Initialize(column);
        }

    }

    protected static string GetStringValue(DataRow row, string columnName)
    {
        string value = null;

        try
        {
            if (row.Table.Columns.Contains(columnName) && !row.IsNull(columnName))
            {
                value = Convert.ToString(row[columnName]);
            }
        }
        catch
        {

        }

        return value;
    }

    protected static int GetInt32Value(DataRow row, string columnName)
    {
        var value = 0;

        try
        {
            var strValue = GetStringValue(row, columnName);
            if (!string.IsNullOrEmpty(strValue))
            {
                int.TryParse(strValue, out value);
            }
        }
        catch
        {

        }

        return value;
    }

    protected static int? GetNullableInt32Value(DataRow row, string columnName)
    {
        int? value = null;

        try
        {
            var strValue = GetStringValue(row, columnName);
            if (!string.IsNullOrEmpty(strValue))
            {
                if (int.TryParse(strValue, out var value2))
                {
                    value = value2;
                }
            }
        }
        catch
        {

        }

        return value;
    }

    protected static bool GetBoolValue(DataRow row, string columnName)
    {
        var value = false;

        try
        {
            var strValue = GetStringValue(row, columnName);

            if (!string.IsNullOrEmpty(strValue))
            {
                if (strValue.EqualsIgnoreCase("Yes") || strValue.EqualsIgnoreCase("1"))
                {
                    strValue = "True";
                }
                else if (strValue.EqualsIgnoreCase("No") || strValue.EqualsIgnoreCase("0"))
                {
                    strValue = "False";
                }
                bool.TryParse(strValue, out value);
            }
        }
        catch
        {

        }

        return value;
    }

    #endregion

    #region Database Structure

    public IEnumerable<ColumnModel> GetTableColumns(DbConnection connection)
    {
        IEnumerable<ColumnModel> list = new List<ColumnModel>();
        var dataTable = GetTableColumnsDataTable(connection);
        if (dataTable != null)
        {
            list = GetColumns(dataTable);
        }

        return list;
    }

    public IEnumerable<ColumnModel> GetViewColumns(DbConnection connection)
    {
        IEnumerable<ColumnModel> list = new List<ColumnModel>();

        var dataTable = GetViewColumnsDataTable(connection);

        if (dataTable != null)
        {
            list = GetColumns(dataTable);
        }

        return list;
    }

    public IEnumerable<DefinitionModel> GetDefinitions(DbConnection connection)
    {
        var list = new List<DefinitionModel>();

        var dtDefinitions = GetDefinitionsDataTable(connection);

        if (dtDefinitions != null)
        {
            foreach (var row in dtDefinitions.Rows.OfType<DataRow>())
            {
                var model = new DefinitionModel
                {
                    Definition = GetStringValue(row, "definition").Replace("\t", "    ").RemoveLeadingAndTrailingLines(),
                    DefinitionName = GetStringValue(row, "name"),
                    SchemaName = GetStringValue(row, "schema_name"),
                    XType = GetStringValue(row, "xtype").Trim()
                };
                list.Add(model);

                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(model);
                }
            }

        }

        list = list.OrderBy(i => i.XType).ThenBy(i => i.DefinitionName).ToList();

        return list;
    }
    public IEnumerable<SecurityPolicyModel> GetSecurityPolicies(DbConnection connection)
    {
        var list = new List<SecurityPolicyModel>();

        var dtDefinitions = GetSecurityPoliciesDataTable(connection);

        var dtDependencies = GetDefinitionDependenciesDataTable(connection);

        if (dtDefinitions != null)
        {
            list = (
                from i in dtDefinitions.Rows.OfType<DataRow>()
                group new SecurityPolicyPredicate
                {
                    TargetSchema = GetStringValue(i, "TargetSchema"),
                    Operation = GetStringValue(i, "Operation"),
                    PredicateDefinition = GetStringValue(i, "PredicateDefinition"),
                    PredicateType = GetStringValue(i, "PredicateType"),
                    TargetName = GetStringValue(i, "TargetName")
                }
                by new
                {
                    PolicySchema = GetStringValue(i, "PolicySchema"),
                    PolicyName = GetStringValue(i, "PolicyName"),
                    IsEnabled = GetBoolValue(i, "IsEnabled"),
                    IsSchemaBound = GetBoolValue(i, "IsSchemaBound")
                } into g
                select new SecurityPolicyModel
                {
                    IsEnabled = g.Key.IsEnabled,
                    PolicyName = g.Key.PolicyName,
                    PolicySchema = g.Key.PolicySchema,
                    Predicates = g.ToList(),
                    IsSchemaBound = g.Key.IsSchemaBound
                }
                ).ToList();

            foreach (var item in list)
            {
                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(item);
                }
            }

        }
        return list;
    }

    public IEnumerable<ForeignKeyModel> GetForeignKeys(DbConnection connection)
    {
        IList<ForeignKeyModel> list = new List<ForeignKeyModel>();

        var dataTable = GetForeignKeysDataTable(connection);

        if (dataTable != null)
        {
            foreach (var tableGroup in
           from i in dataTable.Rows.Cast<DataRow>()
           group i by new
           {
               SchemaName = GetStringValue(i, "schema_name"),
               TableName = GetStringValue(i, "table_name"),
               ForeignKeyName = GetStringValue(i, "foreign_key_name")
           } into g
           select new
           {
               g.Key.TableName,
               g.Key.SchemaName,
               g.Key.ForeignKeyName,
               Items = g.ToList()
           })
            {

                var summaryRow = tableGroup.Items[0];

                var foreignKey = new ForeignKeyModel
                {
                    ForeignKeyName = tableGroup.ForeignKeyName,
                    TableName = tableGroup.TableName,
                    SchemaName = tableGroup.SchemaName,
                    ReferencedSchemaName = GetStringValue(summaryRow, "referenced_schema_name"),
                    ReferencedTableName = GetStringValue(summaryRow, "referenced_table_name"),
                    IsNotForReplication = GetBoolValue(summaryRow, "is_not_for_replication"),
                    DeleteAction = GetStringValue(summaryRow, "delete_action"),
                    UpdateAction = GetStringValue(summaryRow, "update_action")
                };

                list.Add(foreignKey);

                foreignKey.Detail = tableGroup.Items.Select(i => new ForeignKeyDetailModel
                {
                    Column = GetStringValue(i, "column_name"),
                    ReferencedColumn = GetStringValue(i, "referenced_column_name"),
                }).ToList();

                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(foreignKey);
                }

            }
        }

        return list;
    }

    public IEnumerable<CheckConstraintModel> GetCheckConstraints(DbConnection connection)
    {
        var list = new List<CheckConstraintModel>();

        var dataTable = GetCheckConstraintsDataTable(connection);

        if (dataTable != null)
        {
            foreach (DataRow detailRow in dataTable.Rows)
            {
                var strTableName = GetStringValue(detailRow, "table_name");
                var strSchemaName = GetStringValue(detailRow, "schema_name");
                var strConstraintName = GetStringValue(detailRow, "check_constraint_name");
                var strDefinition = GetStringValue(detailRow, "check_constraint_definition");

                var constraint = new CheckConstraintModel
                {
                    CheckConstraintName = strConstraintName,
                    TableName = strTableName,
                    SchemaName = strSchemaName,
                    CheckConstraintDefinition = strDefinition
                };

                list.Add(constraint);

                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(constraint);
                }

            }
        }

        return list;
    }

    public IEnumerable<IndexModel> GetIndexes(DbConnection connection)
    {
        IList<IndexModel> list = new List<IndexModel>();
        DataTable dtIndexes = null;
        dtIndexes = GetIndexesDataTable(connection);

        if (dtIndexes != null)
        {

            var indexBucketCounts = new List<IndexBucket>();

            var dtIndexBucketCounts = GetIndexeBucketCountsDataTable(connection);
            if (dtIndexBucketCounts != null)
            {
                indexBucketCounts = (from i in dtIndexBucketCounts.Rows.OfType<DataRow>()
                                     select new IndexBucket
                                     {
                                         TableName = GetStringValue(i, "table_name"),
                                         IndexName = GetStringValue(i, "index_name"),
                                         SchemaName = GetStringValue(i, "schema_name"),
                                         BucketCount = GetInt32Value(i, "total_bucket_count")
                                     }).ToList();
            }

            foreach (var indexGroup in
                from i in dtIndexes.Rows.Cast<DataRow>()
                group i by new
                {
                    IndexName = GetStringValue(i, "index_name"),
                    TableName = GetStringValue(i, "table_name"),
                    SchemaName = GetStringValue(i, "schema_name")
                } into g
                select new
                {
                    g.Key.IndexName,
                    g.Key.TableName,
                    g.Key.SchemaName,
                    Items = g.ToList()
                })
            {

                var summaryRow = indexGroup.Items[0];

                var index = new IndexModel
                {
                    TableName = indexGroup.TableName,
                    IndexName = indexGroup.IndexName,
                    SchemaName = indexGroup.SchemaName,
                    PartitionSchemeName = GetStringValue(summaryRow, "partition_scheme_name") ?? "",
                    DataCompressionDesc = GetStringValue(summaryRow, "data_compression_desc") ?? "",
                    CompressionDelay = GetNullableInt32Value(summaryRow, "compression_delay"),
                    IndexType = GetStringValue(summaryRow, "index_type"),
                    FilterDefinition = GetStringValue(summaryRow, "filter_definition") ?? "",
                    IsUnique = GetBoolValue(summaryRow, "is_unique"),
                    FillFactor = GetInt32Value(summaryRow, "fill_factor"),
                    IsPrimaryKey = GetBoolValue(summaryRow, "is_primary_key")
                };

                var indexBucketCount = (from i in indexBucketCounts
                                        where i.SchemaName == index.SchemaName &&
                                       i.TableName == index.TableName &&
                                       i.IndexName == index.IndexName
                                        select i).FirstOrDefault();

                if (indexBucketCount != null)
                {
                    index.TotalBucketCount = indexBucketCount.BucketCount;
                }

                foreach (var detialRow in indexGroup.Items.OrderBy(i => GetInt32Value(i, "key_ordinal")))
                {
                    var blnIsDescending = GetBoolValue(detialRow, "is_descending_key");
                    var blnIsIncludeColumn = GetBoolValue(detialRow, "is_included_column");
                    var strColumnName = GetStringValue(detialRow, "column_name");

                    var columnModel = new IndexColumnModel
                    {
                        ColumnName = strColumnName,
                        IsDescending = blnIsDescending,
                        PartitionOrdinal = GetInt32Value(detialRow, "partition_ordinal"),
                        SubPart = GetNullableInt32Value(detialRow, "sub_part")
                    };

                    if (blnIsIncludeColumn)
                    {
                        index.IncludeColumns.Add(columnModel);
                    }
                    else
                    {
                        index.Columns.Add(columnModel);
                    }
                }

                list.Add(index);

                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(index);
                }

            }
        }

        return list;
    }

    private IEnumerable<TableModel> GetTables(DataTable dataTable, IEnumerable<ColumnModel> columns, bool isView = false)
    {
        var list = new List<TableModel>();

        var tables = new List<string>();

        var columnIndex = new Dictionary<string, IList<ColumnModel>>();

        foreach (var columnGroup in
            from i in columns
            group i by new { i.TableName, i.SchemaName } into g
            select new { g.Key.TableName, g.Key.SchemaName, Items = g.ToList() })
        {
            columnIndex.Add($"{columnGroup.SchemaName}.{columnGroup.TableName}".ToUpper(), columnGroup.Items);
        }

        foreach (DataRow row in dataTable.Rows)
        {
            var table = new TableModel();
            table.TableName = GetStringValue(row, "table_name");
            table.SchemaName = GetStringValue(row, "schema_name");
            table.TemporalType = GetInt32Value(row, "temporal_type");
            table.HistoryTableName = GetStringValue(row, "history_table_name");
            table.IsMemoryOptimized = GetBoolValue(row, "is_memory_optimized");
            table.DurabilityDesc = GetStringValue(row, "durability_desc");
            table.IsExternal = GetBoolValue(row, "is_external");
            table.RemoteSchemaName = GetStringValue(row, "remote_schema_name");
            table.RemoteObjectName = GetStringValue(row, "remote_object_name");
            table.DataSourceName = GetStringValue(row, "data_source_name");
            table.PartitionSchemeColumns = GetStringValue(row, "partition_scheme_columns");
            table.PartitionSchemeName = GetStringValue(row, "partition_scheme_name");
            table.Engine = GetStringValue(row, "engine");
            table.CharacterSetName = GetStringValue(row, "character_set_name");
            table.CollationName = GetStringValue(row, "collation_name");
            table.IsView = isView;
            var extendedProperites = GetStringValue(row, "extended_properties");
            if (!string.IsNullOrEmpty(extendedProperites))
            {
                try
                {
                    var props = JsonSerializer.Deserialize<IEnumerable<ExtendedProperty>>(extendedProperites, ModelsJsonSerializerOptions.Default);
                    foreach (var item in props)
                    {
                        table.ExtendedProperties.Add(item.Name.ToLower(), item.Value);
                    }
                }
                catch
                {

                }
            }

            var tableKey = $"{table.SchemaName}.{table.TableName}".ToUpper();

            if (!tables.Contains(tableKey))
            {
                if (columnIndex.TryGetValue(tableKey, out var value))
                {
                    var tableColumns = value;
                    foreach (var column in
                                from i in tableColumns
                                where i.TableName.EqualsIgnoreCase(table.TableName)
                                orderby i.ColumnId
                                select i
                                )
                    {
                        table.Columns.Add(column);
                    }
                }
                tables.Add(tableKey);
                list.Add(table);
            }


            foreach (var databaseModelProvider in _databaseModelProviders)
            {
                databaseModelProvider.Initialize(table);
            }

        }

        return list;
    }

    public IEnumerable<TableModel> GetTables(DbConnection connection, IEnumerable<ColumnModel> columns)
    {
        DataTable dataTable = null;

        dataTable = GetTablesDataTable(connection);

        IList<TableModel> list = new List<TableModel>();

        if (dataTable != null)
        {
            list = GetTables(dataTable, columns).ToList();

            //Remove PowerBuilder Tables
            list = (
                from i in list
                where !i.TableName.StartsWith("pbcat", StringComparison.InvariantCultureIgnoreCase)
                select i).ToList();

            //Remove Access Sys Tables
            list = (
                from i in list
                where !i.TableName.StartsWith("MSys", StringComparison.InvariantCultureIgnoreCase)
                select i).ToList();

            //Remove Access Sys Tables
            list = (
                from i in list
                where !i.TableName.StartsWith("ISYS", StringComparison.InvariantCultureIgnoreCase)
                select i).ToList();
        }

        return list;
    }

    public IEnumerable<TableModel> GetViews(DbConnection connection, IEnumerable<ColumnModel> columns)
    {
        var dataTable = GetViewsDataTable(connection);
        if (dataTable != null)
        {
            return GetTables(dataTable, columns, true);
        }
        return Enumerable.Empty<TableModel>();
    }

    public IEnumerable<TriggerModel> GetTriggers(DbConnection connection)
    {
        IList<TriggerModel> list = new List<TriggerModel>();
        var dataTable = GetTriggersDataTable(connection);

        if (dataTable != null)
        {
            foreach (DataRow detailRow in dataTable.Rows)
            {
                var strTableName = GetStringValue(detailRow, "table_name");
                var strTriggerName = GetStringValue(detailRow, "trigger_name");
                var strDefinition = GetStringValue(detailRow, "definition");

                var trigger = new TriggerModel
                {
                    TableName = strTableName,
                    TriggerName = strTriggerName,
                    SchemaName = GetStringValue(detailRow, "schema_name"),
                    Definition = strDefinition.Replace("\t", "    ").RemoveLeadingAndTrailingLines()
                };

                list.Add(trigger);

                foreach (var databaseModelProvider in _databaseModelProviders)
                {
                    databaseModelProvider.Initialize(trigger);
                }
            }
        }

        return list;
    }

    #endregion

    #region Datatable

    protected abstract DataTable GetTablesDataTable(DbConnection connection);

    protected abstract DataTable GetViewsDataTable(DbConnection connection);

    protected abstract DataTable GetTriggersDataTable(DbConnection connection);

    protected abstract DataTable GetForeignKeysDataTable(DbConnection connection);

    protected abstract DataTable GetCheckConstraintsDataTable(DbConnection connection);

    protected abstract DataTable GetDefinitionsDataTable(DbConnection connection);

    protected abstract DataTable GetDefinitionDependenciesDataTable(DbConnection connection);

    protected abstract DataTable GetTableColumnsDataTable(DbConnection connection);

    protected abstract DataTable GetViewColumnsDataTable(DbConnection connection);

    protected abstract DataTable GetIndexeBucketCountsDataTable(DbConnection connection);

    protected abstract DataTable GetIndexesDataTable(DbConnection connection);

    protected abstract DataTable GetSecurityPoliciesDataTable(DbConnection connection);

    #endregion

    #region Provider

    public abstract IDatabaseObjectScript GetScript(CheckConstraintModel model);

    public abstract IDatabaseObjectScript GetScript(DefinitionModel model);

    public abstract IDatabaseObjectScript GetScript(ForeignKeyModel model);

    public abstract IDatabaseObjectScript GetScript(IndexModel model, bool isView);

    public abstract IDatabaseObjectScript GetScript(SecurityPolicyModel model);

    public abstract IDatabaseObjectScript GetScript(TableModel model);
    public abstract string GetRenameScript(TableModel model, string newTableName);

    public abstract IDatabaseObjectScript GetScript(TriggerModel model);

    public virtual void SetReadTimeout(DbCommand sourceCommand)
    {
    }

    public virtual string TransformConnectionString(string connectionString)
    {
        return connectionString;
    }

    public virtual void UpdateParameter(DbParameter parameter, ColumnModel column)
    {
    }

    public abstract ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType);

    public abstract DbConnection CreateConnection();

    public abstract bool TryHandleColumnValueLoadException(Exception ex, ColumnModel column, out object value);

    public virtual void ConvertBulk(TableConverterService service, IProgress<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, DbConnection targetConnection, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true)
    {
        throw new NotImplementedException();
    }

    public abstract string GetDatabaseName(string connectionString);

    public abstract bool SupportsConnection(DbConnection connection);

    public abstract int GetRowCount(DbCommand command, string schemaName, string tableName);
    public abstract string GetSelectStatement(string schemaName, string tableName, IEnumerable<string> columns);
    public abstract IDatabaseObjectScript GetColumnScript(ColumnModel column);

    #endregion

    #region Nested Types

    private class IndexBucket
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public string SchemaName { get; set; }
        public int BucketCount { get; set; }
    }

    #endregion

}
