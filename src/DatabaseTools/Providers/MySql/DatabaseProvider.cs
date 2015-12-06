using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Providers.MySql
{
    public class DatabaseProvider : IDatabaseProvider
    {
        public Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType)
        {
            if (targetDatabaseType == Models.DatabaseType.MicrosoftSQLServer)
            {
                var targetColumnType = new Models.ColumnTypeModel();

                targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Replace("UNSIGNED", "").Trim();

                if (targetColumnType.ColumnType.Contains("(") &&
                targetColumnType.ColumnType.EndsWith(")"))
                {
                    if (targetColumnType.ColumnType == "TINYINT(1)")
                    {
                        targetColumnType.ColumnType = "BIT";
                    }
                    else
                    {
                        targetColumnType.ColumnType = targetColumnType.ColumnType.Substring(0, targetColumnType.ColumnType.IndexOf("("));
                    }
                }

                targetColumnType.Precision = sourceColumnType.Precision;
                targetColumnType.Scale = sourceColumnType.Scale;

                switch (targetColumnType.ColumnType)
                {
                    case "LONG VARCHAR":
                    case "TEXT":
                    case "LONGTEXT":
                        targetColumnType.ColumnType = "NVARCHAR";
                        if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                    case "VARCHAR":
                    case "STRING":
                        targetColumnType.ColumnType = "NVARCHAR";
                        break;
                    case "INTEGER":
                    case "INT32":
                    case "MEDIUMINT":
                        targetColumnType.ColumnType = "INT";
                        break;
                    case "INT16":
                    case "TINYINT":
                        targetColumnType.ColumnType = "SMALLINT";
                        break;
                    case "NUMERIC":
                    case "DOUBLE":
                    case "SINGLE":
                    case "DEC":
                        targetColumnType.ColumnType = "DECIMAL";
                        break;
                    case "TIMESTAMP":
                        targetColumnType.ColumnType = "DATETIME";
                        break;
                    case "DATETIME":
                        targetColumnType.ColumnType = "DATETIME2";
                        break;
                    case "CHAR":
                        targetColumnType.ColumnType = "NCHAR";
                        break;
                    case "BOOLEAN":
                        targetColumnType.ColumnType = "BIT";
                        break;
                    case "BYTE[]":
                    case "BLOB":
                    case "LONGBLOB":
                        targetColumnType.ColumnType = "VARBINARY";
                        break;
                }

                switch (targetColumnType.ColumnType)
                {
                    case "NVARCHAR":
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
            DataTable dtIndexColumns;

            using (var conn = Processes.Database.CreateDbConnection(connectionString))
            {
                var dtIndex = conn.GetSchema("Indexes");

                dtIndexColumns = conn.GetSchema("IndexColumns");

                dtIndexColumns.Columns.Add("is_descending_key");
                dtIndexColumns.Columns.Add("is_included_column");
                dtIndexColumns.Columns.Add("is_unique");
                dtIndexColumns.Columns.Add("fill_factor");
                dtIndexColumns.Columns.Add("key_ordinal");
                dtIndexColumns.Columns.Add("is_primary_key");
                dtIndexColumns.Columns.Add("index_type");

                foreach (DataRow row in dtIndexColumns.Rows)
                {
                    string indexName = row["INDEX_NAME"].ToString();
                    string tableName = row["TABLE_NAME"].ToString();

                    var indexRow = (from i in dtIndex.Rows.OfType<DataRow>()
                                    where i["INDEX_NAME"].ToString() == indexName &&
                                          i["TABLE_NAME"].ToString() == tableName
                                    select i).Single();

                    row["is_descending_key"] = row["SORT_ORDER"].ToString() == "D";
                    row["is_included_column"] = false;
                    row["is_unique"] = Convert.ToBoolean(indexRow["UNIQUE"]);
                    row["fill_factor"] = 0;
                    row["key_ordinal"] = (int)row["ORDINAL_POSITION"];
                    row["is_primary_key"] = Convert.ToBoolean(indexRow["PRIMARY"]);
                    row["index_type"] = "NONCLUSTERED";

                    if (Convert.ToBoolean(indexRow["PRIMARY"]))
                    {
                        row["INDEX_NAME"] = "PK_" + tableName;
                        row["index_type"] = "CLUSTERED";
                    }
                    else if (indexName.StartsWith("fk", StringComparison.InvariantCultureIgnoreCase))
                    {
                        row["INDEX_NAME"] = "IDX_" + indexName;
                    }
                    else if (indexName.StartsWith("index_", StringComparison.InvariantCultureIgnoreCase))
                    {
                        indexName = indexName.Substring(6);
                        if (indexName.StartsWith(tableName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            row["INDEX_NAME"] = "IDX_" + indexName;
                        }
                        else
                        {
                            row["INDEX_NAME"] = "IDX_" + tableName + "_" + indexName;
                        }
                    }
                    else if (!indexName.StartsWith("IDX", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (indexName.StartsWith(tableName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            row["INDEX_NAME"] = "IDX_" + indexName;
                        }
                        else
                        {
                            row["INDEX_NAME"] = "IDX_" + tableName + "_" + indexName;
                        }
                    }


                }

            }

            return dtIndexColumns;
        }
    }
}
