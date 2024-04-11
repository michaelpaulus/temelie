using System.Data.Common;
using Cornerstone.Database.Models;

namespace Cornerstone.Database.Services;
public interface IDatabaseModelService
{
    DatabaseModel CreateModel(ConnectionStringModel connectionString, DatabaseModelOptions options = null);
    DatabaseModel CreateModel(DbConnection connection, DatabaseModelOptions options = null);
}