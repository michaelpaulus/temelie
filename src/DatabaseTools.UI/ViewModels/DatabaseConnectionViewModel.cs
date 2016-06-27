using DatabaseTools.Configuration.Preferences;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.ViewModels
{
    public class DatabaseConnectionViewModel : ViewModel
    {

        public DatabaseConnectionViewModel()
        {
            this.AddCommand = new Command(() =>
            {
                this.Add();
            });

            this.DeleteCommand = new Command(() =>
            {
                this.Delete();
            });

            this.SaveCommand = new Command(() =>
            {
                this.Save();
            });

            this.LoadConnections();

        }

        public Command AddCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command SaveCommand { get; set; }

        public Models.DatabaseConnection SelectedConnection { get; set; }

        private ObservableCollection<Models.DatabaseConnection> _connnections;
        public ObservableCollection<Models.DatabaseConnection> Connections
        {
            get
            {
                if (_connnections == null)
                {
                    _connnections = new ObservableCollection<Models.DatabaseConnection>();
                }
                return _connnections;
            }
        }

        private ObservableCollection<Models.DatabaseConnectionType> _databaseConnectionTypes;
        public ObservableCollection<Models.DatabaseConnectionType> ConnectionTypes
        {
            get
            {
                if (_databaseConnectionTypes == null)
                {
                    _databaseConnectionTypes = new ObservableCollection<Models.DatabaseConnectionType>();
                }
                return _databaseConnectionTypes;
            }
        }

        private void Add()
        {
            var connection = new Models.DatabaseConnection() { Name = "Connection" };
            this.Connections.Add(connection);
            this.SelectedConnection = connection;
        }

        private void Delete()
        {
            if (this.SelectedConnection != null)
            {
                this.Connections.Remove(this.SelectedConnection);
                this.SelectedConnection = this.Connections.FirstOrDefault();
            }
        }

        private void Save()
        {
            ConnectionSettingsContext.Current.Connections.Clear();
            foreach (var item in this.Connections.OrderBy(i => i.Name))
            {
                ConnectionSettingsContext.Current.Connections.Add(item);
            }
            ConnectionSettingsContext.Save();
        }

        private void LoadConnections()
        {
            this.SelectedConnection = null;
            this.Connections.Clear();
            this.ConnectionTypes.Clear();

            foreach (var item in Models.DatabaseConnectionType.GetDatabaseConnectionTypes())
            {
                this.ConnectionTypes.Add(item);
            }

            foreach (var item in ConnectionSettingsContext.Current.Connections.OrderBy(i => i.Name))
            {
                this.Connections.Add(item);
            }

            this.SelectedConnection = this.Connections.FirstOrDefault();

        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(SelectedConnection):
                    this.DeleteCommand.IsEnabled = SelectedConnection != null;
                    break;
            }
        }

    }
}
