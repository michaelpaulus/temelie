
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
	public partial class CompareSchemas
	{

		private System.ComponentModel.BackgroundWorker SourceBackgroundWorker = new System.ComponentModel.BackgroundWorker();
		private System.ComponentModel.BackgroundWorker TargetBackgroundWorker = new System.ComponentModel.BackgroundWorker();


		public CompareSchemas()
		{

			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// Add any initialization after the InitializeComponent() call.
			this.SourceDatabaseConnection.IsSource = true;
			SubscribeToEvents();
		}

		private void GenerateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			this.GenerateScriptButton.IsEnabled = false;

			this.SourceBackgroundWorker.RunWorkerAsync(this.SourceDatabaseConnection.ConnectionString);
			this.TargetBackgroundWorker.RunWorkerAsync(this.TargetDatabaseConnection.ConnectionString);
		}

		private void SourceBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var sourceDatabase = new DatabaseTools.Models.DatabaseModel((System.Configuration.ConnectionStringSettings)e.Argument) { ExcludeDoubleUnderscoreObjects = true };
			e.Result = sourceDatabase.GetScript("");
		}

		private void SourceBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			this.SourceResultTextBox.Text = e.Result.ToString();
			if (!this.TargetBackgroundWorker.IsBusy)
			{
				this.GenerateScriptButton.IsEnabled = true;
			}
		}

		private void TargetBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var targetDatabase = new DatabaseTools.Models.DatabaseModel((System.Configuration.ConnectionStringSettings)e.Argument) { ExcludeDoubleUnderscoreObjects = true };
			e.Result = targetDatabase.GetScript("");
		}

		private void TargetBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			this.TargetResultTextBox.Text = e.Result.ToString();
			if (!this.SourceBackgroundWorker.IsBusy)
			{
				this.GenerateScriptButton.IsEnabled = true;
			}
		}

		private void CompareButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			string sourceFile = System.IO.Path.GetTempFileName() + ".txt";
			string targetFile = System.IO.Path.GetTempFileName() + ".txt";

			System.IO.File.WriteAllText(sourceFile, this.SourceResultTextBox.Text);
			System.IO.File.WriteAllText(targetFile, this.TargetResultTextBox.Text);

			System.Diagnostics.Process process = new System.Diagnostics.Process();
			process.StartInfo.Arguments = string.Format("\"{0}\" \"{1}\"", sourceFile, targetFile);
			process.StartInfo.FileName = "WinDiff.Exe";
			process.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
			process.Start();
		}


		private bool EventsSubscribed = false;
		private void SubscribeToEvents()
		{
			if (EventsSubscribed)
				return;
			else
				EventsSubscribed = true;

			GenerateScriptButton.Click += GenerateScriptButton_Click;
			SourceBackgroundWorker.DoWork += SourceBackgroundWorker_DoWork;
			SourceBackgroundWorker.RunWorkerCompleted += SourceBackgroundWorker_RunWorkerCompleted;
			TargetBackgroundWorker.DoWork += TargetBackgroundWorker_DoWork;
			TargetBackgroundWorker.RunWorkerCompleted += TargetBackgroundWorker_RunWorkerCompleted;
			CompareButton.Click += CompareButton_Click;
		}

	}

}