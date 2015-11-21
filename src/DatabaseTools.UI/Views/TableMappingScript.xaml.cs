
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

        private void AddMappingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.AddMapping();
        }

        private void RemoveMappingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.RemoveMapping();
        }

        private void AutoMatchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.AutoMatchMappings();
        }

        private void MappingConverterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            string strMappingConverter = string.Empty;
            switch (strMappingConverter)
            {
                case "None":
                    this.ViewModel.ColumnMapping = string.Empty;
                    break;
                default:
                    this.ViewModel.ColumnMapping = strMappingConverter;
                    break;
            }
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

            SourceDatabaseConnection.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
            TargetDatabaseConnection.SelectionChanged += TargetDatabaseConnection_SelectionChanged;
            AddMappingButton.Click += AddMappingButton_Click;
            RemoveMappingButton.Click += RemoveMappingButton_Click;
            AutoMatchButton.Click += AutoMatchButton_Click;
            CreateScriptButton.Click += CreateScriptButton_Click;
            CreateXmlButton.Click += CreateXmlButton_Click;
        }

    }

}