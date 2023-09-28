using System;
using System.Collections.Generic;
using System.Configuration;
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

        Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType);
        DbProviderFactory CreateProvider();

        bool TryHandleColumnValueLoadException(Exception ex, Models.ColumnModel column, out object value);
        DataTable GetSecurityPolicies(DbConnection connection);
    }
}
