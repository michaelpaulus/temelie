
using System;
using System.Collections.Generic;
using System.Windows;
using Cornerstone.Database.Services;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database;

public partial class TableMappingScript
{

    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

    public TableMappingScript()
    {
        _databaseProviders = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
        _connectionCreatedNotifications = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IConnectionCreatedNotification>();

        // This call is required by the Windows Form Designer.
        InitializeComponent();

        // Add any initialization after the InitializeComponent() call.
        this.DataContext = this.ViewModel;
        SubscribeToEvents();

        this.SourceDatabaseConnection.ViewModel.LoadConnections();
        this.TargetDatabaseConnection.ViewModel.LoadConnections();

    }

    #region Properties

    private ViewModels.TableMappingViewModel _viewModel;
    public ViewModels.TableMappingViewModel ViewModel
    {
        get
        {
            if (this._viewModel == null)
            {
                this._viewModel = new ViewModels.TableMappingViewModel(_databaseProviders, _connectionCreatedNotifications);
            }
            return this._viewModel;
        }
    }

    #endregion

    #region Event Handlers

    private void SourceDatabaseConnection_SelectionChanged(object sender, EventArgs e)
    {
        this.ViewModel.SourceDatabaseConnectionString = this.SourceDatabaseConnection.ConnectionString;
    }

    private void TargetDatabaseConnection_SelectionChanged(object sender, EventArgs e)
    {
        this.ViewModel.TargetDatabaseConnectionString = this.TargetDatabaseConnection.ConnectionString;
    }

    private void CreateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ResultsDialog dialog = new ResultsDialog();
        dialog.ResultsTextBox.Text = this.ViewModel.CreateSql();
        dialog.ShowDialog();
    }

    private void CreateXmlButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ResultsDialog dialog = new ResultsDialog();
        dialog.ResultsTextBox.Text = this.ViewModel.CreateXml();
        dialog.ShowDialog();
    }

    #endregion

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

        SourceDatabaseConnection.ViewModel.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
        TargetDatabaseConnection.ViewModel.SelectionChanged += TargetDatabaseConnection_SelectionChanged;
        CreateScriptButton.Click += CreateScriptButton_Click;
        CreateXmlButton.Click += CreateXmlButton_Click;
    }

}
