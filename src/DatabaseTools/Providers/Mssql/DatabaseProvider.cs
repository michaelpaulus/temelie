using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseTools.Models;

namespace DatabaseTools.Providers.Mssql
{
    public class DatabaseProvider : IDatabaseProvider
    {
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
                        if (targetColumnType.Precision.GetValueOrDefault() > 4000)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                }

                return targetColumnType;

            }

            return null;
        }

        public DataTable GetDefinitionDependencies(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sysobjects.name, 
	                        r.referencing_entity_name 
                        FROM 
	                        sysobjects INNER JOIN 
	                        sys.schemas ON 
		                        sysobjects.uid = sys.schemas.schema_id CROSS APPLY 
	                        sys.dm_sql_referencing_entities(sys.schemas.name + '.' + sysobjects.name, 'OBJECT') r 
                        WHERE 
	                        sysobjects.xtype IN ('P', 'V', 'FN', 'IF') AND 
	                        sysobjects.category = 0 AND 
	                        sysobjects.name NOT LIKE '%diagram%' AND 
	                        sys.schemas.name = 'dbo' 
                        ORDER BY 
	                        sysobjects.name, 
	                        r.referencing_entity_name
                        ";
            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetDefinitions(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sysobjects.name, 
	                        sysobjects.xtype, 
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
	                        sysobjects.name NOT LIKE '%diagram%' AND 
	                        sys.schemas.name = 'dbo' 
                        ORDER BY 
	                        sysobjects.xtype, 
	                        sysobjects.name
                        ";
            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetForeignKeys(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sys.tables.name AS table_name, 
	                        sys.foreign_keys.name AS foreign_key_name, 
	                        parent_columns.name AS column_name, 
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
		                        sys.foreign_key_columns.referenced_column_id = referenced_columns.column_id 
                        WHERE 
	                        sys.tables.name NOT LIKE 'sys%' 
                        ORDER BY 
	                        sys.tables.name, 
	                        sys.foreign_keys.name, 
	                        sys.foreign_key_columns.constraint_column_id
                        ";

            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetIndexes(ConnectionStringSettings connectionString)
        {
            var dtIndexes = Processes.Database.Execute(connectionString, @"
SELECT 
    sys.tables.name AS table_name, 
    sys.indexes.name AS index_name, 
    sys.indexes.type_desc index_type, 
    sys.indexes.is_unique, 
    sys.indexes.fill_factor, 
    sys.columns.name AS column_name, 
    sys.index_columns.is_included_column, 
    sys.index_columns.is_descending_key, 
    sys.index_columns.key_ordinal,
    sys.indexes.is_primary_key
FROM 
    sys.indexes INNER JOIN 
    sys.tables ON sys.indexes.object_id = sys.tables.object_id INNER JOIN 
    sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND 
    sys.indexes.index_id = sys.index_columns.index_id INNER JOIN 
    sys.columns ON sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id 
WHERE 
    sys.tables.name NOT LIKE 'sys%' AND 
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

        public DataTable GetTableColumns(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sys.tables.name AS table_name, 
	                        sys.columns.name AS column_name, 
	                        UPPER(sys.types.name) AS column_type, 
	                        CASE ISNULL(sys.columns.precision, 0) WHEN 0 THEN sys.columns.max_length ELSE ISNULL(sys.columns.precision, 0) END AS precision, 
	                        ISNULL(sys.columns.scale, 0) AS scale, 
	                        sys.columns.is_nullable, 
	                        sys.columns.is_identity, 
	                        sys.columns.is_computed, 
	                        ISNULL(sys.computed_columns.definition, '') computed_definition, 
	                        sys.columns.column_id, 
	                        ISNULL(sys.default_constraints.definition, '') column_default,
	                        ISNULL((SELECT 1 FROM sys.indexes INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id WHERE sys.indexes.is_primary_key = 1 AND sys.indexes.object_id = sys.tables.object_id AND sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id), 0) is_primary_key 
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
	                        sys.tables.name NOT LIKE 'sys%' AND 
	                        (sys.schemas.name = 'dbo') 
                        ORDER BY 
	                        sys.tables.name, 
	                        sys.columns.column_id
                        ";

            DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetTables(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sys.tables.name AS table_name 
                        FROM 
	                        sys.tables INNER JOIN 
	                        sys.schemas ON 
		                        sys.tables.schema_id = sys.schemas.schema_id 
                        WHERE 
	                        sys.tables.name NOT LIKE 'sys%' AND 
	                        sys.schemas.name = 'dbo' 
                        ORDER BY 
	                        sys.tables.name
                        ";

            DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetTriggers(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                    sys.tables.name AS table_name, 
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
	                    sys.tables.name NOT LIKE 'sys%' AND 
	                    sys.schemas.name = 'dbo' 
                    ORDER BY 
	                    sys.tables.name, 
	                    sys.triggers.name
                        ";

            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetViewColumns(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sys.views.name AS table_name, 
	                        sys.columns.name AS column_name, 
	                        UPPER(sys.types.name) AS column_type, 
	                        CASE ISNULL(sys.columns.precision, 0) WHEN 0 THEN sys.columns.max_length ELSE ISNULL(sys.columns.precision, 0) END AS precision, 
	                        ISNULL(sys.columns.scale, 0) AS scale, 
	                        sys.columns.is_nullable, 
	                        sys.columns.is_identity, 
	                        sys.columns.is_computed, 
	                        ISNULL(sys.computed_columns.definition, '') computed_definition, 
	                        sys.columns.column_id, 
	                        ISNULL(sys.default_constraints.definition, '') column_default,
	                        ISNULL((SELECT 1 FROM sys.indexes INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id WHERE sys.indexes.is_primary_key = 1 AND sys.indexes.object_id = sys.views.object_id AND sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id), 0) is_primary_key 
                        FROM 
	                        sys.views INNER JOIN 
	                        sys.schemas on 
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
	                        sys.views.name NOT LIKE 'sys%' AND 
	                        (sys.schemas.name = 'dbo') 
                        ORDER BY 
	                        sys.views.name,
	                        sys.columns.column_id
                        ";

            DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetViews(ConnectionStringSettings connectionString)
        {
            string sql = @"
                        SELECT 
	                        sys.views.name AS table_name 
                        FROM 
	                        sys.views INNER JOIN 
	                        sys.schemas ON 
		                        sys.views.schema_id = sys.schemas.schema_id 
                        WHERE 
	                        sys.views.name NOT LIKE 'sys%' AND 
	                        sys.schemas.name = 'dbo' 
                        ORDER BY 
	                        sys.views.name
                        ";

            DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }
    }
}
