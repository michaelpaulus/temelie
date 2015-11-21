
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
    namespace Processes
    {
        public class Database
        {

            private Database()
            {

            }

            #region Methods

            public static System.Data.Common.DbConnection CloneDbConnection(System.Data.Common.DbConnection dbConnection)
            {
                System.Data.Common.DbConnection connection = CreateDbProviderFactory(dbConnection).CreateConnection();
                connection.ConnectionString = dbConnection.ConnectionString;
                connection.Open();
                return connection;
            }

            public static System.Data.Common.DbCommand CreateDbCommand(System.Data.Common.DbConnection dbConnection)
            {
                System.Data.Common.DbCommand command = dbConnection.CreateCommand();
                command.Connection = dbConnection;

                command.CommandTimeout = Math.Max(1800, command.CommandTimeout);

                if (Data.DbTransactionScope.Current != null && Data.DbTransactionScope.Current.Connection == dbConnection)
                {
                    command.Transaction = Data.DbTransactionScope.Current;
                }
                return command;
            }

            public static System.Data.Common.DbConnection CreateDbConnection(System.Configuration.ConnectionStringSettings connectionString)
            {
                return CreateDbConnection(CreateDbProviderFactory(connectionString), connectionString);
            }

            public static System.Data.Common.DbConnection CreateDbConnection(System.Data.Common.DbProviderFactory dbProviderFactory, System.Configuration.ConnectionStringSettings connectionString)
            {
                System.Data.Common.DbConnection connection = dbProviderFactory.CreateConnection();
                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.SqlClient.SqlConnectionStringBuilder csb = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString.ConnectionString);
                    connection.ConnectionString = csb.ConnectionString;
                }
                else
                {
                    connection.ConnectionString = connectionString.ConnectionString;
                }
                connection.Open();
                return connection;
            }

            public static System.Data.Common.DbConnection CreateDbConnection(System.Data.SqlClient.SqlConnectionStringBuilder csb)
            {
                System.Data.Common.DbConnection connection = new System.Data.SqlClient.SqlConnection();
                connection.ConnectionString = csb.ConnectionString;
                connection.Open();
                return connection;
            }

            public static System.Data.Common.DbProviderFactory CreateDbProviderFactory(System.Data.Common.DbConnection connection)
            {
                string strProviderName = GetProviderName(connection);
                return CreateDbProviderFactory(strProviderName);
            }

            public static System.Data.Common.DbProviderFactory CreateDbProviderFactory(System.Configuration.ConnectionStringSettings connectionString)
            {
                string strProviderName = GetProviderName(connectionString);
                return CreateDbProviderFactory(strProviderName);
            }

            public static System.Data.Common.DbProviderFactory CreateDbProviderFactory(string providerName)
            {
                switch (providerName)
                {
                    default:
                        return System.Data.Common.DbProviderFactories.GetFactory(providerName);
                }
            }

            public static System.Data.DataSet Execute(System.Data.Common.DbConnection connection, string sqlCommand)
            {
                System.Data.DataSet ds = new System.Data.DataSet();

                using (System.Data.Common.DbCommand command = Database.CreateDbCommand(connection))
                {
                    command.CommandText = sqlCommand;
                    using (System.Data.Common.DbDataReader reader = command.ExecuteReader())
                    {
                        ds.Load(reader, LoadOption.OverwriteChanges, "Table");
                    }
                }

                return ds;
            }

            public static System.Data.DataSet Execute(System.Configuration.ConnectionStringSettings connectionString, string sqlCommand)
            {
                System.Data.DataSet ds = new System.Data.DataSet();

                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    System.Data.Common.DbProviderFactory factory = Database.CreateDbProviderFactory(connectionString);
                    using (System.Data.Common.DbConnection connection = Database.CreateDbConnection(factory, connectionString))
                    {
                        ds = Execute(connection, sqlCommand);
                    }
                }

                return ds;
            }

            public static void ExecuteFile(System.Data.Common.DbConnection connection, string sqlCommand)
            {
                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex("^[\\s]*GO[^a-zA-Z0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                    foreach (string commandText in regEx.Split(sqlCommand))
                    {
                        System.Data.Common.DbConnection commandConnection = connection;

                        if (commandText.IndexOf("ALTER DATABASE", StringComparison.InvariantCultureIgnoreCase) != -1 && Data.DbTransactionScope.Current != null && Data.DbTransactionScope.Current.Connection == connection)
                        {

                            //Cannot Alter Database inside Transaction
                            commandConnection = CloneDbConnection(connection);
                        }

                        if (!(string.IsNullOrEmpty(commandText.Trim())))
                        {
                            ExecuteNonQuery(commandConnection, commandText);
                        }
                    }
                }
            }

            public static void ExecuteFile(System.Configuration.ConnectionStringSettings connectionString, string sqlCommand)
            {
                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    System.Data.Common.DbProviderFactory factory = Database.CreateDbProviderFactory(connectionString);
                    using (System.Data.Common.DbConnection connection = Database.CreateDbConnection(factory, connectionString))
                    {
                        ExecuteFile(connection, sqlCommand);
                    }
                }
            }

            public static void ExecuteNonQuery(System.Data.Common.DbConnection connection, string sqlCommand)
            {
                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    using (System.Data.Common.DbCommand command = Database.CreateDbCommand(connection))
                    {
                        command.CommandText = sqlCommand;
                        command.ExecuteNonQuery();
                    }
                }
            }

            public static void ExecuteNonQuery(System.Configuration.ConnectionStringSettings connectionString, string sqlCommand)
            {
                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    System.Data.Common.DbProviderFactory factory = Database.CreateDbProviderFactory(connectionString);
                    using (System.Data.Common.DbConnection connection = Database.CreateDbConnection(factory, connectionString))
                    {
                        ExecuteNonQuery(connection, sqlCommand);
                    }
                }
            }

            public static object ExecuteScalar(System.Configuration.ConnectionStringSettings connectionString, string sqlCommand)
            {
                object returnValue = null;
                if (!(string.IsNullOrEmpty(sqlCommand)))
                {
                    System.Data.Common.DbProviderFactory factory = Database.CreateDbProviderFactory(connectionString);

                    using (System.Data.Common.DbConnection connection = Database.CreateDbConnection(factory, connectionString))
                    {
                        using (System.Data.Common.DbCommand command = Database.CreateDbCommand(connection))
                        {
                            command.CommandText = sqlCommand;

                            returnValue = command.ExecuteScalar();

                            if (returnValue == DBNull.Value)
                            {
                                returnValue = null;
                            }
                        }
                    }
                }
                return returnValue;
            }

            private static List<Models.ColumnModel> GetColumns(DataTable dataTable)
            {
                List<Models.ColumnModel> list = new List<Models.ColumnModel>();
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    Models.ColumnModel column = new Models.ColumnModel();
                    column.Initialize(row);
                    list.Add(column);
                }
                return list;
            }

            private static Int32 GetServerVersion(Models.DatabaseType databaseType, System.Configuration.ConnectionStringSettings connectionString)
            {
                Int32 intVersion = -1;

                switch (databaseType)
                {
                    case Models.DatabaseType.MicrosoftSQLServer:
                        try
                        {
                            intVersion = 10;

                            var strVersion = Convert.ToString(ExecuteScalar(connectionString, "SELECT @@VERSION"));

                            if (strVersion.StartsWith("Microsoft SQL Server  2000"))
                            {
                                intVersion = 8;
                            }
                            else if (strVersion.StartsWith("Microsoft SQL Server 2005"))
                            {
                                intVersion = 9;
                            }
                            else if (strVersion.StartsWith("Microsoft SQL Server 2008"))
                            {
                                intVersion = 10;
                            }
                            else if (strVersion.StartsWith("Microsoft SQL Server 2012"))
                            {
                                intVersion = 11;
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        break;
                }

                return intVersion;
            }

            public static List<Models.ColumnModel> GetTableColumns(System.Configuration.ConnectionStringSettings connectionString)
            {
                var databaseType = GetDatabaseType(connectionString);

                List<Models.ColumnModel> list = new List<Models.ColumnModel>();

                string strSelect = Database.GetSysTableColumnsSelect(databaseType, GetServerVersion(databaseType, connectionString));

                System.Data.DataTable dataTable = null;

                if (!(string.IsNullOrEmpty(strSelect)))
                {
                    try
                    {
                        DataSet ds = Execute(connectionString, strSelect);
                        dataTable = ds.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        strSelect = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(strSelect))
                {
                    using (var conn = CreateDbConnection(connectionString))
                    {
                        var dataTypes = conn.GetSchema("DataTypes");
                        dataTable = conn.GetSchema("Columns");
                        UpdateSchemaColumns(dataTable, dataTypes);
                    }
                }

                if (dataTable != null)
                {
                    list = GetColumns(dataTable);
                }

                return list;
            }

            protected static void UpdateSchemaColumns(DataTable table, DataTable dataTypes)
            {
                if (table.Columns.Contains("ordinal_position"))
                {
                    table.Columns["ordinal_position"].ColumnName = "column_id";
                }

                if (table.Columns.Contains("type_name") && !(table.Columns.Contains("column_type")))
                {
                    table.Columns["type_name"].ColumnName = "column_type";
                }

                if (table.Columns.Contains("data_type") && !(table.Columns.Contains("column_type")))
                {
                    table.Columns.Add("column_type", typeof(string));
                    table.Columns.Add("is_identity", typeof(bool));
                    foreach (var row in table.Rows.OfType<System.Data.DataRow>())
                    {
                        string strColumnType = Convert.ToString(row["data_type"]);

                        if (!(string.IsNullOrEmpty(strColumnType)) && strColumnType.ToUpper().EndsWith(" IDENTITY"))
                        {
                            strColumnType = strColumnType.Substring(0, strColumnType.Length - " IDENTITY".Length);
                            row["is_identity"] = true;
                        }
                        else
                        {
                            row["is_identity"] = false;
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

                //"is_nullable"

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
                                    catch (ArgumentException ex)
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

            public static List<Models.ColumnModel> GetViewColumns(System.Configuration.ConnectionStringSettings connectionString)
            {
                var databaseType = GetDatabaseType(connectionString);

                List<Models.ColumnModel> list = new List<Models.ColumnModel>();

                string strSelect = Database.GetSysViewColumnsSelect(databaseType);

                System.Data.DataTable dataTable = null;

                if (string.IsNullOrEmpty(strSelect))
                {
                    using (var conn = CreateDbConnection(connectionString))
                    {
                        var dataTypes = conn.GetSchema("DataTypes");
                        dataTable = conn.GetSchema("Columns");
                        UpdateSchemaColumns(dataTable, dataTypes);
                    }
                }
                else
                {
                    DataSet ds = Execute(connectionString, strSelect);
                    dataTable = ds.Tables[0];
                }

                list = GetColumns(dataTable);

                return list;
            }

            public static System.Configuration.ConnectionStringSettings GetConnectionStringSetting(string connectionStringName)
            {
                return Configuration.ConnectionInfo.GetConnectionStringSetting(connectionStringName);
            }

            public static Models.DatabaseType GetDatabaseType(System.Data.Common.DbConnection connection)
            {
                if ((connection) is System.Data.Odbc.OdbcConnection)
                {
                    return Models.DatabaseType.Odbc;
                }
                else if (connection.GetType().FullName.StartsWith("System.Data.Oracle", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Models.DatabaseType.Oracle;
                }
                else if (connection.GetType().FullName.StartsWith("System.Data.SqlServerCe", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Models.DatabaseType.MicrosoftSQLServerCompact;
                }
                else if (connection.GetType().FullName.StartsWith("System.Data.Ole", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (connection.ConnectionString.Contains("Microsoft.ACE"))
                    {
                        return Models.DatabaseType.AccessOLE;
                    }
                    return Models.DatabaseType.OLE;
                }
                return Models.DatabaseType.MicrosoftSQLServer;
            }

            public static Models.DatabaseType GetDatabaseType(System.Configuration.ConnectionStringSettings connectionString)
            {
                switch (GetProviderName(connectionString).ToLower())
                {
                    case "system.data.odbc":
                        return Models.DatabaseType.Odbc;
                    case "system.data.oracle":
                        return Models.DatabaseType.Oracle;
                    case "system.data.sqlserverce.3.5":
                        return Models.DatabaseType.MicrosoftSQLServerCompact;
                    case "system.data.sqlclient":
                        return Models.DatabaseType.MicrosoftSQLServer;
                    case "mysql.data.mysqlclient":
                        return Models.DatabaseType.MySql;
                    case "system.data.oledb":
                        if (connectionString.ConnectionString.Contains("Microsoft.ACE"))
                        {
                            return Models.DatabaseType.AccessOLE;
                        }
                        return Models.DatabaseType.OLE;
                }
                return Models.DatabaseType.MicrosoftSQLServer;
            }

            public static System.Data.DbType GetDBType(string typeName)
            {
                switch (typeName.ToUpper())
                {
                    case "BIT":
                        return System.Data.DbType.Boolean;
                    case "CHAR":
                        return System.Data.DbType.StringFixedLength;
                    case "DATETIME":
                        return System.Data.DbType.DateTime;
                    case "DECIMAL":
                        return System.Data.DbType.Decimal;
                    case "IMAGE":
                    case "BINARY":
                    case "VARBINARY":
                        return System.Data.DbType.Binary;
                    case "INT":
                        return System.Data.DbType.Int32;
                    case "SMALLINT":
                        return System.Data.DbType.Int16;
                    case "BIGINT":
                        return DbType.Int64;
                    case "UNIQUEIDENTIFIER":
                        return System.Data.DbType.Guid;
                    case "VARCHAR":
                        return System.Data.DbType.String;
                    case "TIME":
                        return DbType.Time;
                }
                return System.Data.DbType.String;
            }

            public static IList<Models.DefinitionModel> GetDefinitions(System.Configuration.ConnectionStringSettings connectionString)
            {
                List<Models.DefinitionModel> list = new List<Models.DefinitionModel>();

                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.DataSet dsDefinitions = Database.Execute(connectionString, "SELECT sysobjects.name, sysobjects.xtype, ISNULL(sys.sql_modules.definition, sys.system_sql_modules.definition) AS definition FROM sysobjects INNER JOIN sys.schemas ON sysobjects.uid = sys.schemas.schema_id LEFT OUTER JOIN sys.sql_modules ON sys.sql_modules.object_id = sysobjects.id LEFT OUTER JOIN sys.system_sql_modules ON sys.system_sql_modules.object_id = sysobjects.id WHERE sysobjects.xtype IN ('P', 'V', 'FN', 'IF') AND sysobjects.category = 0 AND sysobjects.name NOT LIKE '%diagram%' AND sysobjects.name NOT LIKE '%aspnet%' AND sys.schemas.name = 'dbo' ORDER BY sysobjects.xtype, sysobjects.name");
                    System.Data.DataSet dsDependencies = Database.Execute(connectionString, "SELECT sysobjects.name, r.referencing_entity_name FROM sysobjects INNER JOIN sys.schemas ON sysobjects.uid = sys.schemas.schema_id CROSS APPLY sys.dm_sql_referencing_entities(sys.schemas.name + '.' + sysobjects.name, 'OBJECT') r WHERE sysobjects.xtype IN ('P', 'V', 'FN', 'IF') AND sysobjects.category = 0 AND sysobjects.name NOT LIKE '%diagram%' AND sysobjects.name NOT LIKE '%aspnet%' AND sys.schemas.name = 'dbo' ORDER BY sysobjects.name, r.referencing_entity_name");
                    list = (
                            from i in dsDefinitions.Tables[0].Rows.OfType<System.Data.DataRow>()
                            select new Models.DefinitionModel { Definition = i["definition"].ToString(), DefinitionName = i["name"].ToString(), XType = i["xtype"].ToString().Trim() }
                            ).ToList();

                    VerifyDependencies(list, dsDependencies);
                }
                return list;
            }

            private static void VerifyDependencies(IList<Models.DefinitionModel> list, System.Data.DataSet dependencies)
            {
                bool blnListChanged = false;
                foreach (System.Data.DataRow row in dependencies.Tables[0].Rows)
                {
                    string strName = row["name"].ToString();
                    string strReferencingName = row["referencing_entity_name"].ToString().Trim();

                    var nameDefinition = (
                        from i in list
                        where i.DefinitionName.EqualsIgnoreCase(strName)
                        select i).FirstOrDefault();
                    var referenceDefinition = (
                        from i in list
                        where i.DefinitionName.EqualsIgnoreCase(strReferencingName)
                        select i).FirstOrDefault();

                    if (nameDefinition != null && referenceDefinition != null)
                    {

                        int nameIndex = list.IndexOf(nameDefinition);
                        int referenceIndex = list.IndexOf(referenceDefinition);

                        if (nameIndex > referenceIndex)
                        {
                            list.Remove(nameDefinition);
                            list.Insert(list.IndexOf(referenceDefinition), nameDefinition);
                            blnListChanged = true;
                            break;
                        }
                    }
                }
                if (blnListChanged)
                {
                    VerifyDependencies(list, dependencies);
                }
            }

            public static IList<Models.ForeignKeyModel> GetForeignKeys(System.Configuration.ConnectionStringSettings connectionString, IList<string> tables)
            {
                List<Models.ForeignKeyModel> list = new List<Models.ForeignKeyModel>();

                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.DataSet dsForeignKeys = Database.Execute(connectionString, "SELECT sys.tables.name AS table_name, sys.foreign_keys.name AS foreign_key_name, parent_columns.name AS column_name, referenced_tables.name AS referenced_table_name, referenced_columns.name AS referenced_column_name, sys.foreign_keys.is_not_for_replication, sys.foreign_keys.delete_referential_action_desc delete_action, sys.foreign_keys.update_referential_action_desc update_action FROM sys.foreign_keys INNER JOIN sys.tables ON sys.foreign_keys.parent_object_id = sys.tables.object_id INNER JOIN sys.tables AS referenced_tables ON sys.foreign_keys.referenced_object_id = referenced_tables.object_id INNER JOIN sys.foreign_key_columns ON sys.foreign_keys.object_id = sys.foreign_key_columns.constraint_object_id INNER JOIN sys.columns AS parent_columns ON sys.foreign_key_columns.parent_object_id = parent_columns.object_id AND sys.foreign_key_columns.parent_column_id = parent_columns.column_id INNER JOIN sys.columns AS referenced_columns ON sys.foreign_key_columns.referenced_object_id = referenced_columns.object_id AND sys.foreign_key_columns.referenced_column_id = referenced_columns.column_id WHERE sys.tables.name NOT LIKE 'sys%' ORDER BY sys.tables.name, sys.foreign_keys.name, sys.foreign_key_columns.constraint_column_id");

                    foreach (var tableGroup in (
                        from i in dsForeignKeys.Tables[0].Rows.Cast<System.Data.DataRow>()
                        group i by new { TableName = i["table_name"].ToString(), ForeignKeyName = i["foreign_key_name"].ToString() } into g
                        select new { TableName = g.Key.TableName, ForeignKeyName = g.Key.ForeignKeyName, Items = g.ToList() }))
                    {
                        if (ContainsTable(tables, tableGroup.TableName))
                        {
                            System.Data.DataRow summaryRow = tableGroup.Items[0];

                            Models.ForeignKeyModel foreignKey = new Models.ForeignKeyModel { ForeignKeyName = tableGroup.ForeignKeyName, TableName = tableGroup.TableName, ReferencedTableName = summaryRow["referenced_table_name"].ToString(), IsNotForReplication = Convert.ToBoolean(summaryRow["is_not_for_replication"]), DeleteAction = summaryRow["delete_action"].ToString().Replace("_", " "), UpdateAction = summaryRow["update_action"].ToString().Replace("_", " ") };
                            list.Add(foreignKey);

                            foreach (System.Data.DataRow detailRow in tableGroup.Items)
                            {
                                foreignKey.Detail.Add(new Models.ForeignKeyDetailModel { Column = detailRow["column_name"].ToString(), ReferencedColumn = detailRow["referenced_column_name"].ToString() });
                            }
                        }
                    }
                }
                

                return list;
            }

            public static IList<Models.IndexModel> GetIndexes(System.Configuration.ConnectionStringSettings connectionString, IList<string> tables)
            {
                IList<Models.IndexModel> list = new List<Models.IndexModel>();

                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.DataSet dsIndexes = Database.Execute(connectionString, "SELECT sys.tables.name AS table_name, sys.indexes.name AS index_name, sys.indexes.type_desc index_type, sys.indexes.is_unique, sys.indexes.fill_factor, sys.columns.name AS column_name, sys.index_columns.is_included_column, sys.index_columns.is_descending_key FROM sys.indexes INNER JOIN sys.tables ON sys.indexes.object_id = sys.tables.object_id INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id INNER JOIN sys.columns ON sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id WHERE sys.tables.name NOT LIKE 'sys%' AND sys.indexes.name NOT LIKE 'MSmerge_index%' AND sys.indexes.name IS NOT NULL AND sys.indexes.is_primary_key = 0 ORDER BY sys.tables.name, sys.indexes.name, sys.index_columns.key_ordinal, sys.index_columns.index_column_id");
                    list = GetIndexes(dsIndexes, tables, false);
                }

                return list;
            }

            private static bool ContainsTable(IList<string> tables, string table)
            {
                return (from i in tables where i.EqualsIgnoreCase(table) select i).Any();
            }

            private static IList<Models.IndexModel> GetIndexes(System.Data.DataSet indexes, IList<string> tables, bool isPrimaryKey)
            {
                List<Models.IndexModel> list = new List<Models.IndexModel>();

                foreach (var indexGroup in (
                    from i in indexes.Tables[0].Rows.Cast<System.Data.DataRow>()
                    group i by new { IndexName = i["index_name"].ToString(), TableName = i["table_name"].ToString() } into g
                    select new { IndexName = g.Key.IndexName, TableName = g.Key.TableName, Items = g.ToList() }))
                {
                    if (ContainsTable(tables, indexGroup.TableName))
                    {
                        System.Data.DataRow summaryRow = indexGroup.Items[0];
                        Models.IndexModel index = new Models.IndexModel { TableName = indexGroup.TableName, IndexName = indexGroup.IndexName, IndexType = summaryRow["index_type"].ToString(), IsUnique = Convert.ToBoolean(summaryRow["is_unique"]), FillFactor = Convert.ToInt32(summaryRow["fill_factor"]), IsPrimaryKey = isPrimaryKey };

                        foreach (var detialRow in indexGroup.Items)
                        {
                            bool blnIsDescending = Convert.ToBoolean(detialRow["is_descending_key"]);
                            bool blnIsIncludeColumn = Convert.ToBoolean(detialRow["is_included_column"]);
                            string strColumnName = detialRow["column_name"].ToString();

                            if (blnIsIncludeColumn)
                            {
                                index.IncludeColumns.Add(strColumnName);
                            }
                            else if (blnIsDescending)
                            {
                                index.Columns.Add(strColumnName + " DESC");
                            }
                            else
                            {
                                index.Columns.Add(strColumnName);
                            }
                        }

                        list.Add(index);

                    }
                }

                return list;
            }

            public static IList<Models.IndexModel> GetPrimaryKeys(System.Configuration.ConnectionStringSettings connectionString, IList<string> tables)
            {
                IList<Models.IndexModel> list = new List<Models.IndexModel>();

                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.DataSet dsPrimaryKeys = Database.Execute(connectionString, "SELECT sys.tables.name AS table_name, sys.indexes.name AS index_name, sys.indexes.type_desc index_type, sys.indexes.is_unique, sys.indexes.fill_factor, sys.columns.name AS column_name, sys.index_columns.is_included_column, sys.index_columns.is_descending_key FROM sys.indexes INNER JOIN sys.tables ON sys.indexes.object_id = sys.tables.object_id INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id INNER JOIN sys.columns ON sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id WHERE sys.tables.name NOT LIKE 'sys%' AND sys.indexes.name IS NOT NULL AND sys.indexes.is_primary_key = 1 ORDER BY sys.tables.name, sys.indexes.name, sys.index_columns.key_ordinal, sys.index_columns.index_column_id");
                    list = GetIndexes(dsPrimaryKeys, tables, false);
                }

                return list;
            }

            public static string GetProviderName(System.Data.Common.DbConnection connection)
            {
                switch (GetDatabaseType(connection))
                {
                    case Models.DatabaseType.Oracle:
                        return "System.Data.Oracle";
                    case Models.DatabaseType.Odbc:
                        return "System.Data.Odbc";
                    case Models.DatabaseType.MicrosoftSQLServerCompact:
                        return "System.Data.SqlServerCe.3.5";
                    case Models.DatabaseType.OLE:
                    case Models.DatabaseType.AccessOLE:
                        return "System.Data.OleDb";
                }
                return "System.Data.SqlClient";
            }

            public static string GetProviderName(System.Configuration.ConnectionStringSettings connectionString)
            {
                return connectionString.ProviderName;
            }

            public static string GetSysTableColumnsSelect(Models.DatabaseType type, Int32 serverVersion)
            {
                switch (type)
                {
                    case Models.DatabaseType.Odbc:
                        return "SELECT systable.table_name AS table_name, syscolumn.column_name AS column_name, UPPER(ISNULL(sysusertype.type_name, sysdomain.domain_name)) AS column_type, syscolumn.width AS \"precision\", syscolumn.scale, CASE syscolumn.nulls WHEN 'Y' THEN 1 ELSE 0 END AS is_nullable, CASE syscolumn.\"default\" WHEN 'autoincrement' THEN 1 ElSE 0 END AS is_identity, 0 AS is_computed, '' AS computed_definition, syscolumn.column_id, 0 is_primary_key FROM systable INNER JOIN syscolumn ON systable.table_id = syscolumn.table_id INNER JOIN sysdomain ON syscolumn.domain_id = sysdomain.domain_id LEFT OUTER JOIN sysusertype ON syscolumn.user_type = sysusertype.type_id ORDER BY systable.table_name, syscolumn.column_id";
                    case Models.DatabaseType.MicrosoftSQLServer:
                        if (serverVersion <= 8)
                        {
                            return "SELECT sysobjects.name AS table_name, syscolumns.name AS column_name, UPPER(systypes.name) AS column_type, CASE ISNULL(syscolumns.prec, 0) WHEN 0 THEN syscolumns.length ELSE ISNULL(syscolumns.prec, 0) END AS [precision], ISNULL(syscolumns.scale, 0) AS scale, syscolumns.isnullable is_nullable, CASE WHEN autoval IS NULL THEN 0 ELSE 1 END is_identity, syscolumns.iscomputed is_computed, ISNULL(NULL, '') computed_definition, syscolumns.colorder column_id, ISNULL((SELECT 1 FROM information_schema.key_column_usage WHERE TABLE_NAME = sysobjects.name AND COLUMN_NAME = syscolumns.name AND CONSTRAINT_SCHEMA = 'dbo' AND CONSTRAINT_NAME LIKE 'pk%'), 0) is_primary_key FROM sysobjects INNER JOIN syscolumns ON sysobjects.id = syscolumns.id INNER JOIN systypes ON syscolumns.type = systypes.type AND syscolumns.usertype = systypes.usertype INNER JOIN sysusers ON sysobjects.uid = sysusers.uid WHERE sysobjects.type = 'U' AND sysobjects.name NOT LIKE 'sys%' AND sysusers.name = 'dbo' ORDER BY sysobjects.name,  syscolumns.colorder";
                        }
                        else
                        {
                            return "SELECT sys.tables.name AS table_name, sys.columns.name AS column_name, UPPER(sys.types.name) AS column_type, CASE ISNULL(sys.columns.precision, 0) WHEN 0 THEN sys.columns.max_length ELSE ISNULL(sys.columns.precision, 0) END AS precision, ISNULL(sys.columns.scale, 0) AS scale, sys.columns.is_nullable, sys.columns.is_identity, sys.columns.is_computed, ISNULL(sys.computed_columns.definition, '') computed_definition, sys.columns.column_id, ISNULL((SELECT 1 FROM sys.indexes INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id WHERE sys.indexes.is_primary_key = 1 AND sys.indexes.object_id = sys.tables.object_id AND sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id), 0) is_primary_key FROM sys.tables INNER JOIN sys.schemas on sys.tables.schema_id = sys.schemas.schema_id INNER JOIN sys.columns ON sys.tables.object_id = sys.columns.object_id INNER JOIN sys.types ON sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN sys.computed_columns ON sys.columns.object_id = sys.computed_columns.object_id AND sys.columns.column_id = sys.computed_columns.column_id WHERE sys.tables.name NOT LIKE 'sys%' AND (sys.schemas.name = 'dbo') ORDER BY sys.tables.name, sys.columns.column_id";
                        }
                        break;
                }
                return "";
            }

            public static string GetSysTablesSelect(Models.DatabaseType type, Int32 serverVersion)
            {
                switch (type)
                {
                    case Models.DatabaseType.Odbc:
                        return "SELECT systable.table_name AS table_name FROM systable WHERE systable.creator = 1 AND table_type = 'BASE' ORDER BY systable.table_name";
                    case Models.DatabaseType.MicrosoftSQLServer:
                        if (serverVersion <= 8)
                        {
                            return "SELECT sysobjects.name AS table_name FROM sysobjects INNER JOIN sysusers ON sysobjects.uid = sysusers.uid WHERE sysobjects.type = 'U' AND sysobjects.name NOT LIKE 'sys%' AND sysusers.name = 'dbo' ORDER BY sysobjects.name";
                        }
                        else
                        {
                            return "SELECT sys.tables.name AS table_name FROM sys.tables INNER JOIN sys.schemas ON sys.tables.schema_id = sys.schemas.schema_id WHERE sys.tables.name NOT LIKE 'sys%' AND sys.tables.name NOT LIKE '%aspnet%' AND sys.schemas.name = 'dbo' ORDER BY sys.tables.name";
                        }
                        break;
                }
                return "";
            }

            public static string GetSysViewColumnsSelect(Models.DatabaseType type)
            {
                switch (type)
                {
                    case Models.DatabaseType.Odbc:
                        return "SELECT systable.table_name AS table_name, syscolumn.column_name AS column_name, UPPER(ISNULL(sysusertype.type_name, sysdomain.domain_name)) AS column_type, syscolumn.width AS \"precision\", syscolumn.scale, CASE syscolumn.nulls WHEN 'Y' THEN 1 ELSE 0 END AS is_nullable, CASE syscolumn.\"default\" WHEN 'autoincrement' THEN 1 ElSE 0 END AS is_identity, 0 AS is_computed, '' AS computed_definition, syscolumn.column_id, 0 is_primary_key FROM systable INNER JOIN syscolumn ON systable.table_id = syscolumn.table_id INNER JOIN sysdomain ON syscolumn.domain_id = sysdomain.domain_id LEFT OUTER JOIN sysusertype ON syscolumn.user_type = sysusertype.type_id ORDER BY systable.table_name, syscolumn.column_id";
                    case Models.DatabaseType.MicrosoftSQLServer:
                        return "SELECT sys.views.name AS table_name, sys.columns.name AS column_name, UPPER(sys.types.name) AS column_type, CASE ISNULL(sys.columns.precision, 0) WHEN 0 THEN sys.columns.max_length ELSE ISNULL(sys.columns.precision, 0) END AS precision, ISNULL(sys.columns.scale, 0) AS scale, sys.columns.is_nullable, sys.columns.is_identity, sys.columns.is_computed, ISNULL(sys.computed_columns.definition, '') computed_definition, sys.columns.column_id, ISNULL((SELECT 1 FROM sys.indexes INNER JOIN sys.index_columns ON sys.indexes.object_id = sys.index_columns.object_id AND sys.indexes.index_id = sys.index_columns.index_id WHERE sys.indexes.is_primary_key = 1 AND sys.indexes.object_id = sys.views.object_id AND sys.index_columns.object_id = sys.columns.object_id AND sys.index_columns.column_id = sys.columns.column_id), 0) is_primary_key FROM sys.views INNER JOIN sys.schemas on sys.views.schema_id = sys.schemas.schema_id INNER JOIN sys.columns ON sys.views.object_id = sys.columns.object_id INNER JOIN sys.types ON sys.columns.user_type_id = sys.types.user_type_id LEFT OUTER JOIN sys.computed_columns ON sys.columns.object_id = sys.computed_columns.object_id AND sys.columns.column_id = sys.computed_columns.column_id WHERE sys.views.name NOT LIKE 'sys%' AND (sys.schemas.name = 'dbo') ORDER BY sys.views.name, sys.columns.column_id";
                }
                return "";
            }

            public static string GetSysViewsSelect(Models.DatabaseType type)
            {
                switch (type)
                {
                    case Models.DatabaseType.Odbc:
                        return "SELECT table_name table_name FROM systable WHERE table_type = 'VIEW' AND table_name NOT LIKE 'sys%' ORDER BY table_name";
                    case Models.DatabaseType.MicrosoftSQLServer:
                        return "SELECT sys.views.name AS table_name FROM sys.views INNER JOIN sys.schemas ON sys.views.schema_id = sys.schemas.schema_id WHERE sys.views.name NOT LIKE 'sys%' AND sys.views.name NOT LIKE '%aspnet%' AND sys.schemas.name = 'dbo' ORDER BY sys.views.name";
                }
                return "";
            }

            private static List<Models.TableModel> GetTables(DataTable dataTable, IList<Models.ColumnModel> columns)
            {
                List<Models.TableModel> list = new List<Models.TableModel>();

                List<string> tables = new List<string>();

                Dictionary<string, IList<Models.ColumnModel>> columnIndex = new Dictionary<string, IList<Models.ColumnModel>>();

                foreach (var columnGroup in (
                    from i in columns
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g.ToList() }))
                {
                    columnIndex.Add(columnGroup.TableName.ToUpper(), columnGroup.Items);
                }

                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    Models.TableModel table = new Models.TableModel();
                    table.Initialize(row);
                    if (!(tables.Contains(table.TableName.ToUpper())))
                    {
                        if (columnIndex.ContainsKey(table.TableName.ToUpper()))
                        {
                            var tableColumns = columnIndex[table.TableName.ToUpper()];
                            table.Initialize(tableColumns);
                        }
                        tables.Add(table.TableName.ToUpper());
                        list.Add(table);
                    }
                }

                return list;
            }

            public static List<Models.TableModel> GetTables(System.Configuration.ConnectionStringSettings connectionString, IList<Models.ColumnModel> columns, bool withBackup = false)
            {
                System.Data.DataTable dataTable = null;

                var databaseType = GetDatabaseType(connectionString);

                string strSelect = Database.GetSysTablesSelect(databaseType, GetServerVersion(databaseType, connectionString));

                if (!(string.IsNullOrEmpty(strSelect)))
                {
                    try
                    {
                        var ds = Execute(connectionString, strSelect);
                        dataTable = ds.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        strSelect = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(strSelect))
                {
                    using (var conn = CreateDbConnection(connectionString))
                    {
                        dataTable = conn.GetSchema("Tables");
                    }
                }

                List<Models.TableModel> list = new List<Models.TableModel>();

                if (dataTable != null)
                {
                    list = GetTables(dataTable, columns);

                    if (list.Count == 0 && databaseType == Models.DatabaseType.Odbc)
                    {

                        list = GetViews(connectionString, columns);
                    }

                    //Remove PowerBuilder Tables
                    list = (
                        from i in list
                        where !(i.TableName.StartsWith("pbcat", StringComparison.InvariantCultureIgnoreCase))
                        select i).ToList();

                    //Remove Access Sys Tables
                    list = (
                        from i in list
                        where !(i.TableName.StartsWith("MSys", StringComparison.InvariantCultureIgnoreCase))
                        select i).ToList();

                    //Remove Access Sys Tables
                    list = (
                        from i in list
                        where !(i.TableName.StartsWith("ISYS", StringComparison.InvariantCultureIgnoreCase))
                        select i).ToList();

                    if (!withBackup)
                    {
                        list = (
                            from i in list
                            where !(i.TableName.EndsWith("_backup", StringComparison.InvariantCultureIgnoreCase)) && !(i.TableName.EndsWith("_old", StringComparison.InvariantCultureIgnoreCase))
                            select i).ToList();
                    }
                }

                return list;
            }

            public static List<Models.TableModel> GetViews(System.Configuration.ConnectionStringSettings connectionString, IList<Models.ColumnModel> columns)
            {
                System.Data.DataTable dataTable = null;

                var databaseType = GetDatabaseType(connectionString);

                string strSelect = Database.GetSysViewsSelect(databaseType);

                if (string.IsNullOrEmpty(strSelect))
                {
                    using (var conn = CreateDbConnection(connectionString))
                    {
                        dataTable = conn.GetSchema("Views");
                    }
                }
                else
                {
                    var ds = Execute(connectionString, strSelect);
                    dataTable = ds.Tables[0];
                }

                var list = GetTables(dataTable, columns);

                return list;
            }

            public static IList<Models.TriggerModel> GetTriggers(System.Configuration.ConnectionStringSettings connectionString, IList<string> tables, string objectFilter)
            {
                List<Models.TriggerModel> list = new List<Models.TriggerModel>();

                if (GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    System.Data.DataSet dsTriggers = Database.Execute(connectionString, "SELECT sys.tables.name AS table_name, sys.triggers.name AS trigger_name, ISNULL(sys.sql_modules.definition, sys.system_sql_modules.definition) AS definition FROM sys.triggers INNER JOIN sys.tables ON sys.triggers.parent_id = sys.tables.object_id INNER JOIN sys.schemas ON sys.tables.schema_id = sys.schemas.schema_id LEFT OUTER JOIN sys.sql_modules ON sys.sql_modules.object_id = sys.triggers.object_id LEFT OUTER JOIN sys.system_sql_modules ON sys.system_sql_modules.object_id = sys.triggers.object_id WHERE sys.tables.name NOT LIKE 'sys%' AND sys.tables.name NOT LIKE '%aspnet%' AND sys.schemas.name = 'dbo' ORDER BY sys.tables.name, sys.triggers.name");

                    foreach (System.Data.DataRow detailRow in dsTriggers.Tables[0].Rows)
                    {
                        string strTableName = detailRow["table_name"].ToString();
                        string strTriggerName = detailRow["trigger_name"].ToString();
                        string strDefinition = detailRow["definition"].ToString();

                        if (ContainsTable(tables, strTableName) || (!(string.IsNullOrEmpty(objectFilter)) && strTriggerName.ToLower().Contains(objectFilter)))
                        {
                            list.Add(new Models.TriggerModel { TableName = strTableName, TriggerName = strTriggerName, Definition = strDefinition });
                        }
                    }
                }

                return list;
            }

            public static bool IsDatabaseEmpty(System.Configuration.ConnectionStringSettings connectionString)
            {
                Models.DatabaseModel database = new Models.DatabaseModel(connectionString);
                return database.Tables.Count == 0;
            }

            #endregion

        }
    }





}