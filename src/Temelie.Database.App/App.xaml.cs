using System;
using System.Windows;
using Temelie.Database.UI;
using Microsoft.Extensions.Hosting;
using Temelie.DependencyInjection;

namespace Temelie.Database.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application, IServiceProviderApplication
{
    public IServiceProvider ServiceProvider => _host.Services;

    private IHost _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        using var context = new StartupConfigurationContext();
        var builder = Host.CreateApplicationBuilder(e.Args);

        context.Configure(builder.Configuration);

        context.Configure(builder.Services, builder.Configuration);

        _host = builder.Build();

        context.Configure(_host.Services);

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
