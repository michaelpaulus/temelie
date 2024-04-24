using Temelie.Database.Services;
using AdventureWorks.Server.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Temelie.Database.Models;
using Temelie.DependencyInjection;

var connectionString = "Data Source=(local);Initial Catalog=AdventureWorks2022;Integrated Security=True;Encrypt=False;Application Name=Temelie.Database";

using var context = new StartupConfigurationContext();

var builder = Host.CreateApplicationBuilder(args);

context.Configure(builder.Configuration);

context.Configure(builder.Services, builder.Configuration);

using var host = builder.Build();

context.Configure(host.Services);

var scriptService = host.Services.GetRequiredService<IScriptService>();

scriptService.CreateScripts(
    new ConnectionStringModel() { Name= "AdventureWorks", DatabaseProviderName = Temelie.Database.Providers.Mssql.DatabaseProvider.ProviderName, ConnectionString = connectionString },
    new DirectoryInfo(Path.Combine(DirectoryConfig.RepoDirectory, "src", "AdventureWorks.Database")),
    new Progress<ScriptProgress>(Console.WriteLine));
