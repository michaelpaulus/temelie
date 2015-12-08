
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
    public partial class CreateInsertScript
    {

        private void DatabaseConnection_SelectionChanged(object sender, EventArgs e)
        {
            this.TableComboBox.ItemsSource = null;
            Action<object> action = new Action<object>((object obj) =>
            {
                try
                {
                    var connectionString = (System.Configuration.ConnectionStringSettings)obj;
                    var tables = DatabaseConnection.GetTables(connectionString);
                    var views = DatabaseConnection.GetViews(connectionString);

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

            System.Threading.Tasks.Task.Factory.StartNew(action, this.DatabaseConnection.ConnectionString);
        }

        private void GenerateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DatabaseTools.Models.DatabaseModel database = new DatabaseTools.Models.DatabaseModel(this.DatabaseConnection.ConnectionString);
            this.ResultTextBox.Text = database.GetInsertScript(this.TableComboBox.Text);
        }


        public CreateInsertScript()
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

            DatabaseConnection.SelectionChanged += DatabaseConnection_SelectionChanged;
            GenerateScriptButton.Click += GenerateScriptButton_Click;
        }

    }

}