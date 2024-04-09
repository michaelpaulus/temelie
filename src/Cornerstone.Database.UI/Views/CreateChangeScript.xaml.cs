using System.Collections.Generic;
using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database;

public partial class CreateChangeScript
{

    private readonly System.ComponentModel.BackgroundWorker ScriptBackgroundWorker = new System.ComponentModel.BackgroundWorker();

    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

    public CreateChangeScript()
    {
        _databaseProviders = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
        _connectionCreatedNotifications = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IConnectionCreatedNotification>();
        // This call is required by the Windows Form Designer.
        InitializeComponent();

        // Add any initialization after the InitializeComponent() call.
        this.SourceDatabaseConnection.IsSource = true;
        SubscribeToEvents();
    }

    private void GenerateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        this.GenerateScriptButton.IsEnabled = false;

        this.ScriptBackgroundWorker.RunWorkerAsync(new object[] { this.SourceDatabaseConnection.ConnectionString, this.TargetDatabaseConnection.ConnectionString });
    }

    private void ScriptBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
    {
        var args = (object[])e.Argument;

        var sourceConnectionString = (System.Configuration.ConnectionStringSettings)args[0];
        var targetConnectionString = (System.Configuration.ConnectionStringSettings)args[1];

        Cornerstone.Database.Models.DatabaseModel sourceDatabase = new Cornerstone.Database.Models.DatabaseModel(sourceConnectionString, _databaseProviders, _connectionCreatedNotifications) { ExcludeDoubleUnderscoreObjects = true };
        Cornerstone.Database.Models.DatabaseModel targetDatabase = new Cornerstone.Database.Models.DatabaseModel(targetConnectionString, _databaseProviders, _connectionCreatedNotifications) { ExcludeDoubleUnderscoreObjects = true };

        e.Result = targetDatabase.GetChangeScript(sourceDatabase);
    }

    private void ScriptBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
    {
        this.ResultTextBox.Text = e.Result.ToString();
        this.GenerateScriptButton.IsEnabled = true;
    }

    private bool EventsSubscribed;
    private void SubscribeToEvents()
    {
        if (EventsSubscribed)
        {
            return;
        }
        else
        {
            EventsSubscribed = true;
        }

        GenerateScriptButton.Click += GenerateScriptButton_Click;
        ScriptBackgroundWorker.DoWork += ScriptBackgroundWorker_DoWork;
        ScriptBackgroundWorker.RunWorkerCompleted += ScriptBackgroundWorker_RunWorkerCompleted;
    }

}
