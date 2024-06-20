using System.Data;
using System.Data.Common;
using System.Text;
using Temelie.Database.Models;
using Temelie.Database.Services;
using Temelie.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace Temelie.Database.Providers.Mssql;

[ExportProvider(typeof(IDatabaseProvider))]
public class DatabaseProvider : DatabaseProviderBase
{

    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;
    private readonly IDatabaseExecutionService _databaseService;

    public DatabaseProvider(IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
    {
        _connectionCreatedNotifications = connectionCreatedNotifications;
        var factory = new DatabaseFactory([this], _connectionCreatedNotifications);
        _databaseService = new Services.DatabaseExecutionService(factory);
    }

    public static string ProviderName => nameof(SqlConnection);

    public override string QuoteCharacterStart => "[";
    public override string QuoteCharacterEnd => "]";

    public override string Name => ProviderName;

    public override string DefaultConnectionString => "Data Source=(local);Initial Catalog=Database;Integrated Security=True;User Id=;Password=;Encrypt=False;";

    public DbProviderFactory CreateProvider()
    {
        return SqlClientFactory.Instance;
    }

    public override ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType)
    {

        var targetColumnType = new Models.ColumnTypeModel();

        targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Trim();

        targetColumnType.Precision = sourceColumnType.Precision;
        targetColumnType.Scale = sourceColumnType.Scale;

        switch (targetColumnType.ColumnType)
        {
            case "TEXT":
                targetColumnType.ColumnType = "NVARCHAR";
                if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
        }

        switch (targetColumnType.ColumnType)
        {
            case "NVARCHAR":
            case "VARCHAR":
            case "VARBINARY":
                if (targetColumnType.Precision.GetValueOrDefault() > 4000 ||
                    targetColumnType.Precision.GetValueOrDefault() == 0 ||
                    targetColumnType.Precision.GetValueOrDefault() == -1)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
        }

        return targetColumnType;

    }

    protected override DataTable GetDefinitionDependenciesDataTable(DbConnection connection)
    {
        string sql = @"
                        SELECT 
                            sysobjects.name, 
                            sys.schemas.name schema_name,
                            r.referencing_entity_name 
                        FROM 
                            sysobjects INNER JOIN 
                            sys.schemas ON 
                                sysobjects.uid = sys.schemas.schema_id CROSS APPLY 
                            sys.dm_sql_referencing_entities(sys.schemas.name + '.' + sysobjects.name, 'OBJECT') r 
                        WHERE 
                            sysobjects.xtype IN ('P', 'V', 'FN', 'IF') AND 
                            sysobjects.category = 0 AND 
                            sysobjects.name NOT LIKE '%diagram%' 
                        ORDER BY 
                            sysobjects.name, 
                            r.referencing_entity_name
                        ";
        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetDefinitionsDataTable(DbConnection connection)
    {
        string sql = @"
                        SELECT 
                            sysobjects.name, 
                            sysobjects.xtype, 
                            sys.schemas.name schema_name,
                            ISNULL(sys.sql_modules.definition, sys.system_sql_modules.definition) AS definition
                        FROM 
                            sysobjects INNER JOIN 
                            sys.schemas ON 
                                sysobjects.uid = sys.schemas.schema_id LEFT OUTER JOIN 
                            sys.sql_modules ON 
                                sys.sql_modules.object_id = sysobjects.id LEFT OUTER JOIN 
                            sys.system_sql_modules ON 
                                sys.system_sql_modules.object_id = sysobjects.id 
                        WHERE 
                            sysobjects.xtype IN ('P', 'V', 'FN', 'IF', 'TF') AND 
                            sysobjects.category = 0 AND 
                            sysobjects.name NOT LIKE '%diagram%' 
                        ORDER BY 
                            sysobjects.xtype, 
                            sysobjects.name
                        ";
        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }
    protected override DataTable GetSecurityPoliciesDataTable(DbConnection connection)
    {
        string sql = @"
       SELECT policySchema.name PolicySchema,
       sys.security_policies.name PolicyName,
       predicate_type_desc PredicateType,
       predicate_definition PredicateDefinition,
       targetSchema.name TargetSchema,
       [target].name TargetName,
       is_enabled IsEnabled,
       operation_desc Operation,
       is_schema_bound IsSchemaBound
FROM sys.security_policies
     INNER JOIN sys.schemas policySchema ON policySchema.schema_id = sys.security_policies.schema_id
     INNER JOIN sys.security_predicates ON sys.security_policies.object_id = sys.security_predicates.object_id
     INNER JOIN sys.sysobjects [target] ON [target].id = target_object_id
     INNER JOIN sys.schemas targetSchema ON targetSchema.schema_id = [target].uid
                        ";
        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetForeignKeysDataTable(DbConnection connection)
    {
        string sql = @"
SELECT 
    sys.tables.name AS table_name, 
    sys.schemas.name schema_name,
    sys.foreign_keys.name AS foreign_key_name, 
    parent_columns.name AS column_name, 
    referenced_schemas.name referenced_schema_name,
    referenced_tables.name AS referenced_table_name, 
    referenced_columns.name AS referenced_column_name, 
    sys.foreign_keys.is_not_for_replication, 
    sys.foreign_keys.delete_referential_action_desc delete_action, 
    sys.foreign_keys.update_referential_action_desc update_action 
FROM 
    sys.foreign_keys INNER JOIN 
    sys.tables ON 
        sys.foreign_keys.parent_object_id = sys.tables.object_id INNER JOIN 
    sys.tables AS referenced_tables ON 
        sys.foreign_keys.referenced_object_id = referenced_tables.object_id INNER JOIN 
    sys.foreign_key_columns ON 
        sys.foreign_keys.object_id = sys.foreign_key_columns.constraint_object_id INNER JOIN 
    sys.columns AS parent_columns ON 
        sys.foreign_key_columns.parent_object_id = parent_columns.object_id AND 
        sys.foreign_key_columns.parent_column_id = parent_columns.column_id INNER JOIN 
    sys.columns AS referenced_columns ON 
        sys.foreign_key_columns.referenced_object_id = referenced_columns.object_id AND 
        sys.foreign_key_columns.referenced_column_id = referenced_columns.column_id INNER JOIN 
    sys.schemas on 
        sys.tables.schema_id = sys.schemas.schema_id INNER JOIN 
    sys.schemas referenced_schemas on 
        referenced_tables.schema_id = referenced_schemas.schema_id 
WHERE 
    sys.tables.name <> 'sysdiagrams'
ORDER BY 
    sys.tables.name, 
    sys.foreign_keys.name, 
    sys.foreign_key_columns.constraint_column_id
                        ";

        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetCheckConstraintsDataTable(DbConnection connection)
    {
        string sql = @"
SELECT 
    sys.tables.name AS table_name, 
    sys.schemas.name schema_name,
    sys.check_constraints.name AS check_constraint_name, 
    sys.check_constraints.definition AS check_constraint_definition
FROM 
    sys.check_constraints INNER JOIN 
    sys.tables ON 
        sys.check_constraints.parent_object_id = sys.tables.object_id INNER JOIN 
    sys.schemas ON
        sys.tables.schema_id = sys.schemas.schema_id
ORDER BY 
    sys.tables.name, 
    sys.check_constraints.name
                        ";

        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetIndexeBucketCountsDataTable(DbConnection connection)
    {
        try
        {
            var dtIndexes = _databaseService.Execute(connection, @"
SELECT
    sys.tables.name AS table_name, 
    sys.schemas.name schema_name,
    sys.indexes.name AS index_name, 
    dm_db_xtp_hash_index_stats.total_bucket_count
FROM
    sys.indexes INNER JOIN
    sys.tables ON 
       sys.indexes.object_id = sys.tables.object_id INNER JOIN 
   sys.dm_db_xtp_hash_index_stats  ON
       indexes.index_id = dm_db_xtp_hash_index_stats.index_id AND 
       indexes.object_id = dm_db_xtp_hash_index_stats.object_id INNER JOIN 
    sys.schemas on 
        sys.tables.schema_id = sys.schemas.schema_id
").Tables[0];

            return dtIndexes;
        }
        catch
        {

        }
        return null;
    }

    protected override DataTable GetIndexesDataTable(DbConnection connection)
    {
        var dtIndexes = _databaseService.Execute(connection, @"
SELECT
	sys.tables.name AS table_name,
	sys.schemas.name schema_name,
	sys.indexes.name AS index_name,
	sys.indexes.type_desc index_type,
	sys.indexes.is_unique,
	sys.indexes.fill_factor,
	sys.columns.name AS column_name,
	sys.index_columns.is_included_column,
	sys.index_columns.is_descending_key,
	sys.index_columns.key_ordinal,
	sys.index_columns.partition_ordinal,
	sys.indexes.is_primary_key,
	sys.indexes.filter_definition,
	(
		SELECT
			partition_schemes.name
		FROM
			sys.partition_schemes
		WHERE
			partition_schemes.data_space_id = indexes.data_space_id
	) partition_scheme_name,
	(
		SELECT
			TOP 1
			partitions.data_compression_desc
		FROM
			sys.partitions
		WHERE
			partitions.object_id = tables.object_id AND
			partitions.index_id = indexes.index_id AND
			partitions.data_compression_desc <> 'NONE'
	) data_compression_desc
FROM
	sys.indexes INNER JOIN
	sys.tables ON
		sys.indexes.object_id = sys.tables.object_id INNER JOIN
	sys.index_columns ON
		sys.indexes.object_id = sys.index_columns.object_id AND
		sys.indexes.index_id = sys.index_columns.index_id INNER JOIN
	sys.columns ON
		sys.index_columns.object_id = sys.columns.object_id AND
		sys.index_columns.column_id = sys.columns.column_id INNER JOIN
	sys.schemas ON
		sys.tables.schema_id = sys.schemas.schema_id
WHERE
	sys.tables.name <> 'sysdiagrams' AND
	sys.indexes.name NOT LIKE 'MSmerge_index%' AND
	sys.indexes.name IS NOT NULL
ORDER BY
	sys.tables.name,
	sys.indexes.name,
	sys.index_columns.key_ordinal,
	sys.index_columns.index_column_id
").Tables[0];

        return dtIndexes;
    }

    protected override DataTable GetTableColumnsDataTable(DbConnection connection)
    {
        string sql2014 = @"
                        SELECT
                            sys.tables.name AS table_name, 
                            sys.schemas.name schema_name,
                            sys.columns.name AS column_name, 
                            UPPER(sys.types.name) AS column_type, 
                            CASE ISNULL(sys.columns.precision, 0) 
                                WHEN 0 THEN 
                                    CASE WHEN sys.types.name = 'nvarchar' OR
                                            sys.types.name = 'nchar' THEN 
                                        sys.columns.max_length / 2
                                    ELSE
                                        sys.columns.max_length 
                                    END 
                                ELSE 
                                    ISNULL(sys.columns.precision, 0) 
                            END AS precision, 
                            ISNULL(sys.columns.scale, 0) AS scale, 
                            sys.columns.is_nullable, 
                            sys.columns.is_identity, 
                            CASE 
                                WHEN sys.columns.is_computed = 1 THEN 1  
                                ELSE 0 
                            END is_computed, 
                            sys.computed_columns.definition computed_definition, 
                            sys.columns.column_id, 
                            0 is_hidden,
                            0 generated_always_type,
                            sys.default_constraints.definition column_default,
                            ISNULL((SELECT 1 FROM sys.indexes INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id WHERE sys.indexes.is_primary_key = 1 AND sys.indexes.object_id = sys.tables.object_id AND sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id), 0) is_primary_key,
                            '[]' extended_properties
                        FROM 
                            sys.tables INNER JOIN 
                            sys.schemas on 
                                sys.tables.schema_id = sys.schemas.schema_id INNER JOIN 
                            sys.columns ON 
                                sys.tables.object_id = sys.columns.object_id INNER JOIN 
                            sys.types ON 
                                sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN 
                            sys.computed_columns ON 
                                sys.columns.object_id = sys.computed_columns.object_id AND 
                                sys.columns.column_id = sys.computed_columns.column_id LEFT OUTER JOIN
                            sys.default_constraints ON
                                sys.columns.object_id = sys.default_constraints.parent_object_id AND
                                sys.columns.column_id = sys.default_constraints.parent_column_id

                        WHERE 
                            sys.tables.name <> 'sysdiagrams'
                        ORDER BY 
                            sys.tables.name, 
                            sys.columns.column_id
                        ";

        string sql2016 = @"
SELECT
    sys.tables.name AS table_name,
    sys.schemas.name schema_name,
    sys.columns.name AS column_name,
    UPPER(sys.types.name) AS column_type,
    CASE
        ISNULL(sys.columns.precision, 0)
        WHEN 0 THEN
            CASE
                WHEN sys.types.name = 'nvarchar' OR
                    sys.types.name = 'nchar' THEN
                    sys.columns.max_length / 2
                ELSE
                    sys.columns.max_length
            END
        ELSE
            ISNULL(sys.columns.precision, 0)
    END AS precision,
    ISNULL(sys.columns.scale, 0) AS scale,
    sys.columns.is_nullable,
    sys.columns.is_identity,
    CASE
        WHEN sys.columns.is_computed = 1 THEN
            1
        WHEN sys.columns.generated_always_type <> 0 THEN
            1
        ELSE
            0
    END is_computed,
    sys.computed_columns.definition computed_definition,
    sys.columns.column_id,
    sys.columns.is_hidden,
    sys.columns.generated_always_type,
    sys.default_constraints.definition column_default,
    ISNULL(
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.index_columns ON
                sys.indexes.object_id = sys.index_columns.object_id AND
                sys.indexes.index_id = sys.index_columns.index_id
        WHERE
            sys.indexes.is_primary_key = 1 AND
            sys.indexes.object_id = sys.tables.object_id AND
            sys.index_columns.object_id = sys.columns.object_id AND
            sys.index_columns.column_id = sys.columns.column_id
    ), 0) is_primary_key,
    ISNULL(
    (
        SELECT
            name,
            value
        FROM
            fn_listextendedproperty(NULL, 'schema', schemas.name, 'table', tables.name, 'column', columns.name)
        WHERE
            name <> 'sys_data_classification_recommendation_disabled' AND
            name NOT LIKE 'MS_%'
        FOR JSON AUTO
    ), '[]') extended_properties
FROM
    sys.tables INNER JOIN
    sys.schemas ON
        sys.tables.schema_id = sys.schemas.schema_id INNER JOIN
    sys.columns ON
        sys.tables.object_id = sys.columns.object_id INNER JOIN
    sys.types ON
        sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN
    sys.computed_columns ON
        sys.columns.object_id = sys.computed_columns.object_id AND
        sys.columns.column_id = sys.computed_columns.column_id LEFT OUTER JOIN
    sys.default_constraints ON
        sys.columns.object_id = sys.default_constraints.parent_object_id AND
        sys.columns.column_id = sys.default_constraints.parent_column_id

WHERE
    sys.tables.name <> 'sysdiagrams'
ORDER BY
    sys.tables.name,
    sys.columns.column_id
";

        DataSet ds;
        try
        {
            ds = _databaseService.Execute(connection, sql2016);
        }
        catch
        {
            ds = _databaseService.Execute(connection, sql2014);
        }

        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetTablesDataTable(DbConnection connection)
    {
        string sql2014 = @"
SELECT 
    tables.name AS table_name,
    schemas.name schema_name,
    0 temporal_type,
    '' history_table_name,
    0 is_memory_optimized,
    '' durability_desc,
    0 is_external,
    '' data_source_name,
    '[]' extended_properties,
    NULL partition_scheme_name,
    NULL partition_scheme_columns
FROM 
    sys.tables INNER JOIN 
    sys.schemas ON 
        tables.schema_id = schemas.schema_id 
WHERE 
    tables.name <> 'sysdiagrams'
ORDER BY 
    tables.name
                        ";

        string sql2016 = @"
SELECT
	t1.table_name,
	t1.schema_name,
	t1.temporal_type,
	t1.history_table_name,
	t1.is_memory_optimized,
	t1.durability_desc,
	t1.is_external,
	t1.data_source_name,
	t1.extended_properties,
	(
		SELECT
			partition_schemes.name
		FROM
			sys.indexes INNER JOIN
			sys.partition_schemes ON
				indexes.data_space_id = partition_schemes.data_space_id
		WHERE
			indexes.object_id = t1.object_id AND
			indexes.index_id = t1.partition_index_id
	) partition_scheme_name,
	(
		SELECT
			STRING_AGG(columns.name, ',')
		FROM
			sys.indexes INNER JOIN
			sys.partition_schemes ON
				indexes.data_space_id = partition_schemes.data_space_id INNER JOIN
			sys.index_columns ON
				indexes.object_id = index_columns.object_id AND
				indexes.index_id = index_columns.index_id INNER JOIN
			sys.columns ON
				sys.index_columns.object_id = sys.columns.object_id AND
				sys.index_columns.column_id = sys.columns.column_id
		WHERE
			indexes.object_id = t1.object_id AND
			indexes.index_id = t1.partition_index_id AND
			index_columns.partition_ordinal > 0
	) partition_scheme_columns
FROM
	(
		SELECT
			tables.object_id,
			tables.name AS table_name,
			schemas.name schema_name,
			tables.temporal_type,
			(
				SELECT
					t1.name
				FROM
					sys.tables t1
				WHERE
					t1.object_id = tables.history_table_id
			) history_table_name,
			tables.is_memory_optimized,
			tables.durability_desc,
			tables.is_external,
			external_data_sources.name data_source_name,
			ISNULL(
			(
				SELECT
					name,
					value
				FROM
					fn_listextendedproperty(NULL, 'schema', schemas.name, 'table', tables.name, DEFAULT, DEFAULT)
                WHERE
		            name NOT LIKE 'MS_%'
				FOR JSON AUTO
			), '[]') extended_properties,
			(
				SELECT
				TOP 1
					indexes.index_id
				FROM
					sys.indexes INNER JOIN
					sys.partition_schemes ON
						indexes.data_space_id = partition_schemes.data_space_id
				WHERE
					indexes.object_id = tables.object_id
			) partition_index_id
		FROM
			sys.tables INNER JOIN
			sys.schemas ON
				tables.schema_id = schemas.schema_id LEFT JOIN
			sys.external_tables ON
				tables.object_id = external_tables.object_id LEFT JOIN
			sys.external_data_sources ON
				external_tables.data_source_id = external_data_sources.data_source_id
		WHERE
			tables.name <> 'sysdiagrams'
	) t1
ORDER BY
	t1.table_name
                        ";

        DataSet ds;
        try
        {
            ds = _databaseService.Execute(connection, sql2016);
        }
        catch
        {
            ds = _databaseService.Execute(connection, sql2014);
        }

        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetTriggersDataTable(DbConnection connection)
    {
        string sql = @"
                    SELECT
                        *
                    FROM
                        (
                        SELECT 
                            sys.tables.name AS table_name, 
                            sys.schemas.name schema_name,
                            sys.triggers.name AS trigger_name, 
                            ISNULL(sys.sql_modules.definition, sys.system_sql_modules.definition) AS definition 
                        FROM 
                            sys.triggers INNER JOIN 
                            sys.tables ON 
                                sys.triggers.parent_id = sys.tables.object_id INNER JOIN 
                            sys.schemas ON 
                                sys.tables.schema_id = sys.schemas.schema_id LEFT OUTER JOIN 
                            sys.sql_modules ON 
                                sys.sql_modules.object_id = sys.triggers.object_id LEFT OUTER JOIN 
                            sys.system_sql_modules ON 
                                sys.system_sql_modules.object_id = sys.triggers.object_id 
                        WHERE 
                            sys.tables.name <> 'sysdiagrams'
                        UNION ALL
                        SELECT 
                            sys.views.name AS table_name, 
                            sys.schemas.name schema_name,
                            sys.triggers.name AS trigger_name, 
                            ISNULL(sys.sql_modules.definition, sys.system_sql_modules.definition) AS definition 
                        FROM 
                            sys.triggers INNER JOIN 
                            sys.views ON 
                                sys.triggers.parent_id = sys.views.object_id INNER JOIN 
                            sys.schemas ON 
                                sys.views.schema_id = sys.schemas.schema_id LEFT OUTER JOIN 
                            sys.sql_modules ON 
                                sys.sql_modules.object_id = sys.triggers.object_id LEFT OUTER JOIN 
                            sys.system_sql_modules ON 
                                sys.system_sql_modules.object_id = sys.triggers.object_id 
                        WHERE 
                            sys.views.name <> 'sysdiagrams'
                    ) t1 
                    ORDER BY
                        t1.table_name,
                        t1.trigger_name
                        ";

        System.Data.DataSet ds = _databaseService.Execute(connection, sql);
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetViewColumnsDataTable(DbConnection connection)
    {
        string sql2014 = @"
SELECT
    sys.views.name AS table_name,
    sys.schemas.name schema_name,
    sys.columns.name AS column_name,
    UPPER(sys.types.name) AS column_type,
    CASE
        ISNULL(sys.columns.precision, 0)
        WHEN 0 THEN
            CASE
                WHEN sys.types.name = 'nvarchar' OR
                    sys.types.name = 'nchar' THEN
                    sys.columns.max_length / 2
                ELSE
                    sys.columns.max_length
            END
        ELSE
            ISNULL(sys.columns.precision, 0)
    END AS precision,
    ISNULL(sys.columns.scale, 0) AS scale,
    sys.columns.is_nullable,
    sys.columns.is_identity,
    sys.columns.is_computed,
    sys.computed_columns.definition computed_definition,
    sys.columns.column_id,
    sys.default_constraints.definition column_default,
    ISNULL(
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.index_columns ON
                sys.indexes.object_id = sys.index_columns.object_id AND
                sys.indexes.index_id = sys.index_columns.index_id
        WHERE
            sys.indexes.is_primary_key = 1 AND
            sys.indexes.object_id = sys.views.object_id AND
            sys.index_columns.object_id = sys.columns.object_id AND
            sys.index_columns.column_id = sys.columns.column_id
    ), 0) is_primary_key,
    '[]' extended_properties
FROM
    sys.views INNER JOIN
    sys.schemas ON
        sys.views.schema_id = sys.schemas.schema_id INNER JOIN
    sys.columns ON
        sys.views.object_id = sys.columns.object_id INNER JOIN
    sys.types ON
        sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN
    sys.computed_columns ON
        sys.columns.object_id = sys.computed_columns.object_id AND
        sys.columns.column_id = sys.computed_columns.column_id LEFT OUTER JOIN
    sys.default_constraints ON
        sys.columns.object_id = sys.default_constraints.parent_object_id AND
        sys.columns.column_id = sys.default_constraints.parent_column_id
WHERE
    sys.views.name <> 'sysdiagrams'
ORDER BY
    sys.views.name,
    sys.columns.column_id
                        ";

        string sql2016 = @"
SELECT
    sys.views.name AS table_name,
    sys.schemas.name schema_name,
    sys.columns.name AS column_name,
    UPPER(sys.types.name) AS column_type,
    CASE
        ISNULL(sys.columns.precision, 0)
        WHEN 0 THEN
            CASE
                WHEN sys.types.name = 'nvarchar' OR
                    sys.types.name = 'nchar' THEN
                    sys.columns.max_length / 2
                ELSE
                    sys.columns.max_length
            END
        ELSE
            ISNULL(sys.columns.precision, 0)
    END AS precision,
    ISNULL(sys.columns.scale, 0) AS scale,
    sys.columns.is_nullable,
    sys.columns.is_identity,
    sys.columns.is_computed,
    sys.computed_columns.definition computed_definition,
    sys.columns.column_id,
    sys.default_constraints.definition column_default,
    ISNULL(
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.index_columns ON
                sys.indexes.object_id = sys.index_columns.object_id AND
                sys.indexes.index_id = sys.index_columns.index_id
        WHERE
            sys.indexes.is_primary_key = 1 AND
            sys.indexes.object_id = sys.views.object_id AND
            sys.index_columns.object_id = sys.columns.object_id AND
            sys.index_columns.column_id = sys.columns.column_id
    ), 0) is_primary_key,
    ISNULL(
    (
        SELECT
            name,
            value
        FROM
            fn_listextendedproperty(NULL, 'schema', schemas.name, 'view', views.name, 'column', columns.name)
        WHERE
            name <> 'sys_data_classification_recommendation_disabled' AND
		    name NOT LIKE 'MS_%'
        FOR JSON AUTO
    ), '[]') extended_properties
FROM
    sys.views INNER JOIN
    sys.schemas ON
        sys.views.schema_id = sys.schemas.schema_id INNER JOIN
    sys.columns ON
        sys.views.object_id = sys.columns.object_id INNER JOIN
    sys.types ON
        sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN
    sys.computed_columns ON
        sys.columns.object_id = sys.computed_columns.object_id AND
        sys.columns.column_id = sys.computed_columns.column_id LEFT OUTER JOIN
    sys.default_constraints ON
        sys.columns.object_id = sys.default_constraints.parent_object_id AND
        sys.columns.column_id = sys.default_constraints.parent_column_id
WHERE
    sys.views.name <> 'sysdiagrams'
ORDER BY
    sys.views.name,
    sys.columns.column_id
                        ";

        DataSet ds;
        try
        {
            ds = _databaseService.Execute(connection, sql2016);
        }
        catch
        {
            ds = _databaseService.Execute(connection, sql2014);
        }
        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    protected override DataTable GetViewsDataTable(DbConnection connection)
    {

        string sql2014 = @"
SELECT
    sys.views.name AS table_name,
    sys.schemas.name schema_name,
    '[]' extended_properties
FROM
    sys.views INNER JOIN
    sys.schemas ON
        sys.views.schema_id = sys.schemas.schema_id
WHERE
    sys.views.name <> 'sysdiagrams'
ORDER BY
    sys.views.name
                        ";

        string sql2016 = @"
SELECT
    sys.views.name AS table_name,
    sys.schemas.name schema_name,
    ISNULL((SELECT
        name,
        value
    FROM
        fn_listextendedproperty (NULL, 'schema', schemas.name, 'view', views.name, default, default)
    WHERE
        name NOT LIKE 'MS_%'
    FOR JSON AUTO
    ), '[]') extended_properties
FROM
    sys.views INNER JOIN
    sys.schemas ON
        sys.views.schema_id = sys.schemas.schema_id
WHERE
    sys.views.name <> 'sysdiagrams'
ORDER BY
    sys.views.name
                        ";

        DataSet ds;
        try
        {
            ds = _databaseService.Execute(connection, sql2016);
        }
        catch
        {
            ds = _databaseService.Execute(connection, sql2014);
        }

        DataTable dataTable = ds.Tables[0];
        return dataTable;
    }

    public override string TransformConnectionString(string connectionString)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
        return csb.ConnectionString;
    }

    public override bool TryHandleColumnValueLoadException(Exception ex, ColumnModel column, out object value)
    {
        value = null;
        return false;
    }

    public override void UpdateParameter(DbParameter parameter, ColumnModel column)
    {
        switch (parameter.DbType)
        {
            case System.Data.DbType.StringFixedLength:
            case System.Data.DbType.String:
                parameter.Size = column.Precision;
                break;
            case System.Data.DbType.Time:
                if (parameter is SqlParameter parameter1)
                {
                    parameter1.SqlDbType = System.Data.SqlDbType.Time;
                }
                break;
        }
    }

    public override void ConvertBulk(TableConverterService service, IProgress<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, DbConnection targetConnection, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true)
    {
        progress?.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        int intProgress = 0;

        if (validateTargetTable)
        {
            var targetRowCount = service.GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        void bulkCopy(SqlTransaction transaction1)
        {
            int intRowIndex = 0;

            var sourceMatchedColumns = service.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
            var targetMatchedColumns = service.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

            using (SqlBulkCopy bcp = new SqlBulkCopy((SqlConnection)targetConnection, SqlBulkCopyOptions.KeepIdentity, transaction1))
            {
                bcp.DestinationTableName = $"[{targetTable.SchemaName}].[{targetTable.TableName}]";
                bcp.BatchSize = batchSize == 0 ? 10000 : batchSize;
                bcp.BulkCopyTimeout = 600;
                bcp.NotifyAfter = bcp.BatchSize;

                foreach (var targetColumn in targetMatchedColumns)
                {
                    bcp.ColumnMappings.Add(targetColumn.ColumnName, targetColumn.ColumnName);
                }

                bcp.SqlRowsCopied += (object sender, SqlRowsCopiedEventArgs e) =>
                {
                    if (progress != null &&
                        sourceRowCount > 0)
                    {
                        intRowIndex += bcp.BatchSize;

                        if (intRowIndex > sourceRowCount)
                        {
                            intRowIndex = sourceRowCount;
                        }

                        int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)sourceRowCount * 100);

                        if (intProgress != intNewProgress &&
                            intNewProgress < 100)
                        {
                            intProgress = intNewProgress;
                            progress.Report(new TableProgress() { ProgressPercentage = intProgress, Table = sourceTable });
                        }
                    }
                };

                bcp.WriteToServer(sourceReader);

            }

        }

        if (System.Transactions.Transaction.Current == null && useTransaction)
        {
            using (var transaction = (SqlTransaction)targetConnection.BeginTransaction())
            {
                bulkCopy(transaction);
                transaction.Commit();
            }
        }
        else
        {
            bulkCopy(null);
        }

        if (progress != null &&
            intProgress != 100)
        {
            progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
        }
    }

    public override string GetDatabaseName(string connectionString)
    {
        var sqlCommandBuilder = new SqlConnectionStringBuilder(connectionString);
        return sqlCommandBuilder.InitialCatalog;
    }

    public override bool SupportsConnection(DbConnection connection)
    {
        return connection is SqlConnection;
    }

    public override DbConnection CreateConnection()
    {
        return new SqlConnection();
    }

    public override IDatabaseObjectScript GetScript(CheckConstraintModel model)
    {
        string generateDropScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            sb.AppendLine($@"IF EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = '{model.CheckConstraintName}' AND
        schemas.name = '{model.SchemaName}'
)
BEGIN
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.check_constraints INNER JOIN
            sys.tables ON
                check_constraints.parent_object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            check_constraints.name = '{model.CheckConstraintName}' AND
            schemas.name = '{model.SchemaName}' AND
            check_constraints.definition = '{model.CheckConstraintDefinition.Replace("'", "''")}'
    )");
            sb.AppendLine($@"BEGIN
ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.CheckConstraintName}{QuoteCharacterEnd}
END
END
GO");

            return sb.ToString();

        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = '{model.CheckConstraintName}' AND
        schemas.name = '{model.SchemaName}'
)");
            sb.AppendLine($"ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} WITH CHECK ADD CONSTRAINT {QuoteCharacterStart}{model.CheckConstraintName}{QuoteCharacterEnd} CHECK ({model.CheckConstraintDefinition})");
            sb.AppendLine("GO");

            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(DefinitionModel model)
    {

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("-- {0}", model.DefinitionName));
            sb.AppendLine($"DROP {model.Type} IF EXISTS {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.DefinitionName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine();

            string strPattern = $"(CREATE\\s*{model.Type}\\s*[\\[]?)([\\[]?{model.SchemaName}[\\.]?[\\]]?[\\.]?[\\[]?)?({model.DefinitionName})([\\]]?)";

            string strDefinitionReplacement = $"CREATE {model.Type} {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.DefinitionName}{QuoteCharacterEnd}";

            model.Definition = System.Text.RegularExpressions.Regex.Replace(model.Definition, strPattern, strDefinitionReplacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

            sb.AppendLine(model.Definition);
            sb.AppendLine("GO");

            if (model.View != null)
            {
                AddExtendedProperties(model.View, sb);
            }

            return sb.ToString();

        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);

    }

    public override IDatabaseObjectScript GetScript(ForeignKeyModel model)
    {

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.Append($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = '{model.ForeignKeyName}' AND
            schemas.name = '{model.SchemaName}'
    )
BEGIN
    IF NOT EXISTS
        (
            SELECT
                1
            FROM
                sys.foreign_keys INNER JOIN
                sys.schemas ON
                    foreign_keys.schema_id = schemas.schema_id
            WHERE
                foreign_keys.name = '{model.ForeignKeyName}' AND
                schemas.name = '{model.SchemaName}' AND
                foreign_keys.delete_referential_action_desc = '{model.DeleteAction}' AND
                foreign_keys.update_referential_action_desc = '{model.UpdateAction}'");
            var i = 0;

            foreach (var detail in model.Detail)
            {
                i++;
                sb.Append($@" AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.parent_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.parent_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = '{model.SchemaName}' AND
                        tables.name = '{model.TableName}' AND
                        columns.name = '{detail.Column}' AND
                        foreign_key_columns.constraint_column_id = {i}
                ) AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.referenced_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.referenced_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = '{model.ReferencedSchemaName}' AND
                        tables.name = '{model.ReferencedTableName}' AND
                        columns.name = '{detail.ReferencedColumn}' AND
                        foreign_key_columns.constraint_column_id = {i}
                )");
            }

            sb.AppendLine();

            sb.AppendLine($@"        )
    BEGIN
        ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.ForeignKeyName}{QuoteCharacterEnd}
    END
END
GO");

            return sb.ToString();

        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();

            string strColumnNames = string.Empty;
            string strReferencedColumnNames = string.Empty;

            foreach (var item in model.Detail)
            {
                string strColumnName = item.Column;
                string strReferencedColumnName = item.ReferencedColumn;

                if (strColumnNames.Length > 0)
                {
                    strColumnNames += ", ";
                }

                strColumnNames += $"{QuoteCharacterStart}{strColumnName}{QuoteCharacterEnd}";

                if (strReferencedColumnNames.Length > 0)
                {
                    strReferencedColumnNames += ", ";
                }

                strReferencedColumnNames += $"{QuoteCharacterStart}{strReferencedColumnName}{QuoteCharacterEnd}"; ;
            }

            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = '{model.ForeignKeyName}' AND
            schemas.name = '{model.SchemaName}'
    )");
            sb.AppendLine($"    ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} WITH CHECK ADD CONSTRAINT {QuoteCharacterStart}{model.ForeignKeyName}{QuoteCharacterEnd} FOREIGN KEY ({strColumnNames})");
            sb.AppendLine($"    REFERENCES {QuoteCharacterStart}{model.ReferencedSchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.ReferencedTableName}{QuoteCharacterEnd} ({strReferencedColumnNames})");

            if (model.UpdateAction != "NO_ACTION")
            {
                sb.AppendLine("    " + string.Format("ON UPDATE {0}", model.UpdateAction.Replace("_", " ")));
            }

            if (model.DeleteAction != "NO_ACTION")
            {
                sb.AppendLine("    " + string.Format("ON DELETE {0}", model.DeleteAction.Replace("_", " ")));
            }

            sb.AppendLine("GO");

            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);

    }

    public override IDatabaseObjectScript GetScript(IndexModel model)
    {

        string generateDropScript()
        {
            var sb = new StringBuilder();
            if (model.IsPrimaryKey)
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
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine($"    ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd}");
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
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine($"    DROP INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            }
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            if (model.IndexType.Contains("HASH"))
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
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine("    " + $"ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} ADD INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {model.IndexType}");
                sb.AppendLine("    " + "(");

                int intColumnCount = 0;

                foreach (var column in model.Columns)
                {
                    if (intColumnCount > 0)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append($"        {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                    intColumnCount += 1;
                }
                sb.AppendLine();
                sb.AppendLine("    " + ")");

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            else if (model.IsPrimaryKey)
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
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine("    " + $"ALTER TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} ADD CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} PRIMARY KEY {model.IndexType}");
                sb.AppendLine("    " + "(");

                int intColumnCount = 0;

                foreach (var column in model.Columns)
                {
                    if (intColumnCount > 0)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append($"    {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                    intColumnCount += 1;
                }
                sb.AppendLine();
                sb.AppendLine("    " + ")");

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            else
            {
                string indexType = model.IndexType;

                if (model.IsUnique)
                {
                    indexType = "UNIQUE " + model.IndexType;
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
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine("    " + $"CREATE {indexType} INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");

                if (model.Columns.Any())
                {
                    sb.AppendLine("    " + "(");

                    bool blnHasColumns = false;

                    foreach (var column in model.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append($"    {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                        blnHasColumns = true;
                    }

                    sb.AppendLine();
                    sb.AppendLine("    " + ")");

                    if (model.IncludeColumns.Any())
                    {
                        sb.Append("    INCLUDE (");

                        bool blnHasIncludeColumns = false;

                        foreach (var includeColumn in model.IncludeColumns)
                        {
                            if (blnHasIncludeColumns)
                            {
                                sb.Append(", ");
                            }
                            sb.Append($"{QuoteCharacterStart}{includeColumn.ColumnName}{QuoteCharacterEnd}");
                            blnHasIncludeColumns = true;
                        }

                        sb.AppendLine(")");
                    }
                }

                if (!string.IsNullOrEmpty(model.FilterDefinition))
                {
                    sb.AppendLine($"    WHERE {model.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
                }

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(SecurityPolicyModel model)
    {

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.security_policies INNER JOIN
            sys.schemas ON
                schemas.schema_id = security_policies.schema_id
        WHERE
            schemas.name = '{model.PolicySchema}' AND
            security_policies.name = '{model.PolicyName}'
    )");

            sb.AppendLine($"    CREATE SECURITY POLICY {QuoteCharacterStart}{model.PolicySchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.PolicyName}{QuoteCharacterEnd}");
            var predicateScripts = new List<StringBuilder>();
            foreach (var predicate in model.Predicates)
            {
                var predicateSql = new StringBuilder();
                predicateSql.AppendLine($"    ADD {predicate.PredicateType} PREDICATE {predicate.PredicateDefinition.Substring(1, predicate.PredicateDefinition.Length - 2)}");
                predicateSql.Append($"    ON {QuoteCharacterStart}{predicate.TargetSchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{predicate.TargetName}{QuoteCharacterEnd}");
                if (predicate.Operation != null && predicate.Operation.Length > 0)
                {
                    predicateSql.AppendLine();
                    predicateSql.Append(predicate.Operation);
                }
                predicateScripts.Add(predicateSql);
            }
            sb.AppendLine(string.Join(",", predicateScripts));
            var suffixes = new List<string>();

            if (!model.IsEnabled)
            {
                suffixes.Add("STATE = OFF");
            }

            if (!model.IsSchemaBound)
            {
                suffixes.Add("SCHEMABINDING = OFF");
            }

            if (suffixes.Count > 0)
            {
                sb.AppendLine("    WITH (" + string.Join(", ", suffixes) + ")");
            }

            sb.AppendLine();
            sb.AppendLine("GO");

            return sb.ToString();
        }

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.security_policies INNER JOIN
            sys.schemas ON
                schemas.schema_id = security_policies.schema_id
        WHERE
            schemas.name = '{model.PolicySchema}' AND
            security_policies.name = '{model.PolicyName}'
    )");
            sb.AppendLine($"    DROP SECURITY POLICY {QuoteCharacterStart}{model.PolicySchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.PolicyName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(TableModel model)
    {

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{model.TableName}' AND
            schemas.name = '{model.SchemaName}'
    )");
            sb.AppendLine($"    DROP{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine(string.Format("-- {0}", model.TableName));

            if (model.TableName.StartsWith("default_", StringComparison.InvariantCultureIgnoreCase))
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
            tables.name = '{model.TableName}' AND
            schemas.name = '{model.SchemaName}'
    )");
                sb.AppendLine($"    DROP{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
                sb.AppendLine("GO");
            }
            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{model.TableName}' AND
            schemas.name = '{model.SchemaName}'
    )");

            sb.AppendLine($"    CREATE{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            sb.AppendLine("    " + "(");

            int intColumnCount = 0;

            foreach (Models.ColumnModel column in (
                from i in model.Columns
                orderby i.ColumnId
                select i))
            {
                if (intColumnCount != 0)
                {
                    sb.AppendLine(",");
                }

                sb.Append("    " + "    " + column.ToString(QuoteCharacterStart, QuoteCharacterEnd));

                intColumnCount += 1;
            }

            if (model.Columns.Where(c => c.GeneratedAlwaysType == 1).Any() &&
                model.Columns.Where(c => c.GeneratedAlwaysType == 2).Any())
            {
                sb.AppendLine(",");
                sb.Append($"        PERIOD FOR SYSTEM_TIME ({model.Columns.Where(c => c.GeneratedAlwaysType == 1).First().ColumnName}, {model.Columns.Where(c => c.GeneratedAlwaysType == 2).First().ColumnName})");
            }

            if (model.IsMemoryOptimized)
            {
                if (model.PrimaryKey != null)
                {
                    sb.AppendLine(",");
                    AppendTableInlineCreateScript(model.PrimaryKey, sb);
                }
            }
            else
            {
                sb.AppendLine();
            }

            sb.AppendLine("    )");

            AddOptions(sb);

            if (!string.IsNullOrEmpty(model.PartitionSchemeName))
            {
                sb.AppendLine($"    ON {model.PartitionSchemeName} ({model.PartitionSchemeColumns})");
            }

            sb.AppendLine("GO");

            AddExtendedProperties(model, sb);

            return sb.ToString();
        }

        void AddOptions(StringBuilder sb)
        {
            var options = model.Options;
            if (model.IsMemoryOptimized)
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"MEMORY_OPTIMIZED = ON, DURABILITY = {model.DurabilityDesc}";
            }
            if (!string.IsNullOrEmpty(model.DataSourceName))
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"DATA_SOURCE = {model.DataSourceName}";
            }
            if (!string.IsNullOrEmpty(options))
            {
                sb.AppendLine($"    WITH ({options})");
            }
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(TriggerModel model)
    {

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TRIGGER IF EXISTS {QuoteCharacterStart}{model.SchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TriggerName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(model.Definition);
            sb.AppendLine("GO");
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    private void AddExtendedProperties(TableModel table, StringBuilder sb)
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

    private void AppendTableInlineCreateScript(IndexModel model, System.Text.StringBuilder sb)
    {
        if (model.IsPrimaryKey)
        {
            sb.AppendLine($"        CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {(model.IsPrimaryKey ? "PRIMARY KEY" : "")} {model.IndexType}");
            sb.AppendLine("        (");
            int intColumnCount = 0;
            foreach (var column in model.Columns)
            {
                if (intColumnCount > 0)
                {
                    sb.AppendLine(",");
                }

                sb.Append($"            {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                intColumnCount += 1;
            }
            sb.AppendLine();
            sb.AppendLine("        )");

            AddOptions(model, sb, 2);
        }
        else
        {
            string indexType = model.IndexType;

            if (model.IsUnique)
            {
                indexType = "UNIQUE " + model.IndexType;
            }

            sb.AppendLine($"        INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {indexType}");

            if (model.Columns.Any())
            {
                sb.AppendLine("        (");

                bool blnHasColumns = false;

                foreach (var column in model.Columns)
                {
                    if (blnHasColumns)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append($"            {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                    blnHasColumns = true;
                }

                sb.AppendLine();
                sb.AppendLine("        )");

                if (model.IncludeColumns.Any())
                {
                    sb.Append("        INCLUDE (");

                    bool blnHasIncludeColumns = false;

                    foreach (var includeColumn in model.IncludeColumns)
                    {
                        if (blnHasIncludeColumns)
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{QuoteCharacterStart}{includeColumn.ColumnName}{QuoteCharacterEnd}");
                        blnHasIncludeColumns = true;
                    }

                    sb.AppendLine("        )");
                }
            }

            if (!string.IsNullOrEmpty(model.FilterDefinition))
            {
                sb.AppendLine($"        WHERE {model.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
            }

            AddOptions(model, sb, 2);
        }
    }

    private void AddOptions(IndexModel model, StringBuilder sb, int indentCount = 1)
    {
        if (model.FillFactor != 0 || model.TotalBucketCount != 0 || !string.IsNullOrEmpty(model.DataCompressionDesc))
        {
            var sbOptions = new StringBuilder();
            if (model.FillFactor != 0)
            {
                sbOptions.Append($"FILLFACTOR = {model.FillFactor}");
            }
            if (model.TotalBucketCount != 0)
            {
                if (sbOptions.Length > 0)
                {
                    sbOptions.Append(", ");
                }
                sbOptions.Append($"BUCKET_COUNT = {model.TotalBucketCount}");
            }
            if (!string.IsNullOrEmpty(model.DataCompressionDesc))
            {
                if (sbOptions.Length > 0)
                {
                    sbOptions.Append(", ");
                }
                sbOptions.Append($"DATA_COMPRESSION = {model.DataCompressionDesc}");
            }
            sb.AppendLine($"{new string(' ', indentCount * 4)}WITH ({sbOptions.ToString()})");
        }

        if (!string.IsNullOrEmpty(model.PartitionSchemeName))
        {
            var partitionColumns = model.Columns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
            if (!partitionColumns.Any())
            {
                partitionColumns = model.IncludeColumns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
            }
            if (partitionColumns.Any())
            {
                sb.AppendLine($"{new string(' ', indentCount * 4)}ON {model.PartitionSchemeName} ({string.Join(", ", partitionColumns)})");
            }
            else
            {
                sb.AppendLine($"{new string(' ', indentCount * 4)}ON {model.PartitionSchemeName}");
            }
        }

    }

}
