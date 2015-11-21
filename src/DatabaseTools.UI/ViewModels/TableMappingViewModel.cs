
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

            public TableMappingViewModel()
            {
                this.AddMappingCommand = new Command(() =>
                {
                    this.AddMapping();
                });
                this.RemoveMappingCommand = new Command(() =>
                {
                    this.RemoveMapping();
                });
                this.AutoMatchCommand = new Command(() =>
                {
                    this.AutoMatchMappings();
                });
            }

            #region Commands

            public Command AddMappingCommand { get; set; }
            public Command RemoveMappingCommand { get; set; }
            public Command AutoMatchCommand { get; set; }

            #endregion


            #region Properties

            public string ColumnMapping { get; set; }

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

            public bool IncludeSourceDatabase { get; set; }

            public bool IncludeTargetDatabase { get; set; }
          
            public bool IncludeNotExists { get; set; }
          

            public DatabaseTools.Models.ColumnMappingModel SelectedColumnMapping { get; set; }
            
            public System.Configuration.ConnectionStringSettings SourceDatabaseConnectionString { get; set; }
            

           public DatabaseTools.Models.TableModel SourceTable { get; set;}
            public DatabaseTools.Models.ColumnModel SourceColumn { get; set; }
           

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

            public DatabaseTools.Models.TableModel TargetTable { get; set; }
           

            public DatabaseTools.Models.ColumnModel TargetColumn { get; set; }
           

            public System.Configuration.ConnectionStringSettings TargetDatabaseConnectionString { get; set; }
          

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