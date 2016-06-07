using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Providers
{
    public interface IDatabaseProvider
    {
        void SetReadTimeout(System.Data.Common.DbCommand sourceCommand);
        DatabaseTools.Models.DatabaseType ForDatabaseType { get; }
        string TransformConnectionString(string connectionString);

        void UpdateParameter(DbParameter parameter, Models.ColumnModel column);

        DataTable GetTables(System.Configuration.ConnectionStringSettings connectionString);
        DataTable GetViews(System.Configuration.ConnectionStringSettings connectionString);

        DataTable GetTriggers(System.Configuration.ConnectionStringSettings connectionString);
        DataTable GetForeignKeys(System.Configuration.ConnectionStringSettings connectionString);
        DataTable GetDefinitions(System.Configuration.ConnectionStringSettings connectionString);
        DataTable GetDefinitionDependencies(System.Configuration.ConnectionStringSettings connectionString);

        DataTable GetTableColumns(System.Configuration.ConnectionStringSettings connectionString);
        DataTable GetViewColumns(System.Configuration.ConnectionStringSettings connectionString);

        DataTable GetIndexes(System.Configuration.ConnectionStringSettings connectionString);

        Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType);
        DbProviderFactory CreateProvider();

        bool TryHandleColumnValueLoadException(Exception ex, Models.ColumnModel column, out object value);
    }
}
