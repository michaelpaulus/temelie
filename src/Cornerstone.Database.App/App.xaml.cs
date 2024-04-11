using System;
using System.Windows;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cornerstone.Database.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IServiceProviderApplication
{
    public IServiceProvider ServiceProvider => _host.Services;

    private IHost _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Configuration.ConfigureStartup();

        builder.Services.RegisterExports();

        builder.Services.ConfigureStartup();

        _host = builder.Build();

        _host.Services.ConfigureStartup();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            _host.Dispose();
            _host = null;
        }
        base.OnExit(e);
    }

}
