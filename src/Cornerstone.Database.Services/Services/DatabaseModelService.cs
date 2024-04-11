
using System.Data.Common;
using Cornerstone.Database.Extensions;
using Cornerstone.Database.Models;
using Cornerstone.Database.Providers;
using Cornerstone.DependencyInjection;

namespace Cornerstone.Database.Services;
[ExportTransient(typeof(IDatabaseModelService))]
public class DatabaseModelService : IDatabaseModelService
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IDatabaseStructureService _databaseStructureService;

    public DatabaseModelService(IDatabaseFactory databaseFactory,
        IDatabaseExecutionService databaseExecutionService,
        IDatabaseStructureService databaseStructureService)
    {
        _databaseFactory = databaseFactory;
        _databaseExecutionService = databaseExecutionService;
        _databaseStructureService = databaseStructureService;
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

        var provider = _databaseFactory.GetDatabaseProvider(connection);

        var databaseName = provider.GetDatabaseName(connection.ConnectionString);
        var tableColumns = GetTableColumns(connection);

        var tables = GetTables(options, connection, tableColumns);

        var viewColumns = GetViewColumns(connection);

        var views = GetViews(options, connection, viewColumns);

        var defs = GetDefinitions(options, connection, views);

        var tableNames = tables.Select(i => i.TableName).ToList();
        var viewNames = views.Select(i => i.TableName).ToList();

        var fks = GetForeignKeys(connection, tableNames);

        var conts = GetCheckConstraints(connection, tableNames);
        var indexes = GetIndexes(connection, tableNames);
        var triggers = GetTriggers(options, connection, tableNames, viewNames);
        var secPol = GetSecurityPolicies(connection);

        return new DatabaseModel(databaseName,
            provider.Name,
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
        return _databaseStructureService.GetViewColumns(connection);
    }

    private IEnumerable<TableModel> GetViews(DatabaseModelOptions options, DbConnection connection, IEnumerable<ColumnModel> columns)
    {

        var views = _databaseStructureService.GetViews(connection, columns).OrderBy(i => i.TableName).ToList();

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

    private IEnumerable<DefinitionModel> GetDefinitions(DatabaseModelOptions options, DbConnection connection, IEnumerable<TableModel> views)
    {

        var definitions = _databaseStructureService.GetDefinitions(connection);

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

    private IEnumerable<Models.ForeignKeyModel> GetForeignKeys(DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in _databaseStructureService.GetForeignKeys(connection, tableNames) orderby i.TableName, i.ForeignKeyName select i).ToList();
    }

    private IEnumerable<Models.CheckConstraintModel> GetCheckConstraints(DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in _databaseStructureService.GetCheckConstraints(connection, tableNames) orderby i.TableName, i.CheckConstraintName select i).ToList();
    }

    private IEnumerable<Models.IndexModel> GetIndexes(DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in _databaseStructureService.GetIndexes(connection, tableNames, null) orderby i.TableName, i.IndexName select i).ToList();
    }

    private IEnumerable<Models.TableModel> GetTables(DatabaseModelOptions options, DbConnection connection, IEnumerable<ColumnModel> columns)
    {
        var tables = _databaseStructureService.GetTables(connection, columns).OrderBy(i => i.TableName).ToList();

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
        return _databaseStructureService.GetTableColumns(connection);
    }

    private IEnumerable<TriggerModel> GetTriggers(DatabaseModelOptions options, DbConnection connection, IEnumerable<string> tableNames, IEnumerable<string> viewNames)
    {
        var triggers = _databaseStructureService.GetTriggers(connection, tableNames, viewNames, options.ObjectFilter).OrderBy(i => i.TriggerName).ToList();

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
        return _databaseStructureService.GetSecurityPolicies(connection).OrderBy(i => i.PolicyName).ToList();
    }
}
