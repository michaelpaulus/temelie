using System.Data;
using System.Data.Common;
using Temelie.Database.Models;
using Temelie.Database.Services;

namespace Temelie.Database.Providers;

public interface IDatabaseProvider
{

    string Name { get; }

    string QuoteCharacterStart { get; }
    string QuoteCharacterEnd { get; }
    string DefaultConnectionString { get; }

    IEnumerable<TableModel> GetTables(DbConnection connection, IEnumerable<ColumnModel> columns);
    IEnumerable<TableModel> GetViews(DbConnection connection, IEnumerable<ColumnModel> columns);

    IEnumerable<TriggerModel> GetTriggers(DbConnection connection);
    IEnumerable<ForeignKeyModel> GetForeignKeys(DbConnection connection);
    IEnumerable<CheckConstraintModel> GetCheckConstraints(DbConnection connection);
    IEnumerable<DefinitionModel> GetDefinitions(DbConnection connection);
    IEnumerable<ColumnModel> GetTableColumns(DbConnection connection);
    IEnumerable<ColumnModel> GetViewColumns(DbConnection connection);
    IEnumerable<IndexModel> GetIndexes(DbConnection connection);
    IEnumerable<SecurityPolicyModel> GetSecurityPolicies(DbConnection connection);

    IDatabaseObjectScript GetScript(CheckConstraintModel model);
    IDatabaseObjectScript GetScript(DefinitionModel model);
    IDatabaseObjectScript GetScript(ForeignKeyModel model);
    IDatabaseObjectScript GetScript(IndexModel model, bool isView);
    IDatabaseObjectScript GetScript(SecurityPolicyModel model);
    IDatabaseObjectScript GetScript(TableModel model);
    IDatabaseObjectScript GetScript(TriggerModel model);
    string GetRenameScript(TableModel model, string newTableName);
    IDatabaseObjectScript GetColumnScript(ColumnModel column);

    void SetReadTimeout(System.Data.Common.DbCommand sourceCommand);
    string TransformConnectionString(string connectionString);

    void UpdateParameter(DbParameter parameter, Models.ColumnModel column);

    Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType);
    DbConnection CreateConnection();

    bool TryHandleColumnValueLoadException(Exception ex, Models.ColumnModel column, out object value);

    void ConvertBulk(
       TableConverterService service,
       IProgress<TableProgress> progress,
       Models.TableModel sourceTable,
       IDataReader sourceReader,
       int sourceRowCount,
       Models.TableModel targetTable,
       DbConnection targetConnection,
       bool trimStrings,
       int batchSize,
       bool useTransaction = true,
       bool validateTargetTable = true);

    string GetDatabaseName(string connectionString);
    bool SupportsConnection(DbConnection connection);
    int GetRowCount(DbCommand connection, string schemaName, string tableName);
    string GetSelectStatement(string schemaName, string tableName, IEnumerable<string> columns);

}
