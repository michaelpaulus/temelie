
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
using Cornerstone.Database.Processes;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database
{
    public partial class CreateMergeScript
    {

        private System.ComponentModel.BackgroundWorker ScriptBackgroundWorker = new System.ComponentModel.BackgroundWorker();

        private Cornerstone.Database.Models.DatabaseModel SourceDatabase;
        private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
        private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

        public CreateMergeScript()
        {
            _databaseProviders = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IDatabaseProvider>();
            _connectionCreatedNotifications = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetServices<IConnectionCreatedNotification>();
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            this.SourceDatabaseConnection.IsSource = true;
            this.ResultTextBox.Visibility = System.Windows.Visibility.Collapsed;

            List<string> list = new List<string>();

            if (Configuration.Preferences.UserSettingsContext.Current.MergeTableLists == null)
            {
                Configuration.Preferences.UserSettingsContext.Current.MergeTableLists = new System.Collections.Specialized.StringCollection();
                Configuration.Preferences.UserSettingsContext.Save();
            }

            foreach (var item in Configuration.Preferences.UserSettingsContext.Current.MergeTableLists)
            {
                list.Add(item.Split(':')[0]);
            }

            this.SaveListComboBox.ItemsSource = list;
            SubscribeToEvents();

            this.SourceDatabaseConnection.ViewModel.LoadConnections();

        }

        private void GenerateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ResultTextBox.Visibility = System.Windows.Visibility.Visible;
            this.TablesDataGrid.Visibility = System.Windows.Visibility.Collapsed;

            this.GenerateScriptButton.IsEnabled = false;

            this.ScriptBackgroundWorker.RunWorkerAsync(this.TargetDatabaseConnection.ConnectionString);
        }

        private void ScriptBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var targetConnectionString = (System.Configuration.ConnectionStringSettings)e.Argument;

            Cornerstone.Database.Models.DatabaseModel targetDatabase = new Cornerstone.Database.Models.DatabaseModel(targetConnectionString, _databaseProviders, _connectionCreatedNotifications);

            e.Result = this.SourceDatabase.GetMergeScript(targetDatabase);
        }

        private void ScriptBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.ResultTextBox.Text = e.Result.ToString();
            this.GenerateScriptButton.IsEnabled = true;
        }

        private void SourceDatabaseConnection_SelectionChanged(object sender, System.EventArgs e)
        {
            this.TablesDataGrid.ItemsSource = null;

            Action<object> action = new Action<object>((object obj) =>
            {
                try
                {
                    var connectionString = (System.Configuration.ConnectionStringSettings)obj;
                    var database = new Cornerstone.Database.Models.DatabaseModel(connectionString, _databaseProviders, _connectionCreatedNotifications);
                    var tables = database.Tables;
                    Dispatcher.Invoke(new Action<Cornerstone.Database.Models.DatabaseModel>((Cornerstone.Database.Models.DatabaseModel results) =>
                    {
                        this.SourceDatabase = results;
                        this.TablesDataGrid.ItemsSource = this.SourceDatabase.Tables;
                        foreach (var table in this.SourceDatabase.Tables)
                        {
                            table.Selected = false;
                        }
                    }), database);
                }
                catch 
                {

                }
            });

            System.Threading.Tasks.Task.Factory.StartNew(action, this.SourceDatabaseConnection.ConnectionString);
        }

        public void DataGridCell_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && cell.IsEditing && !cell.IsReadOnly)
            {
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    dataGrid.CommitEdit();
                }
            }
        }

        public void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                        {
                            cell.IsSelected = true;
                        }
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return default(T);
        }

        private void SaveListButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!(string.IsNullOrEmpty(this.SaveListComboBox.Text)))
            {
                string strName = this.SaveListComboBox.Text;
                System.Text.StringBuilder sbValue = new System.Text.StringBuilder();
                foreach (var table in (
                    from i in this.SourceDatabase.Tables
                    where i.Selected
                    select i))
                {
                    if (sbValue.Length > 0)
                    {
                        sbValue.Append("|");
                    }
                    sbValue.Append(table.TableName);
                }

                bool blnFoundInList = false;

                for (int intIndex = 0; intIndex < Configuration.Preferences.UserSettingsContext.Current.MergeTableLists.Count; intIndex++)
                {
                    if (Configuration.Preferences.UserSettingsContext.Current.MergeTableLists[intIndex].Split(':')[0] == strName)
                    {
                        Configuration.Preferences.UserSettingsContext.Current.MergeTableLists[intIndex] = string.Format("{0}:{1}", strName, sbValue.ToString());
                        blnFoundInList = true;
                    }
                }

                if (!blnFoundInList)
                {
                    Configuration.Preferences.UserSettingsContext.Current.MergeTableLists.Add(string.Format("{0}:{1}", strName, sbValue.ToString()));
                }
            }
            Configuration.Preferences.UserSettingsContext.Save();
        }

        private void SaveListComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.SaveListComboBox.SelectedIndex >= 0 && this.SaveListComboBox.SelectedValue != null)
            {

                foreach (var item in Configuration.Preferences.UserSettingsContext.Current.MergeTableLists)
                {
                    string strName = item.Split(':')[0];
                    string strValue = item.Split(':')[1];
                    if (strName.Equals(this.SaveListComboBox.SelectedValue.ToString()))
                    {
                        List<string> tableNames = new List<string>();

                        tableNames.AddRange(strValue.Split('|'));

                        foreach (var table in this.SourceDatabase.Tables)
                        {
                            table.Selected = tableNames.Contains(table.TableName);
                        }

                        break;
                    }

                }
            }

        }


        
        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            GenerateScriptButton.Click += GenerateScriptButton_Click;
            ScriptBackgroundWorker.DoWork += ScriptBackgroundWorker_DoWork;
            ScriptBackgroundWorker.RunWorkerCompleted += ScriptBackgroundWorker_RunWorkerCompleted;
            SourceDatabaseConnection.ViewModel.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
            SaveListButton.Click += SaveListButton_Click;
            SaveListComboBox.SelectionChanged += SaveListComboBox_SelectionChanged;
        }

    }

}