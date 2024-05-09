using System;
using Temelie.Database.UI;
using Temelie.Database.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Database;

public partial class ExecuteScripts
{
    public ExecuteScripts()
    {
        this.InitializeComponent();
        SubscribeToEvents();
        this.DataContext = this.ViewModel;

        this.DatabaseConnection.ViewModel.LoadConnections();

    }

    private ViewModels.ExecuteScriptsViewModel _viewModel;
    public ViewModels.ExecuteScriptsViewModel ViewModel
    {
        get
        {
            if (this._viewModel == null)
            {
                this._viewModel = ServiceProviderApplication.ServiceProvider.GetService<ExecuteScriptsViewModel>();
            }
            return this._viewModel;
        }
    }

    #region Event Handlers

    private void BrowseButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {

        using (System.Windows.Forms.FolderBrowserDialog fd = new System.Windows.Forms.FolderBrowserDialog())
        {
            if (!(string.IsNullOrEmpty(this.ViewModel.ScriptPath)) && System.IO.Directory.Exists(this.ViewModel.ScriptPath))
            {
                fd.SelectedPath = this.ViewModel.ScriptPath;
            }
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ViewModel.ScriptPath = fd.SelectedPath;
            }
        }
    }

    private void DatabaseConnection_SelectionChanged(object sender, EventArgs e)
    {
        this.ViewModel.DatabaseConnectionString = this.DatabaseConnection.ConnectionString;
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

        BrowseButton.Click += BrowseButton_Click;
        DatabaseConnection.ViewModel.SelectionChanged += DatabaseConnection_SelectionChanged;
    }

}
