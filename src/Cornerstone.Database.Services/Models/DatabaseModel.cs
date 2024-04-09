using Cornerstone.Database.Extensions;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;

namespace Cornerstone.Database
{
    namespace Models
    {
        public class DatabaseModel
        {

            private readonly Services.DatabaseService _databaseService;
            private readonly IDatabaseProvider _databaseProvider;

            public DatabaseModel(IDatabaseFactory databaseFactory, System.Configuration.ConnectionStringSettings connectionString)
            {
                this._connectionString = connectionString;
                _databaseProvider = databaseFactory.GetDatabaseProvider(connectionString);
                _databaseService = new DatabaseService(databaseFactory, _databaseProvider);
                this._databaseName = _databaseProvider.GetDatabaseName(connectionString.ConnectionString);
                QuoteCharacterEnd = _databaseProvider.QuoteCharacterEnd;
                QuoteCharacterStart = _databaseProvider.QuoteCharacterStart;
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

            private IList<Models.DefinitionModel> _definitions;
            public IList<Models.DefinitionModel> Definitions
            {
                get
                {
                    if (this._definitions == null)
                    {
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._definitions = _databaseService.GetDefinitions(conn);
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._foreignKeys = (from i in _databaseService.GetForeignKeys(conn, this.TableNames) orderby i.TableName, i.ForeignKeyName select i).ToList();
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._checkConstraints = (from i in _databaseService.GetCheckConstraints(conn, this.TableNames) orderby i.TableName, i.CheckConstraintName select i).ToList();
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._allIndexes = (from i in _databaseService.GetIndexes(conn, this.TableNames, null) orderby i.TableName, i.IndexName select i).ToList();
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._tables = _databaseService.GetTables(conn, this.TableColumns).OrderBy(i => i.TableName).ToList();
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._tableColumns = _databaseService.GetTableColumns(conn);
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._views = _databaseService.GetViews(conn, this.ViewColumns).OrderBy(i => i.TableName).ToList();
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._viewColumns = _databaseService.GetViewColumns(conn);
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._triggers = _databaseService.GetTriggers(conn, this.TableNames, this.ViewNames, this.ObjectFilter).OrderBy(i => i.TriggerName).ToList(); ;
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
                        using (var conn = _databaseService.CreateDbConnection(this.ConnectionString))
                        {
                            this._securityPolicies = _databaseService.GetSecurityPolicies(conn).OrderBy(i => i.PolicyName).ToList();
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
