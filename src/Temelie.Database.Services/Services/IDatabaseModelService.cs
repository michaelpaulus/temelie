using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface IDatabaseModelService
{
    DatabaseModel CreateModel(ConnectionStringModel connectionString, DatabaseModelOptions options = null);
    DatabaseModel CreateModel(DbConnection connection, DatabaseModelOptions options = null);
    IEnumerable<Models.TableModel> GetTables(DbConnection connection, DatabaseModelOptions options, IEnumerable<ColumnModel> columns);
    IEnumerable<ColumnModel> GetTableColumns(DbConnection connection);
};
