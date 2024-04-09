using System;
using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Database.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IServiceProviderApplication
{
    public IServiceProvider ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddTransient<IDatabaseFactory, DatabaseFactory>();
        services.AddTransient<IDatabaseProvider, Providers.Mssql.DatabaseProvider>();
        services.AddTransient<IDatabaseProvider, Providers.MySql.DatabaseProvider>();
        services.AddTransient<IConnectionCreatedNotification, Providers.Mssql.DefaultAzureCredentialConnectionCreatedNotification>();

        ServiceProvider = services.BuildServiceProvider();

        base.OnStartup(e);
    }

}
