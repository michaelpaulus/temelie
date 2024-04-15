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

    DataTable GetTables(DbConnection connection);
    DataTable GetViews(DbConnection connection);

    DataTable GetTriggers(DbConnection connection);
    DataTable GetForeignKeys(DbConnection connection);
    DataTable GetCheckConstraints(DbConnection connection);
    DataTable GetDefinitions(DbConnection connection);
    DataTable GetDefinitionDependencies(DbConnection connection);

    DataTable GetTableColumns(DbConnection connection);
    DataTable GetViewColumns(DbConnection connection);
    DataTable GetIndexeBucketCounts(DbConnection connection);
    DataTable GetIndexes(DbConnection connection);
    DataTable GetSecurityPolicies(DbConnection connection);

    IDatabaseObjectScript GetScript(CheckConstraintModel model);
    IDatabaseObjectScript GetScript(DefinitionModel model);
    IDatabaseObjectScript GetScript(ForeignKeyModel model);
    IDatabaseObjectScript GetScript(IndexModel model);
    IDatabaseObjectScript GetScript(SecurityPolicyModel model);
    IDatabaseObjectScript GetScript(TableModel model);
    IDatabaseObjectScript GetScript(TriggerModel model);

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

}
