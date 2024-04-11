using Cornerstone.Database.Services;
using AdventureWorks.Server.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Cornerstone.Database.Models;

var connectionString = "Data Source=(local);Initial Catalog=AdventureWorks2022;Integrated Security=True;Encrypt=False;Application Name=Cornerstone.Database";

var builder = Host.CreateApplicationBuilder(args);

builder.Services.RegisterExports();

using var host = builder.Build();

var scriptService = host.Services.GetRequiredService<IScriptService>();

scriptService.CreateScripts(
    new ConnectionStringModel() { Name= "AdventureWorks", DatabaseProviderName = Cornerstone.Database.Providers.Mssql.DatabaseProvider.Name, ConnectionString = connectionString },
    new DirectoryInfo(Path.Combine(DirectoryConfig.RepoDirectory, "src", "AdventureWorks.Database")),
    new Progress<ScriptProgress>(Console.WriteLine));
