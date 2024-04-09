using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace Cornerstone.Database;

public partial class CreateInsertScript
{

    private readonly IDatabaseFactory _databaseFactory;

    public CreateInsertScript()
    {
        _databaseFactory = ((IServiceProviderApplication)Application.Current).ServiceProvider.GetService<IDatabaseFactory>();
        this.InitializeComponent();
        SubscribeToEvents();

        this.DatabaseConnection.ViewModel.LoadConnections();
    }

    private void DatabaseConnection_SelectionChanged(object sender, EventArgs e)
    {
        this.TableComboBox.ItemsSource = null;
        Action<object> action = new Action<object>((object obj) =>
        {
            try
            {
                var connectionString = (System.Configuration.ConnectionStringSettings)obj;
                var tables = Controls.DatabaseConnection.GetTables(_databaseFactory, connectionString);
                var views = Controls.DatabaseConnection.GetViews(_databaseFactory, connectionString);

                var list = tables.Union(views).ToList();

                Dispatcher.Invoke(new Action<IEnumerable>((IEnumerable results) =>
                {
                    this.TableComboBox.ItemsSource = results;
                }), list);
            }
            catch
            {

            }
        });

#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _ = System.Threading.Tasks.Task.Factory.StartNew(action, DatabaseConnection.ConnectionString);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
    }

    private void GenerateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Cornerstone.Database.Models.DatabaseModel database = new Cornerstone.Database.Models.DatabaseModel(_databaseFactory, this.DatabaseConnection.ConnectionString);
        this.ResultTextBox.Text = database.GetInsertScript(this.TableComboBox.Text, WhereTextBox.Text);
    }

    private void SaveToFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog();
        dialog.Filter = "Sql Files (*.sql)|*.sql|All Files (*.*)|*.*";
        dialog.DefaultExt = "sql";
        if (dialog.ShowDialog().GetValueOrDefault())
        {
            File.WriteAllText(dialog.FileName, ResultTextBox.Text, System.Text.Encoding.UTF8);
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

        DatabaseConnection.ViewModel.SelectionChanged += DatabaseConnection_SelectionChanged;
        GenerateScriptButton.Click += GenerateScriptButton_Click;
        SaveToFile.Click += SaveToFile_Click;
    }

}
