
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
	public partial class ExecuteScripts
	{

		private System.ComponentModel.BackgroundWorker ExecuteBackgroundWorker = new System.ComponentModel.BackgroundWorker {WorkerReportsProgress = true};

        public ExecuteScripts()
        {
            this.InitializeComponent();
            SubscribeToEvents();
        }

#region Methods

		protected void UpdateScripts()
		{
			List<System.IO.FileInfo> list = new List<System.IO.FileInfo>();

			if (!(string.IsNullOrEmpty(this.ScriptPathTextBox.Text)) && System.IO.Directory.Exists(this.ScriptPathTextBox.Text))
			{
				System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.ScriptPathTextBox.Text);
				list.AddRange((
				    from i in di.GetFiles("*.sql", System.IO.SearchOption.AllDirectories)
				    select i));
			}

			if (!(string.IsNullOrEmpty(this.CustomerScriptPathTextBox.Text)) && System.IO.Directory.Exists(this.CustomerScriptPathTextBox.Text))
			{
				System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.CustomerScriptPathTextBox.Text);
				list.AddRange((
				    from i in di.GetFiles("*.sql", System.IO.SearchOption.AllDirectories)
				    select i));
			}

			if (!(string.IsNullOrEmpty(this.PartnerScriptPathTextBox.Text)) && System.IO.Directory.Exists(this.PartnerScriptPathTextBox.Text))
			{
				System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(this.PartnerScriptPathTextBox.Text);
				list.AddRange((
				    from i in di.GetFiles("*.sql", System.IO.SearchOption.AllDirectories)
				    select i));
			}

			this.FilesListBox.ItemsSource = (
			    from i in list
			    orderby i.Name
			    select i).ToList();

			this.FilesListBox.SelectAll();
		}

#endregion

#region Event Handlers

		private void MergeButton_Click(object sender, RoutedEventArgs e)
		{
			var scripts = (
			    from i in this.FilesListBox.SelectedItems.OfType<System.IO.FileInfo>()
			    select i.FullName).ToList();
			Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog {Filter = "SQL Files (.sql)|*.sql", InitialDirectory = this.ScriptPathTextBox.Text};
			if (sfd.ShowDialog().GetValueOrDefault())
			{
				DatabaseTools.Processes.Script.MergeScripts(scripts, sfd.FileName);
			}
		}

		private void ExecuteScripts_Loaded(object sender, System.Windows.RoutedEventArgs e)
		{
			if (!(string.IsNullOrEmpty(Configuration.Preferences.UserSettingsContext.Current.ScriptsPath)))
			{
				this.ScriptPathTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.ScriptsPath;
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

		private void ScriptPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Configuration.Preferences.UserSettingsContext.Current.ScriptsPath = this.ScriptPathTextBox.Text;
			Configuration.Preferences.UserSettingsContext.Save();
			this.UpdateScripts();
		}

		private void CustomerScriptsBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			using (System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (!(string.IsNullOrEmpty(this.CustomerScriptPathTextBox.Text)) && System.IO.Directory.Exists(this.CustomerScriptPathTextBox.Text))
				{
					fd.SelectedPath = this.CustomerScriptPathTextBox.Text;
				}
				if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.CustomerScriptPathTextBox.Text = fd.SelectedPath;
				}
			}
		}

		private void CustomerScriptPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.UpdateScripts();
		}

		private void PartnerScriptsBrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			using (System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog())
			{
				if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.PartnerScriptPathTextBox.Text = fd.SelectedPath;
				}
			}
		}

		private void PartnerScriptPathTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			this.UpdateScripts();
		}

		private void ExecuteButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.ExecuteButton.Visibility = System.Windows.Visibility.Collapsed;
			this.MergeButton.Visibility = System.Windows.Visibility.Collapsed;
			this.Progress.ProgressBar.Value = 0;
			this.Progress.ProgressLabel.Text = "";
			this.Progress.Visibility = System.Windows.Visibility.Visible;
			this.ExecuteBackgroundWorker.RunWorkerAsync(new object[] {this.DatabaseConnection.ConnectionString, (
			    from i in this.FilesListBox.SelectedItems.OfType<System.IO.FileInfo>()
			    orderby i.Name
			    select i).ToList()});
		}

		private void ExecuteBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			object[] args = (object[])e.Argument;

			DatabaseTools.Processes.Script.ExecuteScripts((System.Configuration.ConnectionStringSettings)args[0], (List<System.IO.FileInfo>)args[1], (System.ComponentModel.BackgroundWorker)sender);
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
			this.MergeButton.Visibility = System.Windows.Visibility.Visible;
		}

#endregion

        
		private bool EventsSubscribed = false;
		private void SubscribeToEvents()
		{
			if (EventsSubscribed)
				return;
			else
				EventsSubscribed = true;

			MergeButton.Click += MergeButton_Click;
			this.Loaded += ExecuteScripts_Loaded;
			BrowseButton.Click += BrowseButton_Click;
			ScriptPathTextBox.TextChanged += ScriptPathTextBox_TextChanged;
			CustomerScriptsBrowseButton.Click += CustomerScriptsBrowseButton_Click;
			CustomerScriptPathTextBox.TextChanged += CustomerScriptPathTextBox_TextChanged;
			PartnerScriptsBrowseButton.Click += PartnerScriptsBrowseButton_Click;
			PartnerScriptPathTextBox.TextChanged += PartnerScriptPathTextBox_TextChanged;
			ExecuteButton.Click += ExecuteButton_Click;
			ExecuteBackgroundWorker.DoWork += ExecuteBackgroundWorker_DoWork;
			ExecuteBackgroundWorker.ProgressChanged += ExecuteBackgroundWorker_ProgressChanged;
			ExecuteBackgroundWorker.RunWorkerCompleted += ExecuteBackgroundWorker_RunWorkerCompleted;
			ContinueButton.Click += ContinueButton_Click;
		}

	}

}