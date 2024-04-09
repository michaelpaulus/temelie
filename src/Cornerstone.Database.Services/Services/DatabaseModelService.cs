
using System.Data.Common;
using Cornerstone.Database.Extensions;
using Cornerstone.Database.Models;
using Cornerstone.Database.Providers;

namespace Cornerstone.Database.Services;
public class DatabaseModelService
{
    private readonly IDatabaseFactory _databaseFactory;

    public DatabaseModelService(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public DatabaseModel CreateModel(System.Configuration.ConnectionStringSettings connectionString, DatabaseModelOptions options = null)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connectionString);
        var service = new DatabaseService(_databaseFactory, provider);
        using (var conn = service.CreateDbConnection(connectionString))
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
        var service = new DatabaseService(_databaseFactory, provider);

        var databaseName = provider.GetDatabaseName(connection.ConnectionString);
        var quoteCharacterEnd = provider.QuoteCharacterEnd;
        var quoteCharacterStart = provider.QuoteCharacterStart;

        var tableColumns = GetTableColumns(service, connection);

        var tables = GetTables(service, options, connection, tableColumns);

        var viewColumns = GetViewColumns(service, connection);

        var views = GetViews(service, options, connection, viewColumns);

        var defs = GetDefinitions(service, options, connection, views);

        var tableNames = tables.Select(i => i.TableName).ToList();
        var viewNames = views.Select(i => i.TableName).ToList();

        var fks = GetForeignKeys(service, connection, tableNames);

        var conts = GetCheckConstraints(service, connection, tableNames);
        var indexes = GetIndexes(service, connection, tableNames);
        var triggers = GetTriggers(service, options, connection, tableNames, viewNames);
        var secPol = GetSecurityPolicies(service, connection);

        return new DatabaseModel(databaseName, quoteCharacterStart, quoteCharacterEnd,
            tables, tableColumns, views, viewColumns,
            indexes, triggers, fks, conts,
            defs, secPol);
    }

    private IEnumerable<ColumnModel> GetViewColumns(DatabaseService databaseService, DbConnection connection)
    {
        return databaseService.GetViewColumns(connection);
    }

    private IEnumerable<TableModel> GetViews(DatabaseService databaseService, DatabaseModelOptions options, DbConnection connection, IEnumerable<ColumnModel> columns)
    {

        var views = databaseService.GetViews(connection, columns).OrderBy(i => i.TableName).ToList();

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

    private IEnumerable<DefinitionModel> GetDefinitions(DatabaseService databaseService, DatabaseModelOptions options, DbConnection connection, IEnumerable<TableModel> views)
    {

        var definitions = databaseService.GetDefinitions(connection);

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

    private IEnumerable<Models.ForeignKeyModel> GetForeignKeys(DatabaseService databaseService, DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in databaseService.GetForeignKeys(connection, tableNames) orderby i.TableName, i.ForeignKeyName select i).ToList();
    }

    private IEnumerable<Models.CheckConstraintModel> GetCheckConstraints(DatabaseService databaseService, DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in databaseService.GetCheckConstraints(connection, tableNames) orderby i.TableName, i.CheckConstraintName select i).ToList();
    }

    private IEnumerable<Models.IndexModel> GetIndexes(DatabaseService databaseService, DbConnection connection, IEnumerable<string> tableNames)
    {
        return (from i in databaseService.GetIndexes(connection, tableNames, null) orderby i.TableName, i.IndexName select i).ToList();
    }

    private IEnumerable<Models.TableModel> GetTables(DatabaseService databaseService, DatabaseModelOptions options, DbConnection connection, IEnumerable<ColumnModel> columns)
    {
        var tables = databaseService.GetTables(connection, columns).OrderBy(i => i.TableName).ToList();
       
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

    private IEnumerable<ColumnModel> GetTableColumns(DatabaseService databaseService, DbConnection connection)
    {
        return databaseService.GetTableColumns(connection);
    }

    private IEnumerable<TriggerModel> GetTriggers(DatabaseService databaseService, DatabaseModelOptions options, DbConnection connection, IEnumerable<string> tableNames, IEnumerable<string> viewNames)
    {
        var triggers = databaseService.GetTriggers(connection, tableNames, viewNames, options.ObjectFilter).OrderBy(i => i.TriggerName).ToList();

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

    private IEnumerable<SecurityPolicyModel> GetSecurityPolicies(DatabaseService databaseService, DbConnection connection)
    {
        return databaseService.GetSecurityPolicies(connection).OrderBy(i => i.PolicyName).ToList();
    }
}
