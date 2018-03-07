using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DatabaseTools.Processes;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;
using DatabaseTools.Models;

namespace DatabaseTools.ViewModels
{
    public class ConvertViewModel : ViewModel
    {

        public ConvertViewModel()
        {
            this.ThreadCount = 5;
            this.UseBulkCopy = true;
            this.BatchSize = 10000;

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
        public int BatchSize { get; set; }
        public bool TrimStrings { get; set; }

        private Stopwatch Stopwatch { get; set; }

        #endregion

        #region Methods

        private TableConverterSettings GetTableConverterSettings()
        {
            var selectedTables = (from i in this.Tables where i.Selected select i).ToList();

            TableConverterSettings settings = new TableConverterSettings();

            using (var conn = Database.CreateDbConnection(TargetDatabaseConnectionString))
            {
                var targetColumns = DatabaseTools.Processes.Database.GetTableColumns(conn);
                var targetTables = DatabaseTools.Processes.Database.GetTables(conn, targetColumns);
                settings.TargetTables = targetTables;
            }


            settings.SourceConnectionString = this.SourceDatabaseConnectionString;
            settings.TargetConnectionString = this.TargetDatabaseConnectionString;

            settings.SourceTables = selectedTables;
           

            settings.UseBulkCopy = this.UseBulkCopy;
            settings.BatchSize = this.BatchSize;
            settings.TrimStrings = this.TrimStrings;

            return settings;
        }

        public void Convert()
        {
            this.ErrorMessage = $"Started: {DateTime.Now.ToString()}\r\n";
            this.ConvertCommand.IsEnabled = false;
            this.ToggleAllCommand.IsEnabled = false;

            this.Stopwatch = new Stopwatch();
            this.Stopwatch.Start();

            var progress = new Progress<TableProgress>(tableProgress =>
            {
                this.ReportProgress(tableProgress);
            });

            var settings = this.GetTableConverterSettings();

            Task.Factory.StartNew(() =>
            {
                var converter = new TableConverter();
                converter.ConvertTables(settings,
                    progress,
                    this.ThreadCount);

            }).ContinueWith((task) =>
            {
                this.Stopwatch.Stop();

                this.ErrorMessage += $"\r\n\r\nFinished: {DateTime.Now.ToString()}";
                this.ErrorMessage += $"\r\nTotal Minutes: {this.Stopwatch.Elapsed.TotalMinutes}";
                this.ConvertCommand.IsEnabled = true;
                this.ToggleAllCommand.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void UpdateTables()
        {
            try
            {
                var tables = Controls.DatabaseConnection.GetTables(this.SourceDatabaseConnectionString);
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
                tableProgress.Table.ErrorMessage += tableProgress.ErrorMessage;
                this.ErrorMessage += $"\r\n\r\nTable: {tableProgress.Table.TableName}\r\nError: {tableProgress.ErrorMessage}";
            }

        }

        #endregion




    }
}
