
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Collections.ObjectModel;

namespace DatabaseTools
{
    namespace ViewModels
    {
        public class TableMappingViewModel : ViewModel
        {
            #region Properties

            private string _ColumnMapping;
            public string ColumnMapping
            {
                get
                {
                    return this._ColumnMapping;
                }
                set
                {
                    if (!(object.Equals(this._ColumnMapping, value)))
                    {
                        this._ColumnMapping = value;
                        this.OnPropertyChanged("ColumnMapping");
                    }
                }
            }

            private ObservableCollection<DatabaseTools.Models.ColumnMappingModel> _columnMappings;
            public ObservableCollection<DatabaseTools.Models.ColumnMappingModel> ColumnMappings
            {
                get
                {
                    if (this._columnMappings == null)
                    {
                        this._columnMappings = new ObservableCollection<DatabaseTools.Models.ColumnMappingModel>();
                    }
                    return this._columnMappings;
                }
            }

            private bool _IncludeSourceDatabase;
            public bool IncludeSourceDatabase
            {
                get
                {
                    return this._IncludeSourceDatabase;
                }
                set
                {
                    if (!(object.Equals(this._ColumnMapping, value)))
                    {
                        this._IncludeSourceDatabase = value;
                        this.OnPropertyChanged("IncludeSourceDatabase");
                    }
                }
            }

            private bool _IncludeTargetDatabase;
            public bool IncludeTargetDatabase
            {
                get
                {
                    return this._IncludeTargetDatabase;
                }
                set
                {
                    if (!(object.Equals(this._ColumnMapping, value)))
                    {
                        this._IncludeTargetDatabase = value;
                        this.OnPropertyChanged("IncludeTargetDatabase");
                    }
                }
            }

            private bool _IncludeNotExists;
            public bool IncludeNotExists
            {
                get
                {
                    return this._IncludeNotExists;
                }
                set
                {
                    if (!(object.Equals(this._ColumnMapping, value)))
                    {
                        this._IncludeNotExists = value;
                        this.OnPropertyChanged("IncludeNotExists");
                    }
                }
            }

            private DatabaseTools.Models.ColumnMappingModel _SelectedColumnMapping;
            public DatabaseTools.Models.ColumnMappingModel SelectedColumnMapping
            {
                get
                {
                    return this._SelectedColumnMapping;
                }
                set
                {
                    if (!(object.Equals(this._SelectedColumnMapping, value)))
                    {
                        this._SelectedColumnMapping = value;
                        this.OnPropertyChanged("SelectedColumnMapping");
                    }
                }
            }

            private System.Configuration.ConnectionStringSettings _SourceDatabaseConnectionString;
            public System.Configuration.ConnectionStringSettings SourceDatabaseConnectionString
            {
                get
                {
                    return this._SourceDatabaseConnectionString;
                }
                set
                {
                    if (!(object.Equals(this._SourceDatabaseConnectionString, value)))
                    {
                        this._SourceDatabaseConnectionString = value;
                        this.OnPropertyChanged("SourceDatabaseConnectionString");
                    }
                }
            }

            private DatabaseTools.Models.TableModel _SourceTable;
            public DatabaseTools.Models.TableModel SourceTable
            {
                get
                {
                    return this._SourceTable;
                }
                set
                {
                    if (!(object.Equals(this._SourceTable, value)))
                    {
                        this._SourceTable = value;
                        this.OnPropertyChanged("SourceTable");
                    }
                }
            }

            private DatabaseTools.Models.ColumnModel _SourceColumn;
            public DatabaseTools.Models.ColumnModel SourceColumn
            {
                get
                {
                    return this._SourceColumn;
                }
                set
                {
                    if (!(object.Equals(this._SourceColumn, value)))
                    {
                        this._SourceColumn = value;
                        this.OnPropertyChanged("SourceColumn");
                    }
                }
            }

            private ObservableCollection<DatabaseTools.Models.TableModel> _sourceTables;
            public ObservableCollection<DatabaseTools.Models.TableModel> SourceTables
            {
                get
                {
                    if (this._sourceTables == null)
                    {
                        this._sourceTables = new ObservableCollection<DatabaseTools.Models.TableModel>();
                    }
                    return this._sourceTables;
                }
            }

            private ObservableCollection<DatabaseTools.Models.ColumnModel> _sourceColumns;
            public ObservableCollection<DatabaseTools.Models.ColumnModel> SourceColumns
            {
                get
                {
                    if (this._sourceColumns == null)
                    {
                        this._sourceColumns = new ObservableCollection<DatabaseTools.Models.ColumnModel>();
                    }
                    return this._sourceColumns;
                }
            }

            private DatabaseTools.Models.TableModel _TargetTable;
            public DatabaseTools.Models.TableModel TargetTable
            {
                get
                {
                    return this._TargetTable;
                }
                set
                {
                    if (!(object.Equals(this._TargetTable, value)))
                    {
                        this._TargetTable = value;
                        this.OnPropertyChanged("TargetTable");
                    }
                }
            }

            private DatabaseTools.Models.ColumnModel _TargetColumn;
            public DatabaseTools.Models.ColumnModel TargetColumn
            {
                get
                {
                    return this._TargetColumn;
                }
                set
                {
                    if (!(object.Equals(this._TargetColumn, value)))
                    {
                        this._TargetColumn = value;
                        this.OnPropertyChanged("TargetColumn");
                    }
                }
            }

            private System.Configuration.ConnectionStringSettings _TargetDatabaseConnectionString;
            public System.Configuration.ConnectionStringSettings TargetDatabaseConnectionString
            {
                get
                {
                    return this._TargetDatabaseConnectionString;
                }
                set
                {
                    if (!(object.Equals(this._TargetDatabaseConnectionString, value)))
                    {
                        this._TargetDatabaseConnectionString = value;
                        this.OnPropertyChanged("TargetDatabaseConnectionString");
                    }
                }
            }

            private ObservableCollection<DatabaseTools.Models.TableModel> _targetTables;
            public ObservableCollection<DatabaseTools.Models.TableModel> TargetTables
            {
                get
                {
                    if (this._targetTables == null)
                    {
                        this._targetTables = new ObservableCollection<DatabaseTools.Models.TableModel>();
                    }
                    return this._targetTables;
                }
            }

            private ObservableCollection<DatabaseTools.Models.ColumnModel> _targetColumns;
            public ObservableCollection<DatabaseTools.Models.ColumnModel> TargetColumns
            {
                get
                {
                    if (this._targetColumns == null)
                    {
                        this._targetColumns = new ObservableCollection<DatabaseTools.Models.ColumnModel>();
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

                    var mapping = new DatabaseTools.Models.ColumnMappingModel { SourceColumnName = strSourceColumnName, TargetColumnName = this.TargetColumn.ColumnName, IsTargetColumnIdentity = this.TargetColumn.IsIdentity, ColumnMapping = this.ColumnMapping };

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

                    var mappings = DatabaseTools.Processes.Mapping.AutoMatch(sourceTableColumns, targetTableColumns);

                    foreach (var mapping in mappings)
                    {
                        this.AddMapping(mapping);
                    }
                }
            }

            public void AddMapping(DatabaseTools.Models.ColumnMappingModel mapping)
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

            public DatabaseTools.Models.TableMapping CreateTableMapping()
            {
                DatabaseTools.Models.TableMapping tableMapping = new DatabaseTools.Models.TableMapping { SourceTableName = this.SourceTable.TableName, TargetTableName = this.TargetTable.TableName };
                tableMapping.ColumnMappings.AddRange(this.ColumnMappings);

                if (this.IncludeSourceDatabase)
                {
                    System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(this.SourceDatabaseConnectionString.ConnectionString);
                    tableMapping.SourceDatabase = builder.InitialCatalog;
                }

                if (this.IncludeTargetDatabase)
                {
                    System.Data.SqlClient.SqlConnectionStringBuilder builder = new System.Data.SqlClient.SqlConnectionStringBuilder(this.TargetDatabaseConnectionString.ConnectionString);
                    tableMapping.TargetDatabase = builder.InitialCatalog;
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
                return DatabaseTools.Processes.Mapping.CreateScript(tableMapping);
            }

            public string CreateXml()
            {
                var tableMapping = this.CreateTableMapping();
                return DatabaseTools.Processes.Mapping.CreateXml(tableMapping);
            }

            public void UpdateTargetTables()
            {
                var columns = DatabaseTools.Processes.Database.GetTableColumns(this.TargetDatabaseConnectionString);
                var viewColumns = DatabaseTools.Processes.Database.GetViewColumns(this.TargetDatabaseConnectionString);
                var tables = DatabaseTools.Processes.Database.GetTables(this.TargetDatabaseConnectionString, columns, true);
                var views = DatabaseTools.Processes.Database.GetViews(this.TargetDatabaseConnectionString, viewColumns);
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

            public void UpdateSourceTables()
            {
                var columns = DatabaseTools.Processes.Database.GetTableColumns(this.SourceDatabaseConnectionString);
                var viewColumns = DatabaseTools.Processes.Database.GetViewColumns(this.SourceDatabaseConnectionString);
                var tables = DatabaseTools.Processes.Database.GetTables(this.SourceDatabaseConnectionString, columns, true);
                var views = DatabaseTools.Processes.Database.GetViews(this.SourceDatabaseConnectionString, viewColumns);
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