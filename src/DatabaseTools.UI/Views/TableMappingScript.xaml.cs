
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
    public partial class TableMappingScript
    {

        public TableMappingScript()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            this.DataContext = this.ViewModel;
            SubscribeToEvents();

            this.SourceDatabaseConnection.ViewModel.LoadConnections();
            this.TargetDatabaseConnection.ViewModel.LoadConnections();

        }

        #region Properties

        private ViewModels.TableMappingViewModel _viewModel;
        public ViewModels.TableMappingViewModel ViewModel
        {
            get
            {
                if (this._viewModel == null)
                {
                    this._viewModel = new ViewModels.TableMappingViewModel();
                }
                return this._viewModel;
            }
        }

        #endregion

        #region Event Handlers


        private void SourceDatabaseConnection_SelectionChanged(object sender, EventArgs e)
        {
            this.ViewModel.SourceDatabaseConnectionString = this.SourceDatabaseConnection.ConnectionString;
        }

        private void TargetDatabaseConnection_SelectionChanged(object sender, EventArgs e)
        {
            this.ViewModel.TargetDatabaseConnectionString = this.TargetDatabaseConnection.ConnectionString;
        }
      
        private void CreateScriptButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ResultsDialog dialog = new ResultsDialog();
            dialog.ResultsTextBox.Text = this.ViewModel.CreateSql();
            dialog.ShowDialog();
        }

        private void CreateXmlButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ResultsDialog dialog = new ResultsDialog();
            dialog.ResultsTextBox.Text = this.ViewModel.CreateXml();
            dialog.ShowDialog();
        }

        #endregion
        
        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            SourceDatabaseConnection.ViewModel.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
            TargetDatabaseConnection.ViewModel.SelectionChanged += TargetDatabaseConnection_SelectionChanged;
            CreateScriptButton.Click += CreateScriptButton_Click;
            CreateXmlButton.Click += CreateXmlButton_Click;
        }

    }

}