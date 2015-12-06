using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseTools.Models;

namespace DatabaseTools.Providers.Odbc
{
    public class DatabaseProvider : IDatabaseProvider
    {
        public ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType, DatabaseType targetDatabaseType)
        {
            return null;
        }

        public DataTable GetIndexes(ConnectionStringSettings connectionString)
        {
            return null;
        }
    }
}
