using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface IDatabaseStructureService
{
    IEnumerable<CheckConstraintModel> GetCheckConstraints(DbConnection connection, IEnumerable<string> tables);
    IEnumerable<DefinitionModel> GetDefinitions(DbConnection connection);
    IEnumerable<ForeignKeyModel> GetForeignKeys(DbConnection connection, IEnumerable<string> tables);
    IEnumerable<IndexModel> GetIndexes(DbConnection connection, IEnumerable<string> tables, bool? isPrimaryKey = null);
    IEnumerable<SecurityPolicyModel> GetSecurityPolicies(DbConnection connection);
    IList<ColumnModel> GetTableColumns(DbConnection connection);
    IEnumerable<TableModel> GetTables(DbConnection connection, IEnumerable<ColumnModel> columns, bool withBackup = false);
    IEnumerable<TriggerModel> GetTriggers(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> views, string objectFilter);
    IList<ColumnModel> GetViewColumns(DbConnection connection);
    IEnumerable<TableModel> GetViews(DbConnection connection, IEnumerable<ColumnModel> columns);
}