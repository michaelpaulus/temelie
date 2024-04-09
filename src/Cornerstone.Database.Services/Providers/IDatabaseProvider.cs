using System.Configuration;
using System.Data;
using System.Data.Common;
using Cornerstone.Database.Services;

namespace Cornerstone.Database.Providers;

public interface IDatabaseProvider
{
    string QuoteCharacterStart { get; }
    string QuoteCharacterEnd { get; }

    void SetReadTimeout(System.Data.Common.DbCommand sourceCommand);
    string TransformConnectionString(string connectionString);

    void UpdateParameter(DbParameter parameter, Models.ColumnModel column);

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

    Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType);
    DbProviderFactory CreateProvider();

    bool TryHandleColumnValueLoadException(Exception ex, Models.ColumnModel column, out object value);
    DataTable GetSecurityPolicies(DbConnection connection);

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
    bool SupportsConnectionString(ConnectionStringSettings connectionStringSettings);

}
