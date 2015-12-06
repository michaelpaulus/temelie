using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools
{
    public interface IDatabaseProvider
    {
        Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType);
    }
}
