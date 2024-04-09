
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database;

public partial class CreateMergeScript
{

    private readonly System.ComponentModel.BackgroundWorker ScriptBackgroundWorker = new System.ComponentModel.BackgroundWorker();

    private Cornerstone.Database.Models.DatabaseModel SourceDatabase;
    private readonly IDatabaseFactory _databaseFactory;

    public CreateMergeScript()
    {

        _databaseFactory = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetService<IDatabaseFactory>();
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

        Cornerstone.Database.Models.DatabaseModel targetDatabase = new Cornerstone.Database.Models.DatabaseModel(_databaseFactory, targetConnectionString);

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
                var database = new Cornerstone.Database.Models.DatabaseModel(_databaseFactory, connectionString);
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

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = System.Threading.Tasks.Task.Factory.StartNew(action, this.SourceDatabaseConnection.ConnectionString);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    public void DataGridCell_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        DataGridCell cell = sender as DataGridCell;
        if (cell != null && cell.IsEditing && !cell.IsReadOnly)
        {
            DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
            dataGrid?.CommitEdit();
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
                    sbValue.Append('|');
                }
                sbValue.Append(table.TableName);
            }

            bool blnFoundInList = false;

            for (int intIndex = 0; intIndex < Configuration.Preferences.UserSettingsContext.Current.MergeTableLists.Count; intIndex++)
            {
                if (Configuration.Preferences.UserSettingsContext.Current.MergeTableLists[intIndex].Split(':')[0] == strName)
                {
                    Configuration.Preferences.UserSettingsContext.Current.MergeTableLists[intIndex] = $"{strName}:{sbValue.ToString()}";
                    blnFoundInList = true;
                }
            }

            if (!blnFoundInList)
            {
                Configuration.Preferences.UserSettingsContext.Current.MergeTableLists.Add($"{strName}:{sbValue.ToString()}");
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

        GenerateScriptButton.Click += GenerateScriptButton_Click;
        ScriptBackgroundWorker.DoWork += ScriptBackgroundWorker_DoWork;
        ScriptBackgroundWorker.RunWorkerCompleted += ScriptBackgroundWorker_RunWorkerCompleted;
        SourceDatabaseConnection.ViewModel.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
        SaveListButton.Click += SaveListButton_Click;
        SaveListComboBox.SelectionChanged += SaveListComboBox_SelectionChanged;
    }

}
