using System;
using System.Threading.Tasks;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.ViewModels;
[ExportTransient(typeof(ExecuteScriptsViewModel))]
public class ExecuteScriptsViewModel : ViewModel
{

    private readonly IDatabaseFactory _databaseFactory;
    private readonly IScriptService _scriptService;

    public ExecuteScriptsViewModel(IDatabaseFactory databaseFactory,
        IScriptService scriptService)
    {
        _databaseFactory = databaseFactory;
        _scriptService = scriptService;
        this.ExecuteScriptsCommand = new Command(this.ExecuteScripts);
        this.ScriptPath = Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath;
    }

    #region Properties

    public string ScriptPath { get; set; }

    public ConnectionStringModel DatabaseConnectionString { get; set; }

    public int ProgressPercentage { get; set; }
    public string ProgressStatus { get; set; }
    public string ErrorMessage { get; set; }
    public bool ContinueOnError { get; set; }

    #endregion

    #region Commands

    public Command ExecuteScriptsCommand { get; set; }

    #endregion

    #region Methods

    public void ExecuteScripts()
    {
        this.ProgressPercentage = 0;
        this.ProgressStatus = "";
        this.ErrorMessage = $"Started: {DateTime.Now:yyyy-MM-ddTHH:mm:ss}";
        this.ExecuteScriptsCommand.IsEnabled = false;

        var progress = new Progress<ScriptProgress>(this.ReportProgress);

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = Task.Factory.StartNew(() =>
        {
            this.ExecuteScriptsInternal(progress);
        }).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    if (task.Exception.InnerException != null)
                    {
                        this.ErrorMessage = task.Exception.InnerException.Message;
                    }
                    else
                    {
                        this.ErrorMessage = task.Exception.Message;
                    }
                }
                else
                {
                    this.ErrorMessage = "The task didn't complete";
                }
            }
            else
            {
                this.ProgressPercentage = 0;
                this.ErrorMessage += $"Completed: {DateTime.Now:yyyy-MM-ddTHH:mm:ss}";
                this.ProgressStatus = "Completed";
            }

            this.ExecuteScriptsCommand.IsEnabled = true;
        }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    private void ExecuteScriptsInternal(IProgress<ScriptProgress> progress)
    {
        try
        {
            _scriptService.ExecuteScripts(DatabaseConnectionString, new System.IO.DirectoryInfo(ScriptPath), progress, ContinueOnError);
        }
        catch
        {
            //ignore errors in the UI because they are reported to the user
        }
    }

    private void ReportProgress(ScriptProgress progress)
    {
        this.ProgressPercentage = progress.ProgressPercentage;
        this.ProgressStatus = progress.ProgressStatus;
        if (!string.IsNullOrEmpty(progress.ErrorMessage))
        {
            if (string.IsNullOrEmpty(this.ErrorMessage))
            {
                ErrorMessage = "";
            }
            else
            {
                ErrorMessage += "\n";
            }
            ErrorMessage += $"{progress.ProgressStatus}: {progress.ErrorMessage}";
        }
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(ScriptPath):

                Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath = this.ScriptPath;
                Configuration.Preferences.UserSettingsContext.Save();
                break;

        }
    }

    #endregion
}
