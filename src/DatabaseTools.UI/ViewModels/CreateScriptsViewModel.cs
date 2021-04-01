using DatabaseTools.Processes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.ViewModels
{
    public class CreateScriptsViewModel : ViewModel
    {

        public CreateScriptsViewModel()
        {
            this.CreateScriptsCommand = new Command(() =>
            {
                this.CreateScripts();
            });
            this.ScriptPath = Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath;
        }

        #region Properties

        public string ScriptPath { get; set; }
        public string ObjectFilter { get; set; }

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

            var progress = new Progress<ScriptProgress>(tableProgress =>
            {
                this.ReportProgress(tableProgress);
            });

            Task.Factory.StartNew(() =>
            {
                this.CreateScriptsInternal(progress);
            }).ContinueWith((task) =>
            {
                this.ProgressPercentage = 0;
                this.ProgressStatus = "Completed";
                this.CreateScriptsCommand.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CreateScriptsInternal(IProgress<ScriptProgress> progress)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPath);
            Script.CreateScriptsIndividual(this.DatabaseConnectionString, di, Models.DatabaseType.MicrosoftSQLServer, progress, this.ObjectFilter);
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
}
