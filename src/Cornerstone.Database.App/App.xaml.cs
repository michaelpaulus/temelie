using Cornerstone.Database.Processes;
using Cornerstone.Database.Providers;
using Cornerstone.Database.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Cornerstone.Database.App
{
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
                new ServiceDescriptor(typeof(IConnectionCreatedNotification), typeof(Cornerstone.Database.Providers.DefaultAzureCredentialConnectionCreatedNotification), ServiceLifetime.Transient)
            };

            ServiceProvider = services.BuildServiceProvider();

            base.OnStartup(e);
        }

    }
}
