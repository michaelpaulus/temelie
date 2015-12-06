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
    }
}
