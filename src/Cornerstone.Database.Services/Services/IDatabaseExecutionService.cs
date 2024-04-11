using System.Data;
using System.Data.Common;
using Cornerstone.Database.Models;

namespace Cornerstone.Database.Services;
public interface IDatabaseExecutionService
{
    DbCommand CreateDbCommand(DbConnection dbConnection);
    DbConnection CreateDbConnection(ConnectionStringModel connectionString);
    DataSet Execute(DbConnection connection, string sqlCommand);
    void ExecuteFile(ConnectionStringModel connectionString, string sqlCommand);
    void ExecuteFile(DbConnection connection, string sqlCommand);
    void ExecuteNonQuery(DbConnection connection, string sqlCommand);
}