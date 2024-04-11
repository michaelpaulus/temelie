using System;
using System.Windows;
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;
using Cornerstone.Database.UI;
using Cornerstone.Database.ViewModels;
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
        services.AddTransient<IDatabaseStructureService, DatabaseStructureService>();
        services.AddTransient<IDatabaseExecutionService, DatabaseExecutionService>();
        services.AddTransient<IScriptService, ScriptService>();
        services.AddTransient<ITableConverterService, TableConverterService>();
        services.AddTransient<IDatabaseModelService, DatabaseModelService>();
        services.AddTransient<IConnectionCreatedNotification, Providers.Mssql.DefaultAzureCredentialConnectionCreatedNotification>();

        services.AddTransient<ConvertViewModel>();
        services.AddTransient<CreateScriptsViewModel>();
        services.AddTransient<ExecuteScriptsViewModel>();
        services.AddTransient<TableMappingViewModel>();
        
        ServiceProvider = services.BuildServiceProvider();

        base.OnStartup(e);
    }

}
