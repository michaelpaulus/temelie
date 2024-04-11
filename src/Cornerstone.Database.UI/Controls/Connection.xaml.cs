using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Cornerstone.Database.Models;
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

    public ConnectionStringModel ConnectionString
    {
        get
        {
            return this.ViewModel.SelectedConnection;
        }
    }

    public static IList<Cornerstone.Database.Models.TableModel> GetTables(IDatabaseFactory databaseFactory, ConnectionStringModel connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        var databaseType = databaseFactory.GetDatabaseProvider(connectionString);
        var database = new Services.DatabaseService(databaseFactory, databaseType);

        try
        {
            using (var conn = database.CreateDbConnection(connectionString.ConnectionString))
            {
                var columns = database.GetTableColumns(conn);
                tables = database.GetTables(conn, columns).ToList();
            }
        }
        catch
        {

        }

        return tables;
    }

    public static IList<Cornerstone.Database.Models.TableModel> GetViews(IDatabaseFactory databaseFactory, ConnectionStringModel connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        var databaseType = databaseFactory.GetDatabaseProvider(connectionString);
        var database = new Services.DatabaseService(databaseFactory, databaseType);

        try
        {
            using (var conn = database.CreateDbConnection(connectionString.ConnectionString))
            {
                var columns = database.GetTableColumns(conn);
                tables = database.GetTables(conn, columns).ToList();
            }
        }
        catch
        {

        }

        return tables;
    }

}
