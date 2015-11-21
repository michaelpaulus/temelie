
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

namespace DatabaseTools
{
    public partial class CreateScripts
    {

        private System.ComponentModel.BackgroundWorker ExecuteBackgroundWorker = new System.ComponentModel.BackgroundWorker { WorkerReportsProgress = true };

        #region Event Handlers

        private void CreateScripts_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(string.IsNullOrEmpty(Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath)))
            {
                this.ScriptPathTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath;
            }
        }

        private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog())
            {

                if (!(string.IsNullOrEmpty(this.ScriptPathTextBox.Text)) && System.IO.Directory.Exists(this.ScriptPathTextBox.Text))
                {
                    fd.SelectedPath = this.ScriptPathTextBox.Text;
                }

                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.ScriptPathTextBox.Text = fd.SelectedPath;
                }
            }
        }

        private void ExecuteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ExecuteButton.Visibility = System.Windows.Visibility.Collapsed;
            this.Progress.ProgressBar.Value = 0;
            this.Progress.ProgressLabel.Text = "";
            this.Progress.Visibility = System.Windows.Visibility.Visible;
            this.ExecuteBackgroundWorker.RunWorkerAsync(new object[] { this.DatabaseConnection.ConnectionString, this.FilesListBox.SelectedItems.OfType<System.IO.FileInfo>().ToList(), this.ObjectFilterTextBox.Text });
        }

        private void ScriptPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            List<System.IO.FileInfo> itemsSource = new List<System.IO.FileInfo>();
            string strObjectFilter = string.Empty;

            if (System.IO.Directory.Exists(this.ScriptPathTextBox.Text))
            {
                Configuration.Preferences.UserSettingsContext.Current.CreateScriptsPath = this.ScriptPathTextBox.Text;
                Configuration.Preferences.UserSettingsContext.Save();

                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPathTextBox.Text);

                itemsSource = (
                    from System.IO.FileInfo fi in di.GetFiles("*.sql")
                    orderby fi.Name
                    select fi).ToList();

            }

            this.ObjectFilterTextBox.Text = strObjectFilter.ToLower().Replace(" ", "_").Replace("-", "");

            this.FilesListBox.ItemsSource = itemsSource;

            this.FilesListBox.SelectAll();
        }

        private void ExecuteBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            Version fileVersion = null;

            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            fileVersion = new Version(fvi.FileMajorPart, fvi.FileMinorPart, fvi.FileBuildPart, fvi.FilePrivatePart);

            DatabaseTools.Processes.Script.CreateScripts((System.Configuration.ConnectionStringSettings)args[0], (List<System.IO.FileInfo>)args[1], (System.ComponentModel.BackgroundWorker)sender, fileVersion, args[2].ToString());
        }

        private void ExecuteBackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 0)
            {
                this.Progress.ProgressBar.Value = e.ProgressPercentage;
            }
            this.Progress.ProgressLabel.Text = e.UserState.ToString();
        }

        private void ExecuteBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            this.ContinueButton.Visibility = System.Windows.Visibility.Visible;
            this.Progress.ProgressBar.Value = 0;
            this.Progress.ProgressLabel.Text = "";
            this.Progress.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.ContinueButton.Visibility = System.Windows.Visibility.Collapsed;
            this.ExecuteButton.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion




        public CreateScripts()
        {
            this.InitializeComponent();
            SubscribeToEvents();
        }


        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            this.Loaded += CreateScripts_Loaded;
            BrowseButton.Click += BrowseButton_Click;
            ExecuteButton.Click += ExecuteButton_Click;
            ScriptPathTextBox.TextChanged += ScriptPathTextBox_TextChanged;
            ExecuteBackgroundWorker.DoWork += ExecuteBackgroundWorker_DoWork;
            ExecuteBackgroundWorker.ProgressChanged += ExecuteBackgroundWorker_ProgressChanged;
            ExecuteBackgroundWorker.RunWorkerCompleted += ExecuteBackgroundWorker_RunWorkerCompleted;
            ContinueButton.Click += ContinueButton_Click;
        }

    }

}