using System;
using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Providers.Mssql;
using Cornerstone.Database.Services;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cornerstone.Database.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IServiceProviderApplication
{
    public IServiceProvider ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection
        {
            new ServiceDescriptor(typeof(IDatabaseProvider), typeof(Cornerstone.Database.Providers.Mssql.DatabaseProvider), ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IDatabaseProvider), typeof(Cornerstone.Database.Providers.MySql.DatabaseProvider), ServiceLifetime.Transient),
            new ServiceDescriptor(typeof(IConnectionCreatedNotification), typeof(DefaultAzureCredentialConnectionCreatedNotification), ServiceLifetime.Transient)
        };

        ServiceProvider = services.BuildServiceProvider();

        base.OnStartup(e);
    }

}
