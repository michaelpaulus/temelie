using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface IDatabaseModelService
{
    DatabaseModel CreateModel(ConnectionStringModel connectionString, DatabaseModelOptions options = null);
    DatabaseModel CreateModel(DbConnection connection, DatabaseModelOptions options = null);
}