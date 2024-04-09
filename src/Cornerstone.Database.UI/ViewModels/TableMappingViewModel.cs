using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;

namespace Cornerstone.Database
{
    namespace ViewModels
    {
        public class TableMappingViewModel : ViewModel
        {

            private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
            private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

            public TableMappingViewModel(IEnumerable<IDatabaseProvider> databaseProviders,
                IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
            {
                _connectionCreatedNotifications = connectionCreatedNotifications;
                _databaseProviders = databaseProviders;
                this.AddMappingCommand = new Command(this.AddMapping);
                this.RemoveMappingCommand = new Command(this.RemoveMapping);
                this.AutoMatchCommand = new Command(this.AutoMatchMappings);
            }

            #region Commands

            public Command AddMappingCommand { get; set; }
            public Command RemoveMappingCommand { get; set; }
            public Command AutoMatchCommand { get; set; }

            #endregion

            #region Properties

            public string ColumnMapping { get; set; }

            private ObservableCollection<Cornerstone.Database.Models.ColumnMappingModel> _columnMappings;
            public ObservableCollection<Cornerstone.Database.Models.ColumnMappingModel> ColumnMappings
            {
                get
                {
                    if (this._columnMappings == null)
                    {
                        this._columnMappings = new ObservableCollection<Cornerstone.Database.Models.ColumnMappingModel>();
                    }
                    return this._columnMappings;
                }
            }

            public bool IncludeSourceDatabase { get; set; }

            public bool IncludeTargetDatabase { get; set; }

            public bool IncludeNotExists { get; set; }

            public Cornerstone.Database.Models.ColumnMappingModel SelectedColumnMapping { get; set; }

            public System.Configuration.ConnectionStringSettings SourceDatabaseConnectionString { get; set; }

            public Cornerstone.Database.Models.TableModel SourceTable { get; set; }
            public Cornerstone.Database.Models.ColumnModel SourceColumn { get; set; }

            private ObservableCollection<Cornerstone.Database.Models.TableModel> _sourceTables;
            public ObservableCollection<Cornerstone.Database.Models.TableModel> SourceTables
            {
                get
                {
                    if (this._sourceTables == null)
                    {
                        this._sourceTables = new ObservableCollection<Cornerstone.Database.Models.TableModel>();
                    }
                    return this._sourceTables;
                }
            }

            private ObservableCollection<Cornerstone.Database.Models.ColumnModel> _sourceColumns;
            public ObservableCollection<Cornerstone.Database.Models.ColumnModel> SourceColumns
            {
                get
                {
                    if (this._sourceColumns == null)
                    {
                        this._sourceColumns = new ObservableCollection<Cornerstone.Database.Models.ColumnModel>();
                    }
                    return this._sourceColumns;
                }
            }

            public Cornerstone.Database.Models.TableModel TargetTable { get; set; }

            public Cornerstone.Database.Models.ColumnModel TargetColumn { get; set; }

            public System.Configuration.ConnectionStringSettings TargetDatabaseConnectionString { get; set; }

            private ObservableCollection<Cornerstone.Database.Models.TableModel> _targetTables;
            public ObservableCollection<Cornerstone.Database.Models.TableModel> TargetTables
            {
                get
                {
                    if (this._targetTables == null)
                    {
                        this._targetTables = new ObservableCollection<Cornerstone.Database.Models.TableModel>();
                    }
                    return this._targetTables;
                }
            }

            private ObservableCollection<Cornerstone.Database.Models.ColumnModel> _targetColumns;
            public ObservableCollection<Cornerstone.Database.Models.ColumnModel> TargetColumns
            {
                get
                {
                    if (this._targetColumns == null)
                    {
                        this._targetColumns = new ObservableCollection<Cornerstone.Database.Models.ColumnModel>();
                    }
                    return this._targetColumns;
                }
            }

            #endregion

            #region Methods

            public void AddMapping()
            {
                if (this.TargetColumn != null && (this.SourceColumn != null || !(string.IsNullOrEmpty(this.ColumnMapping))))
                {

                    string strSourceColumnName = string.Empty;
                    if (this.SourceColumn != null)
                    {
                        strSourceColumnName = this.SourceColumn.ColumnName;
                    }

                    var mapping = new Cornerstone.Database.Models.ColumnMappingModel
                    {
                        SourceColumnName = strSourceColumnName,
                        TargetColumnName = this.TargetColumn.ColumnName,
                        IsTargetColumnIdentity = this.TargetColumn.IsIdentity,
                        ColumnMapping = this.ColumnMapping,
                        WrapInIsNull = SourceColumn != null &&
                                    Services.DatabaseService.GetSystemType(TargetColumn.DbType) == typeof(string) &&
                                    Services.DatabaseService.GetSystemType(SourceColumn.DbType) == typeof(string) &&
                                    !TargetColumn.IsNullable &&
                                    SourceColumn.IsNullable
                    };

                    this.AddMapping(mapping);
                }
                this.ColumnMapping = "";
            }

            public void AutoMatchMappings()
            {
                if (this.SourceTable != null && this.TargetTable != null)
                {

                    var sourceTableColumns = (
                        from i in this.SourceTable.Columns
                        where !i.IsComputed
                        select i).ToList();
                    var targetTableColumns = (
                        from i in this.TargetTable.Columns
                        where !i.IsComputed
                        select i).ToList();

                    var mappings = Cornerstone.Database.Services.Mapping.AutoMatch(sourceTableColumns, targetTableColumns);

                    foreach (var mapping in mappings)
                    {
                        this.AddMapping(mapping);
                    }
                }
            }

            public void AddMapping(Cornerstone.Database.Models.ColumnMappingModel mapping)
            {
                this.ColumnMappings.Add(mapping);
                if (!(string.IsNullOrEmpty(mapping.SourceColumnName)))
                {
                    var column = (
                        from i in this.SourceColumns
                        where i.ColumnName.Equals(mapping.SourceColumnName)
                        select i).FirstOrDefault();
                    if (column != null)
                    {
                        this.SourceColumns.Remove(column);
                    }
                }
                if (!(string.IsNullOrEmpty(mapping.TargetColumnName)))
                {
                    var column = (
                        from i in this.TargetColumns
                        where i.ColumnName.Equals(mapping.TargetColumnName)
                        select i).FirstOrDefault();
                    if (column != null)
                    {
                        this.TargetColumns.Remove(column);
                    }
                }
            }

            public void RemoveMapping()
            {
                if (this.SelectedColumnMapping != null)
                {
                    var selectedMapping = this.SelectedColumnMapping;
                    this.ColumnMappings.Remove(selectedMapping);
                    this.SelectedColumnMapping = null;
                    if (!(string.IsNullOrEmpty(selectedMapping.SourceColumnName)) && this.SourceTable != null)
                    {
                        var column = (
                            from i in this.SourceTable.Columns
                            where i.ColumnName.Equals(selectedMapping.SourceColumnName)
                            select i).FirstOrDefault();
                        if (column != null)
                        {
                            this.SourceColumns.Add(column);
                        }
                    }
                    if (!(string.IsNullOrEmpty(selectedMapping.TargetColumnName)) && this.TargetTable != null)
                    {
                        var column = (
                            from i in this.TargetTable.Columns
                            where i.ColumnName.Equals(selectedMapping.TargetColumnName)
                            select i).FirstOrDefault();
                        if (column != null)
                        {
                            this.TargetColumns.Add(column);
                        }
                    }
                }
            }

            public Cornerstone.Database.Models.TableMapping CreateTableMapping()
            {
                Cornerstone.Database.Models.TableMapping tableMapping = new Cornerstone.Database.Models.TableMapping { SourceTableName = this.SourceTable.TableName, TargetTableName = this.TargetTable.TableName };
                tableMapping.ColumnMappings.AddRange(this.ColumnMappings);

                if (this.IncludeSourceDatabase)
                {
                    var dbType = DatabaseService.GetDatabaseType(this.SourceDatabaseConnectionString);
                    var provider = DatabaseService.GetDatabaseProvider(_databaseProviders, dbType);
                    tableMapping.SourceDatabase = provider.GetDatabaseName(this.SourceDatabaseConnectionString.ConnectionString);
                }

                if (this.IncludeTargetDatabase)
                {
                    var dbType = DatabaseService.GetDatabaseType(this.TargetDatabaseConnectionString);
                    var provider = DatabaseService.GetDatabaseProvider(_databaseProviders, dbType);
                    tableMapping.TargetDatabase = provider.GetDatabaseName(this.TargetDatabaseConnectionString.ConnectionString);
                }

                if (this.IncludeNotExists)
                {
                    tableMapping.SelectCriteria = tableMapping.GenerateIncludeNotExists(this.SourceTable, this.TargetTable);
                }

                return tableMapping;
            }

            public string CreateSql()
            {
                var tableMapping = this.CreateTableMapping();
                return Cornerstone.Database.Services.Mapping.CreateScript(tableMapping);
            }

            public string CreateXml()
            {
                var tableMapping = this.CreateTableMapping();
                return Cornerstone.Database.Services.Mapping.CreateXml(tableMapping);
            }

            public void UpdateTargetTables()
            {
                var targetDatbaseType = Services.DatabaseService.GetDatabaseType(TargetDatabaseConnectionString);
                var database = new Services.DatabaseService(targetDatbaseType, _databaseProviders, _connectionCreatedNotifications);

                using (var conn = database.CreateDbConnection(TargetDatabaseConnectionString))
                {
                    var columns = database.GetTableColumns(conn);
                    var viewColumns = database.GetViewColumns(conn);
                    var tables = database.GetTables(conn, columns, true);
                    var views = database.GetViews(conn, viewColumns);
                    this.TargetTables.Clear();
                    foreach (var item in tables)
                    {
                        this.TargetTables.Add(item);
                    }
                    foreach (var item in views)
                    {
                        this.TargetTables.Add(item);
                    }
                    this.TargetTable = null;
                    this.ColumnMappings.Clear();
                }

            }

            public void UpdateSourceTables()
            {
                var datbaseType = Services.DatabaseService.GetDatabaseType(SourceDatabaseConnectionString);
                var database = new Services.DatabaseService(datbaseType, _databaseProviders, _connectionCreatedNotifications);

                using (var conn = database.CreateDbConnection(SourceDatabaseConnectionString))
                {
                    var columns = database.GetTableColumns(conn);
                    var viewColumns = database.GetViewColumns(conn);
                    var tables = database.GetTables(conn, columns, true);
                    var views = database.GetViews(conn, viewColumns);
                    this.SourceTables.Clear();
                    foreach (var item in tables)
                    {
                        this.SourceTables.Add(item);
                    }
                    foreach (var item in views)
                    {
                        this.SourceTables.Add(item);
                    }
                    this.SourceTable = null;
                    this.ColumnMappings.Clear();
                }
            }

            #endregion

            #region Event Handlers

            protected override void OnPropertyChanged(string propertyName)
            {
                base.OnPropertyChanged(propertyName);
                switch (propertyName)
                {
                    case "SourceDatabaseConnectionString":
                        try
                        {
                            this.UpdateSourceTables();
                        }
                        catch
                        {

                        }
                        break;
                    case "SourceTable":
                        this.SourceColumns.Clear();
                        if (this.SourceTable != null)
                        {
                            foreach (var column in (
                                from i in this.SourceTable.Columns
                                orderby i.ColumnName
                                select i))
                            {
                                this.SourceColumns.Add(column);
                            }
                        }
                        this.ColumnMappings.Clear();
                        break;
                    case "TargetDatabaseConnectionString":
                        try
                        {
                            this.UpdateTargetTables();
                        }
                        catch
                        {

                        }
                        break;
                    case "TargetTable":
                        this.TargetColumns.Clear();
                        if (this.TargetTable != null)
                        {
                            foreach (var column in (
                                from i in this.TargetTable.Columns
                                orderby i.ColumnName
                                select i))
                            {
                                this.TargetColumns.Add(column);
                            }
                        }
                        this.ColumnMappings.Clear();
                        break;
                }
            }

            #endregion

        }
    }

}
