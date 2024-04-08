using System.Collections.Generic;
using System.Windows;
using Cornerstone.Database.Processes;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database;

public partial class AttachDatabases
{

    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

    public AttachDatabases()
    {
        _databaseProviders = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
        _connectionCreatedNotifications = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IConnectionCreatedNotification>();

        // This call is required by the designer.
        InitializeComponent();

        // Add any initialization after the InitializeComponent() call.
        this.DataContext = this.ViewModel;
    }

    #region Properties

    private ViewModels.AttachDatabaseViewModel _viewModel;
    public ViewModels.AttachDatabaseViewModel ViewModel
    {
        get
        {
            if (this._viewModel == null)
            {
                this._viewModel = new ViewModels.AttachDatabaseViewModel(_databaseProviders, _connectionCreatedNotifications);
            }
            return this._viewModel;
        }
    }

    #endregion

}
