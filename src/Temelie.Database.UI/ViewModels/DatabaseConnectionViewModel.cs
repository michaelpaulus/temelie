using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Temelie.Database.Configuration.Preferences;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.ViewModels;
[ExportTransient(typeof(DatabaseConnectionViewModel))]
public class DatabaseConnectionViewModel : ViewModel
{

    public DatabaseConnectionViewModel(IEnumerable<IDatabaseProvider> databaseProviders)
    {
        _databaseProviders = databaseProviders;

        Connections.ListChanged += Connections_ListChanged;

        this.AddCommand = new Command(this.Add);

        this.CopyCommand = new Command(this.Copy);
        this.DeleteCommand = new Command(this.Delete);

        this.SaveCommand = new Command(this.Save);

        this.LoadConnections();
    }

    public Command AddCommand { get; set; }
    public Command CopyCommand { get; set; }
    public Command DeleteCommand { get; set; }
    public Command SaveCommand { get; set; }

    public Models.ConnectionStringModel SelectedConnection { get; set; }

    private BindingList<Models.ConnectionStringModel> _connnections;
    public BindingList<Models.ConnectionStringModel> Connections
    {
        get
        {
            if (_connnections == null)
            {
                _connnections = new BindingList<Models.ConnectionStringModel>();
            }
            return _connnections;
        }
    }

    private ObservableCollection<Models.DatabaseProviderModel> _databaseConnectionTypes;
    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;

    public ObservableCollection<Models.DatabaseProviderModel> ConnectionTypes
    {
        get
        {
            if (_databaseConnectionTypes == null)
            {
                _databaseConnectionTypes = new ObservableCollection<Models.DatabaseProviderModel>();
            }
            return _databaseConnectionTypes;
        }
    }

    private void Add()
    {
        var connection = new Models.ConnectionStringModel() { Name = "Connection" };
        this.Connections.Add(connection);
        this.SelectedConnection = connection;
    }

    private void Copy()
    {
        if (this.SelectedConnection != null)
        {
            var newConnection = new Models.ConnectionStringModel()
            {
                Name = this.SelectedConnection.Name + " - Copy",
                DatabaseProviderName = this.SelectedConnection.DatabaseProviderName,
                ConnectionString = this.SelectedConnection.ConnectionString
            };
            this.Connections.Add(newConnection);
            this.SelectedConnection = newConnection;
        }
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

        UserSettingsContext.Current.Connections.Clear();
        foreach (var item in this.Connections.OrderBy(i => i.Name))
        {
            UserSettingsContext.Current.Connections.Add(item);
        }
        UserSettingsContext.Save();
    }

    private void LoadConnections()
    {
        this.SelectedConnection = null;
        this.Connections.Clear();
        this.ConnectionTypes.Clear();

        foreach (var item in _databaseProviders)
        {
            this.ConnectionTypes.Add(new Models.DatabaseProviderModel() { Name = item.Name, DefaultConnectionString = item.DefaultConnectionString });
        }

        foreach (var item in UserSettingsContext.Current.Connections.OrderBy(i => i.Name))
        {
            this.Connections.Add(item);
        }

        this.SelectedConnection = this.Connections.FirstOrDefault();

    }

    private void Connections_ListChanged(object sender, ListChangedEventArgs e)
    {
        switch (e.ListChangedType)
        {
            case ListChangedType.ItemChanged:
                if (e.PropertyDescriptor.Name == nameof(ConnectionStringModel.DatabaseProviderName))
                {
                    var connectionString = Connections[e.NewIndex];
                    var provider = _databaseProviders.FirstOrDefault(i => i.Name == connectionString.DatabaseProviderName);
                    if (provider != null)
                    {
                        connectionString.ConnectionString = provider.DefaultConnectionString;
                    }
                }
                break;
        }
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
