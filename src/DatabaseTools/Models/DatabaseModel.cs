
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DatabaseTools
{
    namespace Models
    {
        public class DatabaseModel
        {

            public DatabaseModel(System.Configuration.ConnectionStringSettings connectionString) : this(connectionString, Processes.Database.GetDatabaseType(connectionString))
            {

            }

            public DatabaseModel(System.Configuration.ConnectionStringSettings connectionString, Models.DatabaseType targetDatabseType)
            {
                this._connectionString = connectionString;
                this._databaseType = Processes.Database.GetDatabaseType(connectionString);
                switch (this.DatabaseType)
                {
                    case Models.DatabaseType.MicrosoftSQLServer:
                        System.Data.SqlClient.SqlConnectionStringBuilder sqlCommandBuilder = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString.ConnectionString);
                        this._databaseName = sqlCommandBuilder.InitialCatalog;
                        break;
                }

                switch (targetDatabseType)
                {
                    case Models.DatabaseType.AccessOLE:
                    case Models.DatabaseType.OLE:
                    case Models.DatabaseType.Odbc:
                        this.QuoteCharacterStart = "\"";
                        this.QuoteCharacterEnd = "\"";
                        break;
                    case Models.DatabaseType.MySql:
                        this.QuoteCharacterStart = "`";
                        this.QuoteCharacterEnd = "`";
                        break;
                    case Models.DatabaseType.MicrosoftSQLServer:
                        this.QuoteCharacterStart = "[";
                        this.QuoteCharacterEnd = "]";
                        break;
                }

            }

            #region Properties

            private System.Configuration.ConnectionStringSettings _connectionString;
            public System.Configuration.ConnectionStringSettings ConnectionString
            {
                get
                {
                    return this._connectionString;
                }
            }

            private string _databaseName;
            public string DatabaseName
            {
                get
                {
                    return this._databaseName;
                }
            }

            private DatabaseType _databaseType;
            public DatabaseType DatabaseType
            {
                get
                {
                    return this._databaseType;
                }
            }

            private IList<Models.DefinitionModel> _definitions;
            public IList<Models.DefinitionModel> Definitions
            {
                get
                {
                    if (this._definitions == null)
                    {
                        this._definitions = Processes.Database.GetDefinitions(this.ConnectionString);
                    }

                    if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                    {
                        return (
                            from i in this._definitions
                            where i.DefinitionName.ToLower().Contains(this.ObjectFilter.ToLower())
                            select i).ToList();
                    }

                    return this._definitions;
                }
            }

            private IList<Models.ForeignKeyModel> _foreignKeys;
            public IList<Models.ForeignKeyModel> ForeignKeys
            {
                get
                {
                    if (this._foreignKeys == null)
                    {
                        this._foreignKeys = (from i in Processes.Database.GetForeignKeys(this.ConnectionString, this.TableNames) orderby i.TableName, i.ForeignKeyName select i).ToList();
                    }
                    return this._foreignKeys;
                }
            }

            private IList<Models.IndexModel> _indexes;
            public IList<Models.IndexModel> Indexes
            {
                get
                {
                    if (this._indexes == null)
                    {
                        this._indexes = (from i in Processes.Database.GetIndexes(this.ConnectionString, this.TableNames) orderby i.TableName, i.IndexName select i).ToList();
                    }
                    return this._indexes;
                }
            }

            private IList<Models.IndexModel> _primaryKeys;
            public IList<Models.IndexModel> PrimaryKeys
            {
                get
                {
                    if (this._primaryKeys == null)
                    {
                        this._primaryKeys = (from i in Processes.Database.GetPrimaryKeys(this.ConnectionString, this.TableNames) orderby i.TableName, i.IndexName select i).ToList();
                    }
                    return this._primaryKeys;
                }
            }


            public IList<string> TableNames
            {
                get
                {
                    return (
                        from i in this.Tables
                        where
                            !i.IsHistoryTable
                        select i.TableName).ToList();
                }
            }

            private IList<Models.TableModel> GetFilteredTables()
            {
                if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                {
                    return (
                        from i in this._tables
                        where i.TableName.ToLower().Contains(this.ObjectFilter.ToLower()) &&
                        !i.TableName.StartsWith("__")
                        select i).ToList();
                }

                return this._tables.Where(i => !i.TableName.StartsWith("__")).ToList();
            }

            private IList<Models.TableModel> _tables;
            public IList<Models.TableModel> Tables
            {
                get
                {
                    if (this._tables == null)
                    {
                        this._tables = Processes.Database.GetTables(this.ConnectionString, this.TableColumns).OrderBy(i => i.TableName).ToList();
                        var tableNames = this.GetFilteredTables().Select(i => i.TableName);
                        if (this._indexes == null)
                        {
                            this._indexes = (from i in Processes.Database.GetIndexes(this.ConnectionString, tableNames) orderby i.TableName, i.IndexName select i).ToList();
                        }
                        if (this._primaryKeys == null)
                        {
                            this._primaryKeys = (from i in Processes.Database.GetPrimaryKeys(this.ConnectionString, tableNames) orderby i.TableName, i.IndexName select i).ToList();
                        }
                        foreach (var table in _tables)
                        {
                            foreach (var index in _indexes.Where(i => i.TableName.EqualsIgnoreCase(table.TableName) && i.SchemaName.EqualsIgnoreCase(table.SchemaName)))
                            {
                                table.Indexes.Add(index);
                            }
                            foreach (var index in _primaryKeys.Where(i => i.TableName.EqualsIgnoreCase(table.TableName) && i.SchemaName.EqualsIgnoreCase(table.SchemaName)))
                            {
                                table.Indexes.Add(index);
                            }
                        }
                    }

                    return GetFilteredTables();
                }
            }

            private IList<Models.ColumnModel> _tableColumns;
            public IList<Models.ColumnModel> TableColumns
            {
                get
                {
                    if (this._tableColumns == null)
                    {
                        this._tableColumns = Processes.Database.GetTableColumns(this.ConnectionString);
                    }
                    return this._tableColumns;
                }
            }

            private IList<Models.TableModel> _views;
            public IList<Models.TableModel> Views
            {
                get
                {
                    if (this._views == null)
                    {
                        this._views = Processes.Database.GetViews(this.ConnectionString, this.ViewColumns).OrderBy(i => i.TableName).ToList();
                    }

                    if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                    {
                        return (
                            from i in this._views
                            where i.TableName.ToLower().Contains(this.ObjectFilter.ToLower())
                            select i).ToList();
                    }

                    return this._views;
                }
            }

            public IList<string> ViewNames
            {
                get
                {
                    return (
                        from i in this.Views
                        select i.TableName).ToList();
                }
            }

            private IList<Models.ColumnModel> _viewColumns;
            public IList<Models.ColumnModel> ViewColumns
            {
                get
                {
                    if (this._viewColumns == null)
                    {
                        this._viewColumns = Processes.Database.GetViewColumns(this.ConnectionString);
                    }
                    return this._viewColumns;
                }
            }

            private IList<Models.TriggerModel> _triggers;
            public IList<Models.TriggerModel> Triggers
            {
                get
                {
                    if (this._triggers == null)
                    {
                        this._triggers = Processes.Database.GetTriggers(this.ConnectionString, this.TableNames, this.ViewNames, this.ObjectFilter).OrderBy(i => i.TriggerName).ToList(); ;
                    }
                    return this._triggers;
                }
            }

            public string ObjectFilter { get; set; }

            public string QuoteCharacterStart { get; set; }
            public string QuoteCharacterEnd { get; set; }

            #endregion

            #region Methods

            public System.Data.DataSet Execute(string sqlCommand)
            {
                return Processes.Database.Execute(this.ConnectionString, sqlCommand);
            }

            public string FormatValue(System.Data.DbType dbType, object value)
            {
                if (value == DBNull.Value || value == null)
                {
                    return "NULL";
                }

                switch (dbType)
                {
                    case System.Data.DbType.Boolean:
                        if (Convert.ToBoolean(value))
                        {
                            return "1";
                        }
                        else
                        {
                            return "0";
                        }
                    case System.Data.DbType.Decimal:
                    case System.Data.DbType.Double:
                    case System.Data.DbType.Int16:
                    case System.Data.DbType.Int32:
                    case System.Data.DbType.Int64:
                    case System.Data.DbType.Single:
                    case System.Data.DbType.UInt16:
                    case System.Data.DbType.UInt32:
                    case System.Data.DbType.UInt64:
                        return value.ToString();
                    default:
                        return string.Format("'{0}'", value.ToString().Trim().Replace("'", "''").Replace(Environment.NewLine, "' + CHAR(13) + CHAR(10) + '"));
                }
            }

            public string GetChangeScript(DatabaseModel changedDatabase)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                //Find objects that have been droped (in me but not in the changed database)
                foreach (var definition in (
                    from i in this.Definitions
                    where (
                        from i2 in changedDatabase.Definitions
                        where i2.DefinitionName.Equals(i.DefinitionName)
                        select i2).Count() == 0
                    select i))
                {
                    definition.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var foreignKey in (
                    from i in this.ForeignKeys
                    where (
                        from i2 in changedDatabase.ForeignKeys
                        where i2.ForeignKeyName.Equals(i.ForeignKeyName)
                        select i2).Count() == 0
                    select i))
                {
                    foreignKey.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in (
                    from i in this.Indexes
                    where (
                        from i2 in changedDatabase.Indexes
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    index.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var primaryKey in (
                    from i in this.PrimaryKeys
                    where (
                        from i2 in changedDatabase.PrimaryKeys
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    primaryKey.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var trigger in (
                    from i in this.Triggers
                    where (
                        from i2 in changedDatabase.Triggers
                        where i2.TriggerName.Equals(i.TriggerName)
                        select i2).Count() == 0
                    select i))
                {
                    trigger.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var table in (
                    from i in this.Tables
                    where (
                        from i2 in changedDatabase.Tables
                        where i2.TableName.Equals(i.TableName)
                        select i2).Count() == 0
                    select i))
                {
                    table.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                //Find objects that have been created (in the changedDatabase but not in me)
                foreach (var table in (
                    from i in changedDatabase.Tables
                    where (
                        from i2 in this.Tables
                        where i2.TableName.Equals(i.TableName)
                        select i2).Count() == 0
                    select i))
                {
                    table.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var trigger in (
                    from i in changedDatabase.Triggers
                    where (
                        from i2 in this.Triggers
                        where i2.TriggerName.Equals(i.TriggerName)
                        select i2).Count() == 0
                    select i))
                {
                    trigger.AppendScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var primaryKey in (
                    from i in changedDatabase.PrimaryKeys
                    where (
                        from i2 in this.PrimaryKeys
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    primaryKey.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in (
                    from i in changedDatabase.Indexes
                    where (
                        from i2 in this.Indexes
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    index.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var foreignKey in (
                    from i in changedDatabase.ForeignKeys
                    where (
                        from i2 in this.ForeignKeys
                        where i2.ForeignKeyName.Equals(i.ForeignKeyName)
                        select i2).Count() == 0
                    select i))
                {
                    foreignKey.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var definition in (
                    from i in changedDatabase.Definitions
                    where (
                        from i2 in this.Definitions
                        where i2.DefinitionName.Equals(i.DefinitionName)
                        select i2).Count() == 0
                    select i))
                {
                    definition.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public Dictionary<ForeignKeyModel, string> GetFkDropScriptsIndividual()
            {

                var dictionary = new Dictionary<ForeignKeyModel, string>();

                foreach (var foreignKeyGroup in (
                    from i in this.ForeignKeys
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {

                    foreach (var foreignKey in foreignKeyGroup.Items)
                    {
                        var sb = new StringBuilder();

                        foreignKey.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);

                        dictionary.Add(foreignKey, sb.ToString());
                    }
                }

                return dictionary;
            }

            public string GetFkDropScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var foreignKeyGroup in (
                    from i in this.ForeignKeys
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(string.Format("-- {0}", foreignKeyGroup.TableName));

                    foreach (var foreignKey in foreignKeyGroup.Items)
                    {
                        foreignKey.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    }
                }

                return sb.ToString();
            }

            public string GetFkScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var foreignKeyGroup in (
                    from i in this.ForeignKeys
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(string.Format("-- {0}", foreignKeyGroup.TableName));

                    foreach (var foreignKey in foreignKeyGroup.Items)
                    {
                        foreignKey.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    }
                }

                return sb.ToString();
            }

            public Dictionary<ForeignKeyModel, string> GetFkScriptsIndividual()
            {
                var dictionary = new Dictionary<ForeignKeyModel, string>();

                foreach (var foreignKeyGroup in (
                    from i in this.ForeignKeys
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {

                    foreach (var foreignKey in foreignKeyGroup.Items)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        foreignKey.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                        foreignKey.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                        dictionary.Add(foreignKey, sb.ToString());
                    }
                }

                return dictionary;
            }

            public string GetInsertDefaultScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                Dictionary<string, string> tableScripts = new Dictionary<string, string>();

                foreach (Models.TableModel tableRow in this.Tables)
                {
                    if (tableRow.TableName.StartsWith("default_"))
                    {
                        string strScript = this.GetInsertScript(tableRow.TableName);

                        if (!(string.IsNullOrEmpty(strScript)))
                        {
                            tableScripts.Add(tableRow.TableName, strScript);
                        }
                    }
                }

                while (!(tableScripts.Keys.Count == 0))
                {
                    string key = tableScripts.Keys.ToList()[0];
                    string value = tableScripts[key];

                    sb.AppendLine(value);
                    tableScripts.Remove(key);
                }

                return sb.ToString();
            }

            public Dictionary<TableModel, string> GetInsertDefaultScriptsIndividual()
            {
                var dictionary = new Dictionary<TableModel, string>();

                foreach (Models.TableModel tableRow in this.Tables)
                {
                    if (tableRow.TableName.StartsWith("default_"))
                    {
                        string strScript = this.GetInsertScript(tableRow.TableName);

                        if (!(string.IsNullOrEmpty(strScript)))
                        {
                            dictionary.Add(tableRow, strScript);
                        }
                    }
                }

                return dictionary;
            }

            public string GetInsertScript(string tableName, bool includeGOAfterEachInsert = false)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                System.Text.StringBuilder sbOrderBy = new System.Text.StringBuilder();

                var table = (
                    from i in this.Tables
                    where i.TableName.EqualsIgnoreCase(tableName)
                    select i).FirstOrDefault();

                var blnIsView = (
                    from i in this.Views
                    where i.TableName.EqualsIgnoreCase(tableName)
                    select i).Count() > 0;

                List<Models.ColumnModel> columns = new List<Models.ColumnModel>();

                if (blnIsView)
                {
                    columns = (
                        from c in this.ViewColumns
                        where c.TableName.EqualsIgnoreCase(tableName)
                        select c).ToList();
                }
                else
                {
                    columns = (
                        from c in this.TableColumns
                        where c.TableName.EqualsIgnoreCase(tableName)
                        select c).ToList();
                }

                foreach (Models.ColumnModel item in columns)
                {
                    if (!item.IsNullable)
                    {
                        if (sbOrderBy.Length != 0)
                        {
                            sbOrderBy.Append(", ");
                        }
                        sbOrderBy.Append(item.ColumnName);
                    }
                }

                string strQualifiedTableName = tableName;

                if (table != null)
                {
                    strQualifiedTableName = $"{table.SchemaName}.{table.TableName}";
                }

                string strOrderBy = string.Empty;

                if (sbOrderBy.Length > 0)
                {
                    strOrderBy = "ORDER BY " + sbOrderBy.ToString();
                }

                System.Data.DataSet dsValues = Processes.Database.Execute(this.ConnectionString, string.Format("SELECT * FROM {0} {1}", strQualifiedTableName, strOrderBy));

                if (dsValues.Tables[0].Rows.Count > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }
                    sb.AppendLine(string.Format("-- {0}", tableName));

                    if (tableName.StartsWith("default_", StringComparison.InvariantCultureIgnoreCase))
                    {
                        sb.AppendLine();
                        sb.AppendLine(string.Format("DELETE FROM {0}", strQualifiedTableName));
                        sb.AppendLine();
                    }

                    foreach (System.Data.DataRow valueRow in dsValues.Tables[0].Rows)
                    {
                        System.Text.StringBuilder sbIfNotExists = new System.Text.StringBuilder();
                        System.Text.StringBuilder sbFields = new System.Text.StringBuilder();
                        System.Text.StringBuilder sbValues = new System.Text.StringBuilder();
                        foreach (Models.ColumnModel item in columns)
                        {
                            if (!item.IsNullable)
                            {
                                if (sbIfNotExists.Length != 0)
                                {
                                    sbIfNotExists.Append(" AND ");
                                }
                                sbIfNotExists.Append(string.Format("{0} = {1}", item.ColumnName, this.FormatValue(item.DbType, valueRow[item.ColumnName])));
                            }
                            if (sbFields.Length != 0)
                            {
                                sbFields.Append(", ");
                            }
                            if (sbValues.Length != 0)
                            {
                                sbValues.Append(", ");
                            }
                            sbFields.Append(string.Format("{0}", item.ColumnName));
                            switch (item.ColumnName)
                            {
                                case "changed_date":
                                    sbValues.Append("GETDATE()");
                                    break;
                                case "changed_user_id":
                                    sbValues.Append("'SYSTEM'");
                                    break;
                                default:
                                    sbValues.Append(string.Format("{0}", this.FormatValue(item.DbType, valueRow[item.ColumnName])));
                                    break;
                            }
                        }

                        sb.AppendLine(string.Format("INSERT INTO {0} ({1}) VALUES ({2})", strQualifiedTableName, sbFields.ToString(), sbValues.ToString()));

                        if (includeGOAfterEachInsert)
                        {
                            sb.AppendLine("GO");
                        }
                    }

                    if (!includeGOAfterEachInsert)
                    {
                        sb.AppendLine("GO");
                    }
                }
                return sb.ToString();
            }

            public Dictionary<IndexModel, string> GetIxPkScriptsIndividual()
            {
                var dictionary = new Dictionary<IndexModel, string>();

                foreach (var pk in this.PrimaryKeys)
                {
                    var sb = new StringBuilder();
                    pk.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    dictionary.Add(pk, sb.ToString());
                }

                foreach (var index in this.Indexes)
                {
                    var sb = new StringBuilder();
                    index.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    dictionary.Add(index, sb.ToString());
                }

                return dictionary;
            }

            public string GetIxPkScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var pk in this.PrimaryKeys)
                {
                    pk.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in this.Indexes)
                {
                    index.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public string GetMergeScript(DatabaseModel targetDatabase)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                var selectedTables = (
                    from i in this.Tables
                    where i.Selected
                    select i).ToList();

                this.SortTablesByDependencies(selectedTables);

                foreach (var table in selectedTables)
                {
                    System.Text.StringBuilder sbCriteria = new System.Text.StringBuilder();

                    foreach (var keyColumn in (
                        from i in table.Columns
                        where i.IsPrimaryKey
                        select i))
                    {
                        if (sbCriteria.Length > 0)
                        {
                            sbCriteria.Append(" AND ");
                        }
                        sbCriteria.AppendFormat("target_table.{0} = source_table.{0}", keyColumn.ColumnName);
                    }

                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(string.Format("-- {0}", table.TableName));
                    sb.AppendLine();

                    sb.AppendLine("INSERT INTO");
                    sb.AppendLine($"    {targetDatabase.DatabaseName}.{table.SchemaName}.{table.TableName}");
                    sb.AppendLine("    SELECT");
                    sb.AppendLine("        *");
                    sb.AppendLine("    FROM");
                    sb.AppendLine($"       { this.DatabaseName}.{table.SchemaName}.{table.TableName} source_table");
                    sb.AppendLine("    WHERE");
                    sb.AppendLine($"        NOT EXISTS (SELECT 1 FROM {targetDatabase.DatabaseName}.{table.SchemaName}.{table.TableName} target_table WHERE {sbCriteria.ToString()})");
                }

                return sb.ToString();
            }

            public string GetScript(string quoteCharacter)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine(this.GetTableScripts());

                sb.AppendLine(this.GetIxPkScripts());

                sb.AppendLine(this.GetDefinitionScripts());

                sb.AppendLine(this.GetTriggerScripts());

                sb.AppendLine(this.GetFkScripts());

                if (Processes.Database.GetDatabaseType(this.ConnectionString) == Models.DatabaseType.MicrosoftSQLServer)
                {
                    sb.AppendLine(this.GetInsertDefaultScripts());
                }

                return sb.ToString();
            }

            public string GetDefinitionDropScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var definition in this.Definitions)
                {
                    definition.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public Dictionary<DefinitionModel, string> GetDefinitionScriptsIndividual()
            {

                var dictionary = new Dictionary<DefinitionModel, string>();

                foreach (var definition in this.Definitions)
                {
                    var sb = new System.Text.StringBuilder();
                    definition.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    definition.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    dictionary.Add(definition, sb.ToString());
                }

                return dictionary;
            }

            public string GetDefinitionScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var definition in this.Definitions)
                {
                    definition.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public Dictionary<Models.TableModel, string> GetTableScriptsIndividual(bool includeIfNotExists = true)
            {
                var dictionary = new Dictionary<Models.TableModel, string>();

                foreach (Models.TableModel table in this.Tables.Where(i => !i.IsHistoryTable))
                {
                    var sbTableScript = new StringBuilder();
                    table.AppendCreateScript(sbTableScript, this.QuoteCharacterStart, this.QuoteCharacterEnd, includeIfNotExists);
                    dictionary.Add(table, sbTableScript.ToString());
                }

                return dictionary;
            }

            public string GetTableScripts(bool includeIfNotExists = true)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (Models.TableModel table in this.Tables.Where(i => !i.IsHistoryTable))
                {
                    table.AppendCreateScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd, includeIfNotExists);
                }

                return sb.ToString();
            }

            public Dictionary<Models.TriggerModel, string> GetTriggerDropScriptsIndividual()
            {
                var dictionary = new Dictionary<Models.TriggerModel, string>();

                foreach (var triggerGroup in (
                    from i in this.Triggers
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {

                    foreach (var trigger in triggerGroup.Items)
                    {
                        var sb = new StringBuilder();

                        trigger.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);

                        dictionary.Add(trigger, sb.ToString());
                    }
                }

                return dictionary;
            }

            public string GetTriggerDropScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var triggerGroup in (
                    from i in this.Triggers
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(string.Format("-- {0}", triggerGroup.TableName));

                    foreach (var trigger in triggerGroup.Items)
                    {
                        trigger.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    }
                }

                return sb.ToString();
            }

            public string GetTriggerScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var triggerGroup in (
                    from i in this.Triggers
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(string.Format("-- {0}", triggerGroup.TableName));

                    foreach (var trigger in triggerGroup.Items)
                    {
                        trigger.AppendScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    }
                }

                return sb.ToString();
            }

            public Dictionary<TriggerModel, string> GetTriggerScriptsIndividual()
            {
                var dictionary = new Dictionary<TriggerModel, string>();

                foreach (var triggerGroup in (
                    from i in this.Triggers
                    group i by i.TableName into g
                    select new { TableName = g.Key, Items = g }))
                {

                    foreach (var trigger in triggerGroup.Items)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();

                        trigger.AppendDropScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                        trigger.AppendScript(sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);

                        dictionary.Add(trigger, sb.ToString());
                    }
                }

                return dictionary;
            }

            private void SortTablesByDependencies(IList<TableModel> list)
            {
                //Order tables by dependencies
                bool blnTablesMoved = false;

                foreach (var table in (
                    from i in list
                    select i).ToList())
                {
                    string strTableName = table.TableName;

                    var tableForeignKeys = (
                        from i in this.ForeignKeys
                        where i.TableName == strTableName
                        select i);

                    foreach (var foreignKeyTable in tableForeignKeys)
                    {
                        string strReferenedTableName = foreignKeyTable.ReferencedTableName;
                        var referencedTable = (
                            from i in list
                            where i.TableName == strReferenedTableName
                            select i).FirstOrDefault();

                        if (referencedTable != null)
                        {
                            int intTableIndex = list.IndexOf(table);
                            int intReferencedTableIndex = list.IndexOf(referencedTable);
                            if (intTableIndex < intReferencedTableIndex)
                            {
                                list.Remove(table);
                                list.Insert(list.IndexOf(referencedTable) + 1, table);
                                blnTablesMoved = true;
                            }
                        }

                    }
                }

                if (blnTablesMoved)
                {
                    this.SortTablesByDependencies(list);
                }
            }

            #endregion

        }
    }




}