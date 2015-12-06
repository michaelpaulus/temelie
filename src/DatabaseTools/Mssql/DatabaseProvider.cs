using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseTools.Models;

namespace DatabaseTools.Mssql
{
    public class DatabaseProvider : IDatabaseProvider
    {
        public ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType, DatabaseType targetDatabaseType)
        {
            if (targetDatabaseType == Models.DatabaseType.MicrosoftSQLServer)
            {
                var targetColumnType = new Models.ColumnTypeModel();

                targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Trim();

                targetColumnType.Precision = sourceColumnType.Precision;
                targetColumnType.Scale = sourceColumnType.Scale;

                switch (targetColumnType.ColumnType)
                {
                    case "TEXT":
                        targetColumnType.ColumnType = "NVARCHAR";
                        if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                }

                switch (targetColumnType.ColumnType)
                {
                    case "NVARCHAR":
                    case "VARCHAR":
                    case "VARBINARY":
                        if (targetColumnType.Precision.GetValueOrDefault() > 4000)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                }

                return targetColumnType;

            }

            return null;
        }
    }
}
