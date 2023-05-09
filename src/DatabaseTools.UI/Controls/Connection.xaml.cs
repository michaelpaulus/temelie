using DatabaseTools.Processes;
using DatabaseTools.Providers;
using DatabaseTools.UI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DatabaseTools.Controls
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class DatabaseConnection : UserControl
    {
        private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
        private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

        public DatabaseConnection()
        {
            _databaseProviders = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
            _connectionCreatedNotifications = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IConnectionCreatedNotification>();
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
                    selectedConnectionString.ProviderName = (from i in Models.DatabaseConnectionType.GetDatabaseConnectionTypes() where i.ConnectionType ==  selectedConnection.ConnectionType select i.ProviderName).FirstOrDefault();
                    selectedConnectionString.ConnectionString = selectedConnection.ConnectionString;
                }

                return selectedConnectionString;
            }
        }

        public static IList<DatabaseTools.Models.TableModel> GetTables(System.Configuration.ConnectionStringSettings connectionString,
            IEnumerable<IDatabaseProvider> databaseProviders,
            IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
        {
            IList<DatabaseTools.Models.TableModel> tables = new List<DatabaseTools.Models.TableModel>();

            var databaseType = Database.GetDatabaseType(connectionString);
            var database = new Database(databaseType, databaseProviders, connectionCreatedNotifications);

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

        public static IList<DatabaseTools.Models.TableModel> GetViews(System.Configuration.ConnectionStringSettings connectionString, 
            IEnumerable<IDatabaseProvider> databaseProviders,
            IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
        {
            IList<DatabaseTools.Models.TableModel> tables = new List<DatabaseTools.Models.TableModel>();

            var databaseType = Database.GetDatabaseType(connectionString);
            var database = new Database(databaseType, databaseProviders, connectionCreatedNotifications);

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
}
