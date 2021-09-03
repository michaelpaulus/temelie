using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseTools.Models;
using DatabaseTools.Configuration;

namespace DatabaseTools.Providers.Mssql
{
    [Export(typeof(IDatabaseProvider))]
    public class DatabaseProvider : IDatabaseProvider
    {

        public DatabaseTools.Models.DatabaseType ForDatabaseType
        {
            get
            {
                return Models.DatabaseType.MicrosoftSQLServer;
            }
        }

        public DbProviderFactory CreateProvider()
        {
            return System.Data.SqlClient.SqlClientFactory.Instance;
        }

        public ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType, DatabaseType targetDatabaseType)
        {
            if (targetDatabaseType == Models.DatabaseType.MicrosoftSQLServer)
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
                            targetColumnType.Precision.GetValueOrDefault() == 0)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                }

                return targetColumnType;

            }

            return null;
        }

        public DataTable GetDefinitionDependencies(DbConnection connection)
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
            System.Data.DataSet ds = Processes.Database.Execute(connection, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetDefinitions(DbConnection connection)
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
                            sysobjects.xtype IN ('P', 'V', 'FN', 'IF') AND 
                            sysobjects.category = 0 AND 
                            sysobjects.name NOT LIKE '%diagram%' 
                        ORDER BY 
                            sysobjects.xtype, 
                            sysobjects.name
                        ";
            System.Data.DataSet ds = Processes.Database.Execute(connection, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }
        public DataTable GetSecurityPolicies(DbConnection connection)
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
            System.Data.DataSet ds = Processes.Database.Execute(connection, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetForeignKeys(DbConnection connection)
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

            System.Data.DataSet ds = Processes.Database.Execute(connection, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetIndexeBucketCounts(DbConnection connection)
        {
            try
            {
                var dtIndexes = Processes.Database.Execute(connection, @"
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

        public DataTable GetIndexes(DbConnection connection)
        {
            var dtIndexes = Processes.Database.Execute(connection, @"
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

        public DataTable GetTableColumns(DbConnection connection)
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
                            ISNULL(sys.computed_columns.definition, '') computed_definition, 
                            sys.columns.column_id, 
                            0 is_hidden,
                            0 generated_always_type,
                            ISNULL(sys.default_constraints.definition, '') column_default,
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
    ISNULL(sys.computed_columns.definition, '') computed_definition,
    sys.columns.column_id,
    sys.columns.is_hidden,
    sys.columns.generated_always_type,
    ISNULL(sys.default_constraints.definition, '') column_default,
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
            name <> 'sys_data_classification_recommendation_disabled'
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

            DataSet ds = null;

            try
            {
                ds = Processes.Database.Execute(connection, sql2016);
            }
            catch
            {
                ds = Processes.Database.Execute(connection, sql2014);
            }

            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetTables(DbConnection connection)
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
                            '[]' extended_properties
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
    tables.name AS table_name,
    schemas.name schema_name,
    tables.temporal_type,
    ISNULL(
    (
        SELECT
            t1.name
        FROM
            sys.tables t1
        WHERE
            t1.object_id = tables.history_table_id
    ), '') history_table_name,
    tables.is_memory_optimized,
    tables.durability_desc,
    tables.is_external,
    external_data_sources.name data_source_name,
    ISNULL((SELECT
        name,
        value
    FROM
        fn_listextendedproperty (NULL, 'schema', schemas.name, 'table', tables.name, default, default)
    FOR JSON AUTO
    ), '[]') extended_properties
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
ORDER BY
    tables.name
                        ";

            DataSet ds = null;
            try
            {
                ds = Processes.Database.Execute(connection, sql2016);
            }
            catch
            {
                ds = Processes.Database.Execute(connection, sql2014);
            }

            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetTriggers(DbConnection connection)
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

            System.Data.DataSet ds = Processes.Database.Execute(connection, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetViewColumns(DbConnection connection)
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
    ISNULL(sys.computed_columns.definition, '') computed_definition,
    sys.columns.column_id,
    ISNULL(sys.default_constraints.definition, '') column_default,
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
    ISNULL(sys.computed_columns.definition, '') computed_definition,
    sys.columns.column_id,
    ISNULL(sys.default_constraints.definition, '') column_default,
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
            name <> 'sys_data_classification_recommendation_disabled'
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

            DataSet ds = null;
            try
            {
                ds = Processes.Database.Execute(connection, sql2016);
            }
            catch
            {
                ds = Processes.Database.Execute(connection, sql2014);
            }
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetViews(DbConnection connection)
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

            DataSet ds = null;
            try
            {
                ds = Processes.Database.Execute(connection, sql2016);
            }
            catch
            {
                ds = Processes.Database.Execute(connection, sql2014);
            }

            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public void SetReadTimeout(DbCommand sourceCommand)
        {

        }

        public string TransformConnectionString(string connectionString)
        {
            System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            return csb.ConnectionString;
        }

        public bool TryHandleColumnValueLoadException(Exception ex, ColumnModel column, out object value)
        {
            value = null;
            return false;
        }

        public void UpdateParameter(DbParameter parameter, ColumnModel column)
        {
            switch (parameter.DbType)
            {
                case System.Data.DbType.StringFixedLength:
                case System.Data.DbType.String:
                    parameter.Size = column.Precision;
                    break;
                case System.Data.DbType.Time:
                    if ((parameter) is System.Data.SqlClient.SqlParameter)
                    {
                        ((System.Data.SqlClient.SqlParameter)parameter).SqlDbType = System.Data.SqlDbType.Time;
                    }
                    break;
            }
        }
    }
}
