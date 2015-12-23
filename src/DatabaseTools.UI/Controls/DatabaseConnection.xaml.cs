
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
    public partial class DatabaseConnection
    {

        public DatabaseConnection()
        {
            this.InitializeComponent();
            SubscribeToEvents();
        }


        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            this.Loaded += DatabaseConnection_Loaded;
            DatabaseComboBox.GotFocus += DatabaseComboBox_GotFocus;
            DatabaseComboBox.LostFocus += DatabaseComboBox_LostFocus;
            ServerTextBox.GotFocus += TextBox_GotFocus;
            ServerTextBox.LostFocus += TextBox_LostFocus;

            MssqlTextBox.GotFocus += TextBox_GotFocus;
            MssqlTextBox.LostFocus += TextBox_LostFocus;

            DSNTextBox.GotFocus += TextBox_GotFocus;
            DSNTextBox.LostFocus += TextBox_LostFocus;
            OLETextBox.GotFocus += TextBox_GotFocus;
            OLETextBox.LostFocus += TextBox_LostFocus;
            AccessOLETextBox.GotFocus += TextBox_GotFocus;
            AccessOLETextBox.LostFocus += TextBox_LostFocus;
            MySqlTextBox.GotFocus += TextBox_GotFocus;
            MySqlTextBox.LostFocus += TextBox_LostFocus;
        }

        #region Properties

        private string CurrentTextBoxText;

        public bool IsSource { get; set; }

        public System.Configuration.ConnectionStringSettings ConnectionString
        {
            get
            {
                System.Configuration.ConnectionStringSettings selectedConnectionString = null;

                if (this.ConnectionStringComboBox.SelectedItem != null)
                {
                    selectedConnectionString = (System.Configuration.ConnectionStringSettings)this.ConnectionStringComboBox.SelectedItem;
                }

                System.Configuration.ConnectionStringSettings value = null;

                if (selectedConnectionString != null)
                {
                    value = new System.Configuration.ConnectionStringSettings();

                    value.Name = selectedConnectionString.Name;
                    value.ProviderName = selectedConnectionString.ProviderName;
                    value.ConnectionString = selectedConnectionString.ConnectionString;

                    switch (DatabaseTools.Processes.Database.GetDatabaseType(value))
                    {
                        case Models.DatabaseType.MicrosoftSQLServer:
                            {
                                value.ConnectionString = this.MssqlTextBox.Text;
                                break;
                            }
                        case Models.DatabaseType.Odbc:
                            {
                                System.Data.Odbc.OdbcConnectionStringBuilder builder = new System.Data.Odbc.OdbcConnectionStringBuilder(value.ConnectionString);

                                builder.Dsn = this.DSNTextBox.Text;

                                value.ConnectionString = builder.ConnectionString;
                                break;
                            }
                        case Models.DatabaseType.OLE:
                            {
                                value.ConnectionString = this.OLETextBox.Text;
                                break;
                            }
                        case Models.DatabaseType.AccessOLE:
                            {
                                value.ConnectionString = this.AccessOLETextBox.Text;
                                break;
                            }
                        case Models.DatabaseType.MySql:
                            {
                                value.ConnectionString = this.MySqlTextBox.Text;
                                break;
                            }
                    }
                }

                return value;
            }
        }

        private string DatabasesServerText { get; set; }

        #endregion

        #region Methods

        private void InitializeDatabases()
        {
            if (!(string.Equals(this.ServerTextBox.Text, this.DatabasesServerText)) && !(string.IsNullOrEmpty(this.ServerTextBox.Text)))
            {

                this.DatabasesServerText = this.ServerTextBox.Text;

                var connectionString = new System.Configuration.ConnectionStringSettings("SQL Server", string.Format("Data Source={0};Initial Catalog=master;Integrated Security=True;Encrypt=False;Application Name=DatabaseTools", this.ServerTextBox.Text), "System.Data.SqlClient");

                var task = System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        List<string> databases = new List<string>();
                        var dataSet = DatabaseTools.Processes.Database.Execute(connectionString, "SELECT sys.databases.name FROM sys.databases ORDER BY sys.databases.name");
                        databases.AddRange((
                            from i in dataSet.Tables[0].Rows.OfType<System.Data.DataRow>()
                            select System.Convert.ToString(i[0])));
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.DatabaseComboBox.ItemsSource = databases;
                        }));

                    }
                    catch
                    {

                    }
                });


            }
        }

        public static IList<DatabaseTools.Models.TableModel> GetTables(System.Configuration.ConnectionStringSettings connectionString)
        {
            IList<DatabaseTools.Models.TableModel> tables = new List<DatabaseTools.Models.TableModel>();

            try
            {
                var columns = DatabaseTools.Processes.Database.GetTableColumns(connectionString);
                tables = DatabaseTools.Processes.Database.GetTables(connectionString, columns);
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
                var columns = DatabaseTools.Processes.Database.GetViewColumns(connectionString);
                tables = DatabaseTools.Processes.Database.GetViews(connectionString, columns);
            }
            catch
            {

            }

            return tables;
        }

        #endregion

        #region Event Handlers

        private void DatabaseConnection_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            List<System.Configuration.ConnectionStringSettings> connectionStrings = new List<System.Configuration.ConnectionStringSettings>();

            connectionStrings.Add(new System.Configuration.ConnectionStringSettings("SQL Server", "Data Source=(local)\\MSSQL2008;Initial Catalog=Database;Integrated Security=True;Encrypt=False;Application Name=DatabaseTools", "System.Data.SqlClient"));
            connectionStrings.Add(new System.Configuration.ConnectionStringSettings("MySql", "Database=database;Data Source=localhost;User Id=;Password=", "MySql.Data.MySqlClient"));
            connectionStrings.Add(new System.Configuration.ConnectionStringSettings("ODBC", "DSN=Database", "System.Data.Odbc"));
            connectionStrings.Add(new System.Configuration.ConnectionStringSettings("OLE", "Provider=;Data Source=", "System.Data.OleDb"));
            connectionStrings.Add(new System.Configuration.ConnectionStringSettings("OLE ACCESS", "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=", "System.Data.OleDb"));

            this.ConnectionStringComboBox.ItemsSource = connectionStrings;

            string strSelectedValue = string.Empty;

            if (this.IsSource)
            {
                strSelectedValue = Configuration.Preferences.UserSettingsContext.Current.SourceConnectionString;
            }
            else
            {
                strSelectedValue = Configuration.Preferences.UserSettingsContext.Current.TargetConnectionString;
            }

            if (this.IsSource)
            {
                this.MssqlTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.SourceMssql;
            }
            else
            {
                this.MssqlTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.TargetMssql;
            }

            if (this.IsSource)
            {
                this.DSNTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.SourceDSN;
            }
            else
            {
                this.DSNTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.TargetDSN;
            }

            if (this.IsSource)
            {
                this.OLETextBox.Text = Configuration.Preferences.UserSettingsContext.Current.SourceOLE;
            }
            else
            {
                this.OLETextBox.Text = Configuration.Preferences.UserSettingsContext.Current.TargetOLE;
            }

            if (this.IsSource)
            {
                this.AccessOLETextBox.Text = Configuration.Preferences.UserSettingsContext.Current.SourceAccessOLE;
            }
            else
            {
                this.AccessOLETextBox.Text = Configuration.Preferences.UserSettingsContext.Current.TargetAccessOLE;
            }

            if (this.IsSource)
            {
                this.MySqlTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.SourceMySql;
            }
            else
            {
                this.MySqlTextBox.Text = Configuration.Preferences.UserSettingsContext.Current.TargetMySql;
            }


            if (!(string.IsNullOrEmpty(strSelectedValue)))
            {
                this.ConnectionStringComboBox.SelectedValue = strSelectedValue;
                this.OnSelectionChanged(EventArgs.Empty);
            }

            this.ConnectionStringComboBox.SelectionChanged += this.ConnectionStringComboBox_SelectionChanged;
            this.MssqlTextBox.TextChanged += this.MssqlTextBox_TextChanged;
            this.ServerTextBox.TextChanged += this.ServerTextBox_TextChanged;
            this.DSNTextBox.TextChanged += this.DSNTextBox_TextChanged;
            this.OLETextBox.TextChanged += this.OLETextBox_TextChanged;
            this.AccessOLETextBox.TextChanged += this.AccessOLETextBox_TextChanged;
            this.MySqlTextBox.TextChanged += MySqlTextBox_TextChanged;
        }

        private void ConnectionStringComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ConnectionStringComboBox.SelectedValue != null)
            {
                if (this.IsSource)
                {
                    Configuration.Preferences.UserSettingsContext.Current.SourceConnectionString = this.ConnectionStringComboBox.SelectedValue.ToString();
                }
                else
                {
                    Configuration.Preferences.UserSettingsContext.Current.TargetConnectionString = this.ConnectionStringComboBox.SelectedValue.ToString();
                }
                Configuration.Preferences.UserSettingsContext.Save();
            }

            this.OnSelectionChanged(EventArgs.Empty);
        }

        private void MssqlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsSource)
            {
                Configuration.Preferences.UserSettingsContext.Current.SourceMssql = this.MssqlTextBox.Text;
            }
            else
            {
                Configuration.Preferences.UserSettingsContext.Current.TargetMssql = this.MssqlTextBox.Text;
            }

            Configuration.Preferences.UserSettingsContext.Save();
        }

        private void ServerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var csb = new System.Data.SqlClient.SqlConnectionStringBuilder(this.MssqlTextBox.Text);
            csb.DataSource = this.ServerTextBox.Text;
            this.MssqlTextBox.Text = csb.ConnectionString;
        }

        private void DSNTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsSource)
            {
                Configuration.Preferences.UserSettingsContext.Current.SourceDSN = this.DSNTextBox.Text;
            }
            else
            {
                Configuration.Preferences.UserSettingsContext.Current.TargetDSN = this.DSNTextBox.Text;
            }
            Configuration.Preferences.UserSettingsContext.Save();
        }

        private void OLETextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsSource)
            {
                Configuration.Preferences.UserSettingsContext.Current.SourceOLE = this.OLETextBox.Text;
            }
            else
            {
                Configuration.Preferences.UserSettingsContext.Current.TargetOLE = this.OLETextBox.Text;
            }
            Configuration.Preferences.UserSettingsContext.Save();
        }

        private void AccessOLETextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsSource)
            {
                Configuration.Preferences.UserSettingsContext.Current.SourceAccessOLE = this.AccessOLETextBox.Text;
            }
            else
            {
                Configuration.Preferences.UserSettingsContext.Current.TargetAccessOLE = this.AccessOLETextBox.Text;
            }
            Configuration.Preferences.UserSettingsContext.Save();
        }

        void MySqlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsSource)
            {
                Configuration.Preferences.UserSettingsContext.Current.SourceMySql = this.MySqlTextBox.Text;
            }
            else
            {
                Configuration.Preferences.UserSettingsContext.Current.TargetMySql = this.MySqlTextBox.Text;
            }
            Configuration.Preferences.UserSettingsContext.Save();
        }

        private void DatabaseComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.CurrentTextBoxText = this.DatabaseComboBox.Text;
        }

        private void DatabaseComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var csb = new System.Data.SqlClient.SqlConnectionStringBuilder(this.MssqlTextBox.Text);
            csb.InitialCatalog = this.DatabaseComboBox.Text;
            this.MssqlTextBox.Text = csb.ConnectionString;

            if (!(this.DatabaseComboBox.Text.Equals(this.CurrentTextBoxText)))
            {
                this.OnSelectionChanged(EventArgs.Empty);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.CurrentTextBoxText = ((TextBox)sender).Text;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!(((TextBox)sender).Text.Equals(this.CurrentTextBoxText)))
            {
                this.OnSelectionChanged(EventArgs.Empty);
            }
        }

        #endregion

        #region Event Raising Methods

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            bool blnServerVisible = false;
            bool blnDsnVisible = false;
            bool blnOLEVisible = false;
            bool blnAccessOLEVisible = false;
            bool blnMySqlVisible = false;

            if (this.ConnectionStringComboBox.SelectedItem != null)
            {
                var connectionString = (System.Configuration.ConnectionStringSettings)this.ConnectionStringComboBox.SelectedItem;
                switch (DatabaseTools.Processes.Database.GetDatabaseType(connectionString))
                {
                    case Models.DatabaseType.MicrosoftSQLServer:
                        string value = connectionString.ConnectionString;

                        if (!string.IsNullOrEmpty(this.MssqlTextBox.Text))
                        {
                            value = this.MssqlTextBox.Text;
                        }

                        System.Data.SqlClient.SqlConnectionStringBuilder csb;

                        try
                        {
                            csb = new System.Data.SqlClient.SqlConnectionStringBuilder(value);
                        }
                        catch
                        {
                            csb = new System.Data.SqlClient.SqlConnectionStringBuilder(connectionString.ConnectionString);
                        }  

                        this.MssqlTextBox.Text = csb.ConnectionString;
                        this.ServerTextBox.Text = csb.DataSource;

                        this.InitializeDatabases();

                        this.DatabaseComboBox.Text = csb.InitialCatalog;

                        blnServerVisible = true;
                        break;
                    case Models.DatabaseType.Odbc:
                        System.Data.Odbc.OdbcConnectionStringBuilder builder = new System.Data.Odbc.OdbcConnectionStringBuilder(connectionString.ConnectionString);
                        if (string.IsNullOrEmpty(this.DSNTextBox.Text))
                        {
                            this.DSNTextBox.Text = builder.Dsn;
                        }
                        blnDsnVisible = true;
                        break;
                    case Models.DatabaseType.OLE:
                        if (string.IsNullOrEmpty(this.OLETextBox.Text))
                        {
                            this.OLETextBox.Text = connectionString.ConnectionString;
                        }
                        blnOLEVisible = true;
                        break;
                    case Models.DatabaseType.AccessOLE:
                        if (string.IsNullOrEmpty(this.AccessOLETextBox.Text))
                        {
                            this.AccessOLETextBox.Text = connectionString.ConnectionString;
                        }
                        blnAccessOLEVisible = true;
                        break;
                    case Models.DatabaseType.MySql:
                        if (string.IsNullOrEmpty(this.MySqlTextBox.Text))
                        {
                            this.MySqlTextBox.Text = connectionString.ConnectionString;
                        }
                        blnMySqlVisible = true;
                        break;
                }
            }

            this.OLETextBox.Visibility = blnOLEVisible ? Visibility.Visible : Visibility.Collapsed;
            this.AccessOLETextBox.Visibility = blnAccessOLEVisible ? Visibility.Visible : Visibility.Collapsed;
            this.DSNTextBox.Visibility = blnDsnVisible ? Visibility.Visible : Visibility.Collapsed;
            this.ServerTextBox.Visibility = blnServerVisible ? Visibility.Visible : Visibility.Collapsed;
            this.MySqlTextBox.Visibility = blnMySqlVisible ? Visibility.Visible : Visibility.Collapsed;

            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        #endregion



    }

}