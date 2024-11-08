using System.Data;
using System.Data.Common;
using MySqlConnector;

namespace Temelie.Database.Providers.MySql;
public partial class DatabaseProvider
{

    protected override DataTable GetDefinitionDependenciesDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetDefinitionsDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetIndexeBucketCountsDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetSecurityPoliciesDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetForeignKeysDataTable(DbConnection connection)
    {
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);
        var sql = $@"
SELECT
    '' schema_name,
    KCU.table_name,
    KCU.constraint_name AS foreign_key_name,
    KCU.column_name,
    KCU.referenced_table_name,
    KCU.referenced_column_name,
    0 AS is_not_for_replication,
    CASE WHEN RC.delete_rule = 'RESTRICT' THEN 'NO ACTION' ELSE RC.delete_rule END delete_action,
    CASE WHEN RC.update_rule = 'RESTRICT' THEN 'NO ACTION' ELSE RC.update_rule END update_action
FROM
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU JOIN
    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC ON
        KCU.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG AND
        KCU.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA AND
        KCU.CONSTRAINT_NAME = RC.CONSTRAINT_NAME
WHERE
    RC.constraint_schema = '{csb.Database}'
ORDER BY
    KCU.table_name,
    KCU.CONSTRAINT_NAME,
    KCU.ORDINAL_POSITION
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetCheckConstraintsDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetIndexesDataTable(DbConnection connection)
    {
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);

        var sql = $@"
SELECT
    '' schema_name,
    statistics.table_name,
    statistics.index_name,
    statistics.column_name,
    statistics.seq_in_index AS key_ordinal,
    statistics.sub_part,
    statistics.index_type,
    CASE WHEN statistics.non_unique = 0 THEN 1 ELSE 0 END AS is_unique
FROM
    information_schema.statistics
WHERE
    statistics.table_schema = '{csb.Database}'
ORDER BY
    statistics.table_name,
    statistics.index_name,
    statistics.column_name
";

        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];

        dataTable.Columns.Add("is_descending_key");
        dataTable.Columns.Add("is_included_column");
        dataTable.Columns.Add("fill_factor");
        dataTable.Columns.Add("is_primary_key");
        dataTable.Columns.Add("filter_definition");
        dataTable.Columns.Add("partition_scheme_name");
        dataTable.Columns.Add("data_compression_desc");
        dataTable.Columns.Add("partition_ordinal");

        if (dataTable.Columns["index_name"].MaxLength < 500)
        {
            dataTable.Columns["index_name"].MaxLength = 500;
        }

        foreach (var row in dataTable.Rows.OfType<DataRow>())
        {

            row["is_descending_key"] = false;
            row["is_included_column"] = false;
            row["fill_factor"] = 0;
            row["partition_ordinal"] = 0;
            row["is_primary_key"] = row["index_name"].ToString().ToUpper() == "PRIMARY";
            if (Convert.ToBoolean(row["is_primary_key"]))
            {
                row["index_name"] = "PK_" + row["table_name"].ToString();
            }

        }

        return dataTable;
    }

    protected override DataTable GetTableColumnsDataTable(DbConnection connection)
    {
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);
        var sql = $@"
SELECT
    '' schema_name,
    columns.table_name,
    columns.column_name,
    columns.ordinal_position,
    columns.column_default,
    columns.is_nullable,
    columns.data_type,
    columns.character_maximum_length,
    columns.numeric_precision,
    columns.numeric_scale,
    columns.datetime_precision,
    columns.column_type,
    columns.column_key,
    columns.extra,
    columns.character_set_name,
    columns.collation_name
FROM
    information_schema.columns
WHERE
    columns.table_schema = '{csb.Database}'
ORDER BY
    columns.table_name,
    columns.ordinal_position,
    columns.column_name
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        DataTable dataTypes;

        dataTypes = connection.GetSchema("DataTypes");

        this.UpdateSchemaColumns(dataTable, dataTypes);
        return dataTable;
    }

    protected override DataTable GetTablesDataTable(DbConnection connection)
    {
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);
        var sql = $@"
SELECT
    '' schema_name,
    tables.table_name,
    tables.engine,
    tables.table_collation collation_name,
    (SELECT
        character_set_name 
    FROM
        information_schema.collation_character_set_applicability
    WHERE
        collation_character_set_applicability.collation_name = tables.table_collation
    ) character_set_name
FROM
    information_schema.tables
WHERE
    table_schema = '{csb.Database}'
ORDER BY
    tables.table_name
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetTriggersDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetViewColumnsDataTable(DbConnection connection)
    {
        return null;
    }

    protected override DataTable GetViewsDataTable(DbConnection connection)
    {
        return null;
    }

}
