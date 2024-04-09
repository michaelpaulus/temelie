using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;

namespace Cornerstone.Database.ViewModels;

public class ConvertViewModel : ViewModel
{

    private readonly IDatabaseFactory _databaseFactory;

    public ConvertViewModel(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
        this.ThreadCount = 5;
        this.UseBulkCopy = true;
        this.BatchSize = 10000;

        this.ConvertCommand = new Command(this.Convert);
        this.ToggleAllCommand = new Command(this.ToggleAll);
    }

    #region Commands

    public Command ConvertCommand { get; set; }
    public Command ToggleAllCommand { get; set; }

    #endregion

    #region Properties

    private ObservableCollection<Cornerstone.Database.Models.TableModel> _tables;
    public ObservableCollection<Cornerstone.Database.Models.TableModel> Tables
    {
        get
        {
            if (this._tables == null)
            {
                this._tables = new ObservableCollection<Cornerstone.Database.Models.TableModel>();
            }
            return this._tables;
        }
    }

    public System.Configuration.ConnectionStringSettings SourceDatabaseConnectionString { get; set; }
    public System.Configuration.ConnectionStringSettings TargetDatabaseConnectionString { get; set; }

    public string ErrorMessage { get; set; }
    public int ThreadCount { get; set; }
    public bool UseBulkCopy { get; set; }
    public bool UseTransaction { get; set; } = true;
    public int BatchSize { get; set; }
    public bool TrimStrings { get; set; }

    private Stopwatch Stopwatch { get; set; }

    #endregion

    #region Methods

    private TableConverterSettings GetTableConverterSettings()
    {
        var selectedTables = (from i in this.Tables where i.Selected select i).ToList();

        TableConverterSettings settings = new TableConverterSettings();

        var targetDatabaseType = _databaseFactory.GetDatabaseProvider(TargetDatabaseConnectionString);
        var targetDatabase = new Services.DatabaseService(_databaseFactory, targetDatabaseType);

        using (var conn = targetDatabase.CreateDbConnection(TargetDatabaseConnectionString))
        {
            var targetColumns = targetDatabase.GetTableColumns(conn);
            var targetTables = targetDatabase.GetTables(conn, targetColumns);
            settings.TargetTables = targetTables;
        }

        settings.SourceConnectionString = this.SourceDatabaseConnectionString;
        settings.TargetConnectionString = this.TargetDatabaseConnectionString;

        settings.SourceTables = selectedTables;

        settings.UseTransaction = this.UseTransaction;
        settings.UseBulkCopy = this.UseBulkCopy;
        settings.BatchSize = this.BatchSize;
        settings.TrimStrings = this.TrimStrings;

        return settings;
    }

    public void Convert()
    {
        this.ErrorMessage = $"Started: {DateTime.Now}\r\n";
        this.ConvertCommand.IsEnabled = false;
        this.ToggleAllCommand.IsEnabled = false;

        this.Stopwatch = new Stopwatch();
        this.Stopwatch.Start();

        var progress = new Progress<TableProgress>(this.ReportProgress);

        var settings = this.GetTableConverterSettings();

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = Task.Factory.StartNew(() =>
        {
            var converter = new TableConverterService(_databaseFactory);
            converter.ConvertTables(settings,
                progress,
                this.ThreadCount);

        }).ContinueWith((task) =>
        {
            this.Stopwatch.Stop();

            this.ErrorMessage += $"\r\n\r\nFinished: {DateTime.Now}";
            this.ErrorMessage += $"\r\nTotal Minutes: {this.Stopwatch.Elapsed.TotalMinutes}";
            this.ConvertCommand.IsEnabled = true;
            this.ToggleAllCommand.IsEnabled = true;
        }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    public void UpdateTables()
    {
        try
        {
            var tables = Controls.DatabaseConnection.GetTables(_databaseFactory, this.SourceDatabaseConnectionString);
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
