
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
    public partial class AttachDatabases
    {

        public AttachDatabases()
        {

            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            this.DataContext = this.ViewModel;
            SubscribeToEvents();
        }

        #region Properties

        private ViewModels.AttachDatabaseViewModel _viewModel;
        public ViewModels.AttachDatabaseViewModel ViewModel
        {
            get
            {
                if (this._viewModel == null)
                {
                    this._viewModel = new ViewModels.AttachDatabaseViewModel();
                }
                return this._viewModel;
            }
        }

        #endregion

        #region Event Handlers

        private void DetachButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.DetachDatabases(this.SourceDatabaseConnection.ConnectionString);
        }

        private void AttachButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.AttachDatabases(this.SourceDatabaseConnection.ConnectionString);
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in this.ViewModel.Databases)
            {
                item.IsSelected = !item.IsSelected;
            }
        }

        #endregion

        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            DetachButton.Click += DetachButton_Click;
            AttachButton.Click += AttachButton_Click;
            ToggleButton.Click += ToggleButton_Click;
        }

    }

}