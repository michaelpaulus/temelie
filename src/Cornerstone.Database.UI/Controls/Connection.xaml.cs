using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database.Controls;

/// <summary>
/// Interaction logic for Connection.xaml
/// </summary>
public partial class DatabaseConnection : UserControl
{
    private readonly IDatabaseFactory _databaseFactory;

    public DatabaseConnection()
    {
        _databaseFactory = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetService<IDatabaseFactory>();
        InitializeComponent();
        this.DataContext = new ViewModels.ConnectionViewModel();
    }

    public ViewModels.ConnectionViewModel ViewModel
    {
        get
        {
            return (ViewModels.ConnectionViewModel)DataContext;
        }
    }

    public bool IsSource
    {
        get
        {
            return this.ViewModel.IsSource;
        }
        set
        {
            this.ViewModel.IsSource = value;
        }
    }

    public System.Configuration.ConnectionStringSettings ConnectionString
    {
        get
        {
            System.Configuration.ConnectionStringSettings selectedConnectionString = null;

            var selectedConnection = this.ViewModel.SelectedConnection;

            if (selectedConnection != null)
            {
                selectedConnectionString = new System.Configuration.ConnectionStringSettings();

                selectedConnectionString.Name = selectedConnection.Name;
                selectedConnectionString.ProviderName = (from i in Models.DatabaseConnectionType.GetDatabaseConnectionTypes() where i.ConnectionType == selectedConnection.ConnectionType select i.ProviderName).FirstOrDefault();
                selectedConnectionString.ConnectionString = selectedConnection.ConnectionString;
            }

            return selectedConnectionString;
        }
    }

    public static IList<Cornerstone.Database.Models.TableModel> GetTables(IDatabaseFactory databaseFactory, System.Configuration.ConnectionStringSettings connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        var databaseType = databaseFactory.GetDatabaseProvider(connectionString);
        var database = new Services.DatabaseService(databaseFactory, databaseType);

        try
        {
            using (var conn = database.CreateDbConnection(connectionString))
            {
                var columns = database.GetTableColumns(conn);
                tables = database.GetTables(conn, columns);
            }
        }
        catch
        {

        }

        return tables;
    }

    public static IList<Cornerstone.Database.Models.TableModel> GetViews(IDatabaseFactory databaseFactory, System.Configuration.ConnectionStringSettings connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        var databaseType = databaseFactory.GetDatabaseProvider(connectionString);
        var database = new Services.DatabaseService(databaseFactory, databaseType);

        try
        {
            using (var conn = database.CreateDbConnection(connectionString))
            {
                var columns = database.GetTableColumns(conn);
                tables = database.GetTables(conn, columns);
            }
        }
        catch
        {

        }

        return tables;
    }

}
