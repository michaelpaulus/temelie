
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
	public partial class CreateChangeScript
	{

		private System.ComponentModel.BackgroundWorker ScriptBackgroundWorker = new System.ComponentModel.BackgroundWorker();

		public CreateChangeScript()
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

			this.ScriptBackgroundWorker.RunWorkerAsync(new object[] {this.SourceDatabaseConnection.ConnectionString, this.TargetDatabaseConnection.ConnectionString});
		}

		private void ScriptBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			var args = (object[])e.Argument;

			var sourceConnectionString = (System.Configuration.ConnectionStringSettings)args[0];
			var targetConnectionString = (System.Configuration.ConnectionStringSettings)args[1];

			DatabaseTools.Models.DatabaseModel sourceDatabase = new DatabaseTools.Models.DatabaseModel(sourceConnectionString) { ExcludeDoubleUnderscoreObjects = true };
			DatabaseTools.Models.DatabaseModel targetDatabase = new DatabaseTools.Models.DatabaseModel(targetConnectionString) { ExcludeDoubleUnderscoreObjects = true };

			e.Result = targetDatabase.GetChangeScript(sourceDatabase);
		}

		private void ScriptBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			this.ResultTextBox.Text = e.Result.ToString();
			this.GenerateScriptButton.IsEnabled = true;
		}




		private bool EventsSubscribed = false;
		private void SubscribeToEvents()
		{
			if (EventsSubscribed)
				return;
			else
				EventsSubscribed = true;

			GenerateScriptButton.Click += GenerateScriptButton_Click;
			ScriptBackgroundWorker.DoWork += ScriptBackgroundWorker_DoWork;
			ScriptBackgroundWorker.RunWorkerCompleted += ScriptBackgroundWorker_RunWorkerCompleted;
		}

	}

}