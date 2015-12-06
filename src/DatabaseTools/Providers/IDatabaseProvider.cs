using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Providers
{
    public interface IDatabaseProvider
    {

        DataTable GetIndexes(System.Configuration.ConnectionStringSettings connectionString);

        Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType);
    }
}
