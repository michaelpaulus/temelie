using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DatabaseTools.Processes;
using System.Windows;
using System.Windows.Threading;

namespace DatabaseTools.ViewModels
{
    public class ConvertViewModel : ViewModel
    {

        public ConvertViewModel()
        {
            this.ThreadCount = 5;

            this.ConvertCommand = new Command(() =>
            {
                this.Convert();
            });
            this.ToggleAllCommand = new Command(() =>
            {
                this.ToggleAll();
            });
        }

        #region Commands

        public Command ConvertCommand { get; set; }
        public Command ToggleAllCommand { get; set; }

        #endregion

        #region Properties

        private ObservableCollection<DatabaseTools.Models.TableModel> _tables;
        public ObservableCollection<DatabaseTools.Models.TableModel> Tables
        {
            get
            {
                if (this._tables == null)
                {
                    this._tables = new ObservableCollection<DatabaseTools.Models.TableModel>();
                }
                return this._tables;
            }
        }

        public System.Configuration.ConnectionStringSettings SourceDatabaseConnectionString { get; set; }
        public System.Configuration.ConnectionStringSettings TargetDatabaseConnectionString { get; set; }

        public string ErrorMessage { get; set; }
        public int ThreadCount { get; set; }
        public bool UseBulkCopy { get; set; }
        public bool TrimStrings { get; set; }

        #endregion

        #region Methods

        public void UpdateTables()
        {
            try
            {
                var tables = DatabaseConnection.GetTables(this.SourceDatabaseConnectionString);
                this.Tables.Clear();
                foreach (var table in tables)
                {
                    this.Tables.Add(table);
                }
            }
            catch
            {

            }
        }

        public void Convert()
        {
            this.ErrorMessage = "";
            this.ConvertCommand.IsEnabled = false;
            this.ToggleAllCommand.IsEnabled = false;

            var uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());

            var progress = new Progress<TableProgress>(tableProgress =>
            {
                uiFactory.StartNew(() => this.ReportProgress(tableProgress));
            });

            Task.Factory.StartNew(() =>
            {
                this.ConvertTables(progress);
            }).ContinueWith((task) =>
            {
                this.ConvertCommand.IsEnabled = true;
                this.ToggleAllCommand.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private TableConversion GetTableConvert()
        {
            var selectedTables = (from i in this.Tables where i.Selected select i).ToList();

            var targetColumns = DatabaseTools.Processes.Database.GetTableColumns(this.TargetDatabaseConnectionString);
            var targetTables = DatabaseTools.Processes.Database.GetTables(this.TargetDatabaseConnectionString, targetColumns);

            TableConversion tableConvert = new TableConversion();

            tableConvert.SourceConnectionString = this.SourceDatabaseConnectionString;
            tableConvert.TargetConnectionString = this.TargetDatabaseConnectionString;

            tableConvert.SourceTables = selectedTables;
            tableConvert.TargetTables = targetTables;

            tableConvert.UseBulkCopy = this.UseBulkCopy;
            tableConvert.TrimStrings = this.TrimStrings;

            return tableConvert;
        }

        private void ConvertTables(IProgress<TableProgress> progress)
        {
            var tableConvert = GetTableConvert();

            var tables = tableConvert.SourceTables.OrderBy(i => i.TableName);

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = this.ThreadCount;

            Parallel.ForEach(tables, options, (sourceTable) =>
            {
                this.ConvertTable(tableConvert, sourceTable, progress);
            });
        }

        private void ConvertTable(TableConversion tableConvert, Models.TableModel sourceTable, IProgress<TableProgress> progress)
        {

            var targetTable = (
                       from i in tableConvert.TargetTables
                       where i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                       select i).FirstOrDefault();

            if (targetTable != null)
            {
                try
                {
                    var tableConverter = new Processes.TableConverter();

                    if (tableConvert.UseBulkCopy)
                    {
                        tableConverter.ConvertBulk(progress, sourceTable, tableConvert.SourceConnectionString, targetTable, tableConvert.TargetConnectionString);
                    }
                    else
                    {
                        tableConverter.Convert(progress, sourceTable, tableConvert.SourceConnectionString, targetTable, tableConvert.TargetConnectionString, tableConvert.TrimStrings);
                    }

                }
                catch (Exception ex)
                {
                    string strException = string.Empty;
                    if (!(ex.Message.Equals("ERROR [00000] [QODBC] Error: 3250 - This feature is not enabled or not available in this version of QuickBooks.")))
                    {
                        strException = ex.ToString();
                        if (ex.InnerException != null)
                        {
                            strException += Environment.NewLine + ex.InnerException.ToString();
                        }
                    }
                    progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable, ErrorMessage = strException });
                }
            }
            else
            {
                progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
            }
        }

        public void ToggleAll()
        {
            foreach (var table in this.Tables)
            {
                table.Selected = !table.Selected;
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case (nameof(SourceDatabaseConnectionString)):
                    this.UpdateTables();
                    break;
            }
        }

        private void ReportProgress(TableProgress tableProgress)
        {

            tableProgress.Table.ProgressPercentage = tableProgress.ProgressPercentage;

            if (!string.IsNullOrEmpty(tableProgress.ErrorMessage))
            {
                tableProgress.Table.ErrorMessage += tableProgress.Table.ErrorMessage;
                this.ErrorMessage += $"\r\n\r\nTable: {tableProgress.Table.TableName}\r\nError: {tableProgress.ErrorMessage}";
            }

        }

        #endregion

        #region Nested Types

        private class TableConversion
        {

            public System.Configuration.ConnectionStringSettings SourceConnectionString { get; set; }
            public System.Configuration.ConnectionStringSettings TargetConnectionString { get; set; }

            public IList<DatabaseTools.Models.TableModel> SourceTables { get; set; }
            public IList<DatabaseTools.Models.TableModel> TargetTables { get; set; }

            public bool UseBulkCopy { get; set; }
            public bool TrimStrings { get; set; }

        }

        #endregion


    }
}
