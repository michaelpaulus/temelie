
using System.Data.Common;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;
[ExportTransient(typeof(IDatabaseModelService))]
public class DatabaseModelService : IDatabaseModelService
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly IDatabaseExecutionService _databaseExecutionService;

    public DatabaseModelService(IDatabaseFactory databaseFactory,
        IDatabaseExecutionService databaseExecutionService)
    {
        _databaseFactory = databaseFactory;
        _databaseExecutionService = databaseExecutionService;
    }

    public DatabaseModel CreateModel(ConnectionStringModel connectionString, DatabaseModelOptions options = null)
    {
        using (var conn = _databaseExecutionService.CreateDbConnection(connectionString))
        {
            return CreateModel(conn, options);
        }
    }

    public DatabaseModel CreateModel(DbConnection connection, DatabaseModelOptions options = null)
    {
        if (options is null)
        {
            options = new DatabaseModelOptions()
            {
                ObjectFilter = "",
                ExcludeDoubleUnderscoreObjects = true
            };
        }

        var tableColumns = GetTableColumns(connection);

        var tables = GetTables(connection, options, tableColumns);

        var viewColumns = GetViewColumns(connection);

        var views = GetViews(connection, options, viewColumns);

        var defs = GetDefinitions(connection, options, views);

        var tableNames = tables.Select(i => i.TableName).ToList();
        var viewNames = views.Select(i => i.TableName).ToList();

        var fks = GetForeignKeys(connection).Where(i => tableNames.Contains(i.TableName) && tableNames.Contains(i.ReferencedTableName)).ToList();

        var conts = GetCheckConstraints(connection).Where(i => tableNames.Contains(i.TableName) ).ToList();
        var indexes = GetIndexes(connection).Where(i => tableNames.Contains(i.TableName)).ToList();
        var triggers = GetTriggers(connection, options).Where(i => tableNames.Contains(i.TableName) || viewNames.Contains(i.TableName)).ToList();
        var secPol = GetSecurityPolicies(connection);

        return new DatabaseModel(
            tables,
            views,
            indexes,
            triggers,
            fks,
            conts,
            defs,
            secPol);
    }

    private IEnumerable<ColumnModel> GetViewColumns(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);
        return provider.GetViewColumns(connection);
    }

    private IEnumerable<TableModel> GetViews(DbConnection connection, DatabaseModelOptions options, IEnumerable<ColumnModel> columns)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        var views = provider.GetViews(connection, columns).OrderBy(i => i.TableName).ToList();

        if (!(string.IsNullOrEmpty(options.ObjectFilter)))
        {
            views = (
                from i in views
                where i.TableName.ToLower().Contains(options.ObjectFilter.ToLower())
                select i).ToList();
        }

        if (options.ExcludeDoubleUnderscoreObjects)
        {
            views = views.Where(i => !i.TableName.StartsWith("__")).ToList();
        }

        return views.ToList();
    }

    private IEnumerable<DefinitionModel> GetDefinitions(DbConnection connection, DatabaseModelOptions options, IEnumerable<TableModel> views)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        var definitions = provider.GetDefinitions(connection);

        foreach (var def in definitions.Where(i => i.Type == "VIEW"))
        {
            def.View = views.FirstOrDefault(i => i.SchemaName.EqualsIgnoreCase(def.SchemaName) && i.TableName.EqualsIgnoreCase(def.DefinitionName));
        }

        var filteredList = definitions.ToList();

        if (!(string.IsNullOrEmpty(options.ObjectFilter)))
        {
            filteredList = (
                from i in filteredList
                where i.DefinitionName.ToLower().Contains(options.ObjectFilter.ToLower())
                select i).ToList();
        }

        if (options.ExcludeDoubleUnderscoreObjects)
        {
            filteredList = filteredList.Where(i => !i.DefinitionName.StartsWith("__")).ToList();
        }

        return filteredList.ToList();
    }

    private IEnumerable<Models.ForeignKeyModel> GetForeignKeys(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        return (from i in provider.GetForeignKeys(connection) orderby i.TableName, i.ForeignKeyName select i).ToList();
    }

    private IEnumerable<Models.CheckConstraintModel> GetCheckConstraints(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        return (from i in provider.GetCheckConstraints(connection) orderby i.TableName, i.CheckConstraintName select i).ToList();
    }

    private IEnumerable<Models.IndexModel> GetIndexes(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);
        return (from i in provider.GetIndexes(connection) orderby i.TableName, i.IndexName select i).ToList();
    }

    private IEnumerable<Models.TableModel> GetTables(DbConnection connection, DatabaseModelOptions options, IEnumerable<ColumnModel> columns)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        var tables = provider.GetTables(connection, columns).OrderBy(i => i.TableName).ToList();

        if (!(string.IsNullOrEmpty(options.ObjectFilter)))
        {
            tables = (
                from i in tables
                where i.TableName.ToLower().Contains(options.ObjectFilter.ToLower())
                select i).ToList();
        }

        if (options.ExcludeDoubleUnderscoreObjects)
        {
            tables = tables.Where(i => !i.TableName.StartsWith("__")).ToList();
        }
        return tables;
    }

    private IEnumerable<ColumnModel> GetTableColumns(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        return provider.GetTableColumns(connection);
    }

    private IEnumerable<TriggerModel> GetTriggers(DbConnection connection, DatabaseModelOptions options)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);

        var triggers = provider.GetTriggers(connection).OrderBy(i => i.TriggerName).ToList();

        if (!(string.IsNullOrEmpty(options.ObjectFilter)))
        {
            triggers = (
                from i in triggers
                where i.TriggerName.ToLower().Contains(options.ObjectFilter.ToLower())
                select i).ToList();
        }

        if (options.ExcludeDoubleUnderscoreObjects)
        {
            triggers = triggers.Where(i => !i.TriggerName.StartsWith("__")).ToList();
        }

        return triggers.ToList();
    }

    private IEnumerable<SecurityPolicyModel> GetSecurityPolicies(DbConnection connection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connection);
        return provider.GetSecurityPolicies(connection).OrderBy(i => i.PolicyName).ToList();
    }
}
