using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Cornerstone.Database.Models;
using Cornerstone.Database.Services;

namespace Cornerstone.Database.Controls;

/// <summary>
/// Interaction logic for Connection.xaml
/// </summary>
public partial class DatabaseConnection : UserControl
{

    public DatabaseConnection()
    {
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

    public static IList<Cornerstone.Database.Models.TableModel> GetTables(IDatabaseExecutionService databaseExecutionService, IDatabaseStructureService databaseStructureService, ConnectionStringModel connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        try
        {
            using (var conn = databaseExecutionService.CreateDbConnection(connectionString))
            {
                var columns = databaseStructureService.GetTableColumns(conn);
                tables = databaseStructureService.GetTables(conn, columns).ToList();
            }
        }
        catch
        {

        }

        return tables;
    }

    public static IList<Cornerstone.Database.Models.TableModel> GetViews(IDatabaseExecutionService databaseExecutionService, IDatabaseStructureService databaseStructureService, ConnectionStringModel connectionString)
    {
        IList<Cornerstone.Database.Models.TableModel> tables = new List<Cornerstone.Database.Models.TableModel>();

        try
        {
            using (var conn = databaseExecutionService.CreateDbConnection(connectionString))
            {
                var columns = databaseStructureService.GetTableColumns(conn);
                tables = databaseStructureService.GetTables(conn, columns).ToList();
            }
        }
        catch
        {

        }

        return tables;
    }

}
