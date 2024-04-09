using System.Data;
using System.Text;
using Cornerstone.Database.Extensions;
using Cornerstone.Database.Services;
using Cornerstone.Database.Providers;

namespace Cornerstone.Database
{
    namespace Models
    {
        public class DatabaseModel
        {

            private readonly Services.DatabaseService _database;

            public DatabaseModel(System.Configuration.ConnectionStringSettings connectionString, IEnumerable<IDatabaseProvider> databaseProviders, IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications) : this(connectionString, databaseProviders, connectionCreatedNotifications, Services.DatabaseService.GetDatabaseType(connectionString))
            {

            }

            public DatabaseModel(System.Configuration.ConnectionStringSettings connectionString, IEnumerable<IDatabaseProvider> databaseProviders, IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications, Models.DatabaseType targetDatabseType)
            {
                this._connectionString = connectionString;
                this._databaseType = Services.DatabaseService.GetDatabaseType(connectionString);
                _database = new Services.DatabaseService(targetDatabseType, databaseProviders, connectionCreatedNotifications);
                this._databaseName = _database.Provider.GetDatabaseName(connectionString.ConnectionString);

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

            private readonly System.Configuration.ConnectionStringSettings _connectionString;
            public System.Configuration.ConnectionStringSettings ConnectionString
            {
                get
                {
                    return this._connectionString;
                }
            }

            private readonly string _databaseName;
            public string DatabaseName
            {
                get
                {
                    return this._databaseName;
                }
            }

            private readonly DatabaseType _databaseType;
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
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._definitions = _database.GetDefinitions(conn);
                        }

                        foreach (var def in _definitions.Where(i => i.Type == "VIEW"))
                        {
                            def.View = Views.FirstOrDefault(i => i.SchemaName.EqualsIgnoreCase(def.SchemaName) && i.TableName.EqualsIgnoreCase(def.DefinitionName));
                        }

                    }

                    var filteredList = this._definitions.ToList();

                    if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                    {
                        filteredList = (
                            from i in filteredList
                            where i.DefinitionName.ToLower().Contains(this.ObjectFilter.ToLower())
                            select i).ToList();
                    }

                    if (ExcludeDoubleUnderscoreObjects)
                    {
                        filteredList = filteredList.Where(i => !i.DefinitionName.StartsWith("__")).ToList();
                    }

                    return filteredList.ToList();
                }
            }

            private IList<Models.ForeignKeyModel> _foreignKeys;
            public IList<Models.ForeignKeyModel> ForeignKeys
            {
                get
                {
                    if (this._foreignKeys == null)
                    {
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._foreignKeys = (from i in _database.GetForeignKeys(conn, this.TableNames) orderby i.TableName, i.ForeignKeyName select i).ToList();
                        }
                    }
                    return this._foreignKeys;
                }
            }

            private IList<Models.CheckConstraintModel> _checkConstraints;
            public IList<Models.CheckConstraintModel> CheckConstraints
            {
                get
                {
                    if (this._checkConstraints == null)
                    {
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._checkConstraints = (from i in _database.GetCheckConstraints(conn, this.TableNames) orderby i.TableName, i.CheckConstraintName select i).ToList();
                        }
                    }
                    return this._checkConstraints;
                }
            }

            private IList<Models.IndexModel> _allIndexes;
            private IList<Models.IndexModel> AllIndexes
            {
                get
                {
                    if (this._allIndexes == null)
                    {
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._allIndexes = (from i in _database.GetIndexes(conn, this.TableNames, null) orderby i.TableName, i.IndexName select i).ToList();
                        }
                    }
                    return this._allIndexes;
                }
            }

            private IList<Models.IndexModel> _indexes;
            public IList<Models.IndexModel> Indexes
            {
                get
                {
                    if (this._indexes == null)
                    {
                        this._indexes = (from i in AllIndexes where !i.IsPrimaryKey select i).ToList();
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
                        this._primaryKeys = (from i in AllIndexes where i.IsPrimaryKey select i).ToList();
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
                        select i.TableName).ToList();
                }
            }

            private IList<Models.TableModel> GetFilteredTables()
            {
                var filteredList = this._tables.ToList();

                if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                {
                    filteredList = (
                        from i in filteredList
                        where i.TableName.ToLower().Contains(this.ObjectFilter.ToLower())
                        select i).ToList();
                }

                if (ExcludeDoubleUnderscoreObjects)
                {
                    filteredList = filteredList.Where(i => !i.TableName.StartsWith("__")).ToList();
                }

                return filteredList.ToList();
            }

            private IList<Models.TableModel> GetFilteredViews()
            {
                var filteredList = this._views.ToList();

                if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                {
                    filteredList = (
                        from i in filteredList
                        where i.TableName.ToLower().Contains(this.ObjectFilter.ToLower())
                        select i).ToList();
                }

                if (ExcludeDoubleUnderscoreObjects)
                {
                    filteredList = filteredList.Where(i => !i.TableName.StartsWith("__")).ToList();
                }

                return filteredList.ToList();
            }

            private IList<Models.TriggerModel> GetFilteredTriggers()
            {
                var filteredList = this._triggers.ToList();

                if (!(string.IsNullOrEmpty(this.ObjectFilter)))
                {
                    filteredList = (
                        from i in filteredList
                        where i.TriggerName.ToLower().Contains(this.ObjectFilter.ToLower())
                        select i).ToList();
                }

                if (ExcludeDoubleUnderscoreObjects)
                {
                    filteredList = filteredList.Where(i => !i.TriggerName.StartsWith("__")).ToList();
                }

                return filteredList.ToList();
            }

            private IList<Models.TableModel> _tables;
            public IList<Models.TableModel> Tables
            {
                get
                {
                    if (this._tables == null)
                    {
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._tables = _database.GetTables(conn, this.TableColumns).OrderBy(i => i.TableName).ToList();
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
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._tableColumns = _database.GetTableColumns(conn);
                        }
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
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._views = _database.GetViews(conn, this.ViewColumns).OrderBy(i => i.TableName).ToList();
                        }
                    }

                    return GetFilteredViews();
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
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._viewColumns = _database.GetViewColumns(conn);
                        }
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
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._triggers = _database.GetTriggers(conn, this.TableNames, this.ViewNames, this.ObjectFilter).OrderBy(i => i.TriggerName).ToList(); ;
                        }
                    }
                    return GetFilteredTriggers();
                }
            }
            private IList<Models.SecurityPolicyModel> _securityPolicies;
            public IList<Models.SecurityPolicyModel> SecurityPolicies
            {
                get
                {
                    if (this._securityPolicies == null)
                    {
                        using (var conn = _database.CreateDbConnection(this.ConnectionString))
                        {
                            this._securityPolicies = _database.GetSecurityPolicies(conn).OrderBy(i => i.PolicyName).ToList();
                        }
                    }
                    return this._securityPolicies;
                }
            }

            public string ObjectFilter { get; set; }
            public bool ExcludeDoubleUnderscoreObjects { get; set; }

            public string QuoteCharacterStart { get; set; }
            public string QuoteCharacterEnd { get; set; }

            #endregion

            #region Methods

            public System.Data.DataSet Execute(string sqlCommand)
            {
                return _database.Execute(this.ConnectionString, sqlCommand);
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
                    case DbType.DateTime:
                    case DbType.DateTime2:
                        return string.Format("'{0}'", ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
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
                    definition.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var foreignKey in (
                    from i in this.ForeignKeys
                    where (
                        from i2 in changedDatabase.ForeignKeys
                        where i2.ForeignKeyName.Equals(i.ForeignKeyName)
                        select i2).Count() == 0
                    select i))
                {
                    foreignKey.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in (
                    from i in this.Indexes
                    where (
                        from i2 in changedDatabase.Indexes
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    index.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var primaryKey in (
                    from i in this.PrimaryKeys
                    where (
                        from i2 in changedDatabase.PrimaryKeys
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    primaryKey.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
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
                    table.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
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
                    table.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
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
                    primaryKey.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in (
                    from i in changedDatabase.Indexes
                    where (
                        from i2 in this.Indexes
                        where i2.IndexName.Equals(i.IndexName)
                        select i2).Count() == 0
                    select i))
                {
                    index.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var foreignKey in (
                    from i in changedDatabase.ForeignKeys
                    where (
                        from i2 in this.ForeignKeys
                        where i2.ForeignKeyName.Equals(i.ForeignKeyName)
                        select i2).Count() == 0
                    select i))
                {
                    foreignKey.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var definition in (
                    from i in changedDatabase.Definitions
                    where (
                        from i2 in this.Definitions
                        where i2.DefinitionName.Equals(i.DefinitionName)
                        select i2).Count() == 0
                    select i))
                {
                    definition.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public string GetCheckConstraintScripts()
            {
                var sb = new System.Text.StringBuilder();

                foreach (var constraint in this.CheckConstraints)
                {
                    constraint.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
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
                        foreignKey.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
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
                        foreignKey.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                    }
                }

                return sb.ToString();
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

            public string GetInsertScript(string tableName, string where = "")
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

                var dsValues = _database.Execute(this.ConnectionString, $"SELECT * FROM {strQualifiedTableName} {(string.IsNullOrEmpty(where) ? "" : "WHERE " + where)} {strOrderBy}");

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

                    System.Text.StringBuilder sbFields = new System.Text.StringBuilder();
                    foreach (Models.ColumnModel item in columns)
                    {

                        if (sbFields.Length != 0)
                        {
                            sbFields.Append(", ");
                        }

                        sbFields.Append(string.Format("{0}", item.ColumnName));

                    }

                    int page = 0;
                    int rowsPerPage = 500;

                    var rows = dsValues.Tables[0].Rows.OfType<System.Data.DataRow>().ToList();

                    while (true)
                    {
                        var pagedRows = rows.Skip(rowsPerPage * page).Take(rowsPerPage).ToList();

                        if (pagedRows.Count == 0)
                        {
                            break;
                        }

                        System.Text.StringBuilder sbValues = new System.Text.StringBuilder();

                        foreach (var pagedRow in pagedRows)
                        {
                            if (sbValues.Length > 0)
                            {
                                sbValues.AppendLine(",");
                            }

                            sbValues.Append("    (");

                            var sbColumnValues = new StringBuilder();

                            foreach (Models.ColumnModel item in columns)
                            {
                                if (sbColumnValues.Length != 0)
                                {
                                    sbColumnValues.Append(", ");
                                }
                                switch (item.ColumnName.ToLower())
                                {
                                    case "changed_date":
                                    case "modifieddate":
                                    case "createddate":
                                        sbColumnValues.Append("GETDATE()");
                                        break;
                                    case "changed_user_id":
                                    case "modifiedby":
                                    case "createdby":
                                        sbColumnValues.Append("''");
                                        break;
                                    default:
                                        sbColumnValues.Append(string.Format("{0}", this.FormatValue(item.DbType, pagedRow[item.ColumnName])));
                                        break;
                                }
                            }

                            sbValues.Append($"{sbColumnValues.ToString()})");

                        }

                        sb.AppendLine($@"
INSERT INTO 
    {strQualifiedTableName} 
    (
        {sbFields.ToString()}
    ) 
    VALUES 
{sbValues.ToString()}
GO
");

                        page += 1;
                    }
                }
                return sb.ToString();
            }

            public string GetIxPkScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var pk in this.PrimaryKeys)
                {
                    pk.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                foreach (var index in this.Indexes)
                {
                    index.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
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
                    sb.AppendLine($"       {this.DatabaseName}.{table.SchemaName}.{table.TableName} source_table");
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

                sb.AppendLine(this.GetCheckConstraintScripts());

                sb.AppendLine(this.GetSecurityPolicyScripts());

                if (Services.DatabaseService.GetDatabaseType(this.ConnectionString) == Models.DatabaseType.MicrosoftSQLServer)
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
                    definition.AppendDropScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public string GetDefinitionScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var definition in this.Definitions)
                {
                    definition.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd);
                }

                return sb.ToString();
            }

            public string GetTableScripts(bool includeIfNotExists = true)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (Models.TableModel table in this.Tables.Where(i => !i.IsHistoryTable))
                {
                    table.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd, includeIfNotExists);
                }

                return sb.ToString();
            }

            public string GetTableJsonScripts()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (Models.TableModel table in this.Tables.Where(i => !i.IsHistoryTable))
                {
                    table.AppendJsonScript(this, sb);
                }

                return sb.ToString();
            }

            public string GetSecurityPolicyScripts(bool includeIfNotExists = true)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                foreach (var securityPolicy in this.SecurityPolicies)
                {
                    securityPolicy.AppendCreateScript(this, sb, this.QuoteCharacterStart, this.QuoteCharacterEnd, includeIfNotExists);
                }

                return sb.ToString();
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
