using DatabaseTools.Processes;
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

        public static IList<DatabaseTools.Models.TableModel> GetTables(System.Configuration.ConnectionStringSettings connectionString)
        {
            IList<DatabaseTools.Models.TableModel> tables = new List<DatabaseTools.Models.TableModel>();

            try
            {
                using (var conn = Database.CreateDbConnection(connectionString))
                {
                    var columns = DatabaseTools.Processes.Database.GetTableColumns(conn);
                    tables = DatabaseTools.Processes.Database.GetTables(conn, columns);
                }
            }
            catch
            {

            }

            return tables;
        }

        public static IList<DatabaseTools.Models.TableModel> GetViews(System.Configuration.ConnectionStringSettings connectionString)
        {
            IList<DatabaseTools.Models.TableModel> tables = new List<DatabaseTools.Models.TableModel>();

            try
            {
                using (var conn = Database.CreateDbConnection(connectionString))
                {
                    var columns = DatabaseTools.Processes.Database.GetTableColumns(conn);
                    tables = DatabaseTools.Processes.Database.GetTables(conn, columns);
                }
            }
            catch
            {

            }

            return tables;
        }

    }
}
