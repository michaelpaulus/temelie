using System.Data;
using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface IDatabaseExecutionService
{
    DbCommand CreateDbCommand(DbConnection dbConnection);
    DbConnection CreateDbConnection(ConnectionStringModel connectionString);
    DataSet Execute(DbConnection connection, string sqlCommand);
    void ExecuteFile(ConnectionStringModel connectionString, string sqlCommand);
    void ExecuteFile(DbConnection connection, string sqlCommand);
    void ExecuteNonQuery(DbConnection connection, string sqlCommand);
    Task<IEnumerable<T>> GetRecordsAsync<T>(ConnectionStringModel connectionString, string query, params DbParameter[] parameters);
}
