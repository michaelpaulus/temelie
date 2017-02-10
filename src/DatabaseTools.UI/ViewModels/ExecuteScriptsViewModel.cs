using DatabaseTools.Processes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.ViewModels
{
    public class ExecuteScriptsViewModel : ViewModel
    {

        public ExecuteScriptsViewModel()
        {
            this.ExecuteScriptsCommand = new Command(() =>
            {
                this.ExecuteScripts();
            });
            this.ScriptPath = Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath;
        }

        #region Properties

        public string ScriptPath { get; set; }
        public string CustomScriptPath { get; set; }

        public System.Configuration.ConnectionStringSettings DatabaseConnectionString { get; set; }

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
            this.ErrorMessage = "";
            this.ExecuteScriptsCommand.IsEnabled = false;

            var progress = new Progress<ScriptProgress>(tableProgress =>
            {
                this.ReportProgress(tableProgress);
            });

            Task.Factory.StartNew(() =>
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
                    this.ProgressStatus = "Completed";
                }

                this.ExecuteScriptsCommand.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ExecuteScriptsInternal(IProgress<ScriptProgress> progress)
        {
            Script.ExecuteScripts(this.DatabaseConnectionString, this.Files, this.ContinueOnError, progress);
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

        private void UpdateScripts()
        {
            this.Files.Clear();

            List<System.IO.FileInfo> list = new List<System.IO.FileInfo>();

            if (!(string.IsNullOrEmpty(this.ScriptPath)) && System.IO.Directory.Exists(this.ScriptPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPath);
                list.AddRange((
                    from i in di.GetFiles("*.sql", System.IO.SearchOption.AllDirectories)
                    select i));
            }

            if (!(string.IsNullOrEmpty(this.CustomScriptPath)) && System.IO.Directory.Exists(this.CustomScriptPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPath);
                list.AddRange((
                    from i in di.GetFiles("*.sql", System.IO.SearchOption.AllDirectories)
                    select i));
            }

            foreach (var file in (from i in list
                                  orderby
                                      i.DirectoryName,
                                      i.Name
                                  select i))
            {
                this.Files.Add(file);
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

                    this.UpdateScripts();

                    break;

            }
        }

        #endregion
    }
}
