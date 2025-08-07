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
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);

        string sql = $@"
SELECT
    routines.routine_name AS name, 
    'P' AS xtype, 
    '' AS schema_name,
    '' AS definition
FROM
    information_schema.routines
WHERE 
    routine_schema = '{csb.Database}' AND 
    routine_type = 'PROCEDURE'
";

        var dataTable = _database.Execute(connection, sql).Tables[0];

        dataTable.Columns["definition"].MaxLength = int.MaxValue;

        foreach (DataRow row in dataTable.Rows)
        {
            var name = row["name"].ToString();
            var procedureDefinition = _database.Execute(connection, $"SHOW CREATE PROCEDURE `{name}`").Tables[0];
            row["definition"] = procedureDefinition.Rows[0]["Create Procedure"].ToString().Replace(" DEFINER=`root`@`%`", "") + ";";
        }

        sql = $@"
SELECT 
    views.table_name AS name, 
    'V' AS xtype, 
    '' AS schema_name,
    CONCAT('CREATE VIEW `', views.table_name, '` AS ', REPLACE(views.view_definition, CONCAT('`', views.table_schema, '`.'), ''), ';') AS definition
FROM 
    information_schema.views
WHERE
    views.table_schema = '{csb.Database}' 
ORDER BY 
   views.table_name
                ";

        var viewsDataTable = _database.Execute(connection, sql).Tables[0];

        foreach (DataRow row in viewsDataTable.Rows)
        {
            var newRow = dataTable.NewRow();
            newRow["name"] = row["name"];
            newRow["xtype"] = row["xtype"];
            newRow["schema_name"] = row["schema_name"];
            newRow["definition"] = row["definition"];
            dataTable.Rows.Add(newRow);
        }

        return dataTable;

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
    '' referenced_schema_name,
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
        dataTable.Columns.Add("compression_delay");

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
    columns.collation_name,
    columns.generation_expression
FROM
    information_schema.columns INNER JOIN
    information_schema.tables ON
        columns.table_schema = tables.table_schema AND
        columns.table_name = tables.table_name 
WHERE
    columns.table_schema = '{csb.Database}' AND
    tables.table_type = 'BASE TABLE'
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
    table_schema = '{csb.Database}' AND
    table_type = 'BASE TABLE'
ORDER BY
    tables.table_name
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetTriggersDataTable(DbConnection connection)
    {
        var csb = new MySqlConnectionStringBuilder(connection.ConnectionString);
        var sql = $@"
SELECT
    '' schema_name,
    trigger_name,
    event_object_table table_name,
    CONCAT(
            'CREATE TRIGGER `', trigger_name, '` ',
            action_timing, ' ', event_manipulation, ' ON `', event_object_table, '` ',
            'FOR EACH ', action_orientation, ' ',
            action_statement
        ) definition
FROM
    information_schema.triggers
WHERE
    event_object_schema <> 'sys' AND
    event_object_schema = '{csb.Database}'
ORDER BY
    event_object_table,
    trigger_name;
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetViewColumnsDataTable(DbConnection connection)
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
    information_schema.columns INNER JOIN
    information_schema.tables ON
        columns.table_schema = tables.table_schema AND
        columns.table_name = tables.table_name 
WHERE
    columns.table_schema = '{csb.Database}' AND
    tables.table_type = 'VIEW'
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

    protected override DataTable GetViewsDataTable(DbConnection connection)
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
    table_schema = '{csb.Database}' AND
    table_type = 'VIEW'
ORDER BY
    tables.table_name
";
        System.Data.DataSet ds = _database.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

}
