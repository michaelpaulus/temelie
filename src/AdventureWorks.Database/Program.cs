
using Cornerstone.Database.Providers;
using Cornerstone.Database.Services;
using AdventureWorks.Server.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var connectionString = "Data Source=(local);Initial Catalog=AdventureWorks2022;Integrated Security=True;Encrypt=False;Application Name=Cornerstone.Database";

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<IDatabaseFactory, DatabaseFactory>();
builder.Services.AddTransient<IDatabaseProvider, Cornerstone.Database.Providers.Mssql.DatabaseProvider>();
builder.Services.AddTransient<IConnectionCreatedNotification, Cornerstone.Database.Providers.Mssql.DefaultAzureCredentialConnectionCreatedNotification>();
builder.Services.AddTransient<IScriptService, ScriptService>();

using var host = builder.Build();

var scriptService = host.Services.GetRequiredService<IScriptService>();

scriptService.CreateScripts(
    new System.Configuration.ConnectionStringSettings() { ConnectionString = connectionString },
    new DirectoryInfo(Path.Combine(DirectoryConfig.RepoDirectory, "src", "Cornerstone.Example.Database")),
    new Progress<ScriptProgress>(Console.WriteLine));
