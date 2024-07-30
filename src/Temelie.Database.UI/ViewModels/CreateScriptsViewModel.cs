using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.ViewModels;
[ExportTransient(typeof(CreateScriptsViewModel))]
public class CreateScriptsViewModel : ViewModel
{

    private readonly IDatabaseFactory _databaseFactory;
    private readonly IScriptService _scriptService;
    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;

    public CreateScriptsViewModel(IDatabaseFactory databaseFactory,
        IScriptService scriptService,
        IEnumerable<IDatabaseProvider> databaseProviders)
    {
        _databaseFactory = databaseFactory;
        _scriptService = scriptService;
        _databaseProviders = databaseProviders;
        this.CreateScriptsCommand = new Command(this.CreateScripts);
        this.ScriptPath = Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath;
        foreach (var provider in _databaseProviders)
        {
            Providers.Add(provider);
        }
    }

    #region Properties

    public string ScriptPath { get; set; }
    public string ObjectFilter { get; set; }

    public ConnectionStringModel DatabaseConnectionString { get; set; }

    private ObservableCollection<System.IO.FileInfo> _files;
    public ObservableCollection<System.IO.FileInfo> Files
    {
        get
        {
            if (this._files == null)
            {
                this._files = new ObservableCollection<System.IO.FileInfo>();
            }
            return this._files;
        }
    }

    public int ProgressPercentage { get; set; }
    public string ProgressStatus { get; set; }

    public ObservableCollection<IDatabaseProvider> Providers { get; set; } = new ObservableCollection<IDatabaseProvider>();

    public IDatabaseProvider SelectedProvider { get; set; }


    #endregion

    #region Commands

    public Command CreateScriptsCommand { get; set; }

    #endregion

    #region Methods

    public void CreateScripts()
    {
        this.ProgressPercentage = 0;
        this.ProgressStatus = "";
        this.CreateScriptsCommand.IsEnabled = false;

        var progress = new Progress<ScriptProgress>(this.ReportProgress);

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = Task.Factory.StartNew(() =>
        {
            this.CreateScriptsInternal(progress);
        }).ContinueWith((task) =>
        {
            this.ProgressPercentage = 0;
            this.ProgressStatus = "Completed";
            this.CreateScriptsCommand.IsEnabled = true;
        }, TaskScheduler.FromCurrentSynchronizationContext());
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    private void CreateScriptsInternal(IProgress<ScriptProgress> progress)
    {
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPath);
        _scriptService.CreateScripts(this.DatabaseConnectionString, di, progress, this.ObjectFilter, SelectedProvider);
    }
    private void ReportProgress(ScriptProgress progress)
    {
        this.ProgressPercentage = progress.ProgressPercentage;
        this.ProgressStatus = progress.ProgressStatus;
    }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(ScriptPath):

                if (System.IO.Directory.Exists(this.ScriptPath))
                {
                    Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath = this.ScriptPath;
                    Configuration.Preferences.UserSettingsContext.Save();

                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPath);

                    this.Files.Clear();

                    if (!di.GetDirectories().Where(i => i.Name.Equals("01_Drops", StringComparison.InvariantCultureIgnoreCase)).Any())
                    {
                        foreach (var file in (from i in di.GetFiles("*.sql")
                                              orderby i.Name
                                              select i))
                        {
                            this.Files.Add(file);
                        }
                    }

                }

                break;

        }
    }

    #endregion

}
