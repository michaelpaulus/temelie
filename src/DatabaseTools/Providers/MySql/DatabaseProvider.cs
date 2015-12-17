using DatabaseTools.Extensions;
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

        public DataTable GetDefinitionDependencies(ConnectionStringSettings connectionString)
        {
            return null;
        }

        public DataTable GetDefinitions(ConnectionStringSettings connectionString)
        {
            return null;
        }

        public DataTable GetForeignKeys(ConnectionStringSettings connectionString)
        {
            DataTable dataTable;
            using (var conn = Processes.Database.CreateDbConnection(connectionString))
            {
                DataTable dtForeignKeys = conn.GetSchema("Foreign Keys");

                dataTable = conn.GetSchema("Foreign Key Columns");

                dtForeignKeys.Columns["constraint_name"].ColumnName = "foreign_key_name";
                dataTable.Columns["constraint_name"].ColumnName = "foreign_key_name";

                dataTable.Columns.Add("delete_action", typeof(string));
                dataTable.Columns.Add("update_action", typeof(string));
                dataTable.Columns.Add("is_not_for_replication", typeof(bool));

                foreach (var row in dataTable.Rows.OfType<DataRow>())
                {
                    string tableName = row["table_name"].ToString();
                    string fkName = row["foreign_key_name"].ToString();

                    var fkRow = (from i in dtForeignKeys.Rows.OfType<DataRow>()
                                 where 
                                    i["table_name"].ToString() == tableName &&
                                    i["foreign_key_name"].ToString() == fkName
                                 select i).First();

                    row["delete_action"] = fkRow["delete_rule"].ToString().Replace("RESTRICT", "NO ACTION");
                    row["update_action"] = fkRow["update_rule"].ToString().Replace("RESTRICT", "NO ACTION");
                    row["is_not_for_replication"] = false;

                }


            }
            return dataTable;
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

        public DataTable GetTableColumns(ConnectionStringSettings connectionString)
        {
            var csb = new global::MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString.ConnectionString);
            var sql = $"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE table_schema = '{csb.Database}'";
            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            DataTable dataTypes;
            using (var conn = Processes.Database.CreateDbConnection(connectionString))
            {
                dataTypes = conn.GetSchema("DataTypes");
            }
            this.UpdateSchemaColumns(dataTable, dataTypes);
            return dataTable;
        }

        public DataTable GetTables(ConnectionStringSettings connectionString)
        {
            var csb = new global::MySql.Data.MySqlClient.MySqlConnectionStringBuilder(connectionString.ConnectionString);
            var sql = $"SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = '{csb.Database}'";
            System.Data.DataSet ds = Processes.Database.Execute(connectionString, sql);
            DataTable dataTable = ds.Tables[0];
            return dataTable;
        }

        public DataTable GetTriggers(ConnectionStringSettings connectionString)
        {
            return null;
        }

        public DataTable GetViewColumns(ConnectionStringSettings connectionString)
        {
            return null;
        }

        public DataTable GetViews(ConnectionStringSettings connectionString)
        {
            return null;
        }

        protected void UpdateSchemaColumns(DataTable table, DataTable dataTypes)
        {
            if (table.Columns.Contains("ordinal_position"))
            {
                table.Columns["ordinal_position"].ColumnName = "column_id";
            }

            if (table.Columns.Contains("type_name") && !(table.Columns.Contains("column_type")))
            {
                table.Columns["type_name"].ColumnName = "column_type";
            }

            if (!table.Columns.Contains("is_identity"))
            {
                table.Columns.Add("is_identity", typeof(bool));
                foreach (var row in table.Rows.OfType<System.Data.DataRow>())
                {
                    row["is_identity"] = false;

                    if (table.Columns.Contains("EXTRA"))
                    {
                        string value = Convert.ToString(row["EXTRA"]);
                        if (value != null &&
                            value.EqualsIgnoreCase("auto_increment"))
                        {
                            row["is_identity"] = true;
                        }
                    }

                }
            }

            if (table.Columns.Contains("data_type") && !(table.Columns.Contains("column_type")))
            {
                table.Columns.Add("column_type", typeof(string));

                foreach (var row in table.Rows.OfType<System.Data.DataRow>())
                {
                    string strColumnType = Convert.ToString(row["data_type"]);

                    if (!(string.IsNullOrEmpty(strColumnType)) && strColumnType.ToUpper().EndsWith(" IDENTITY"))
                    {
                        strColumnType = strColumnType.Substring(0, strColumnType.Length - " IDENTITY".Length);
                        row["is_identity"] = true;
                    }

                    var dataTypeRows = (
                        from i in dataTypes.Rows.OfType<System.Data.DataRow>()
                        where string.Equals(Convert.ToString(i["ProviderDbType"]), strColumnType)
                        select i).ToList();

                    if (dataTypeRows.Count == 0 && dataTypes.Columns.Contains("SqlType"))
                    {
                        dataTypeRows = (
                            from i in dataTypes.Rows.OfType<System.Data.DataRow>()
                            where string.Equals(Convert.ToString(i["SqlType"]), strColumnType)
                            select i).ToList();
                    }

                    var dataTypeRow = dataTypeRows.FirstOrDefault();

                    if (dataTypeRow != null)
                    {
                        strColumnType = Convert.ToString(dataTypeRow["DataType"]);
                        if (strColumnType.Contains('.'.ToString()))
                        {
                            strColumnType = strColumnType.Split('.')[strColumnType.Split('.').Length - 1];
                        }
                    }

                    row["column_type"] = strColumnType;
                }

                table.Columns.Remove("data_type");
            }

            if (table.Columns.Contains("numeric_scale"))
            {
                table.Columns["numeric_scale"].ColumnName = "scale";
            }

            if (table.Columns.Contains("decimal_digits"))
            {
                table.Columns["decimal_digits"].ColumnName = "scale";
            }

            if (table.Columns.Contains("num_prec_radix"))
            {
                table.Columns["num_prec_radix"].ColumnName = "numeric_precision";
            }

            if (table.Columns.Contains("column_size"))
            {
                table.Columns["column_size"].ColumnName = "character_maximum_length";
            }

            foreach (var row in table.Rows.OfType<DataRow>())
            {

                string columnDefault = Processes.Database.GetStringValue(row, "COLUMN_DEFAULT");
                
                if (!string.IsNullOrEmpty(columnDefault))
                {
                    if (columnDefault == "NULL" ||
                        columnDefault == "(NULL)")
                    {
                        columnDefault = "";
                    }
                    else if (columnDefault == "0000-00-00 00:00:00")
                    {
                        columnDefault = "";
                    }
                    else if (columnDefault == "b'0'")
                    {
                        columnDefault = "0";
                    }
                }

                row["COLUMN_DEFAULT"] = columnDefault;  

            }

            if (table.Columns.Contains("numeric_precision"))
            {
                if (table.Columns.Contains("character_maximum_length"))
                {
                    table.Columns.Add("precision", typeof(Int32));
                    foreach (System.Data.DataRow row in table.Rows)
                    {
                        if (row.IsNull("numeric_precision"))
                        {
                            if (!row.IsNull("character_maximum_length"))
                            {
                                try
                                {
                                    row["precision"] = row["character_maximum_length"];
                                }
#pragma warning disable CS0168 // Variable is declared but never used
                                catch (ArgumentException ex)
#pragma warning restore CS0168 // Variable is declared but never used
                                {
                                    row["precision"] = Int32.MaxValue;
                                }
                            }
                        }
                        else
                        {
                            row["precision"] = row["numeric_precision"];
                        }
                    }
                }
                else
                {
                    table.Columns["numeric_precision"].ColumnName = "precision";
                }
            }


        }

    }
}
