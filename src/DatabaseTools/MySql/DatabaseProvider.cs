using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.MySql
{
    public class DatabaseProvider : IDatabaseProvider
    {
        public Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType, Models.DatabaseType targetDatabaseType)
        {
            if (targetDatabaseType == Models.DatabaseType.MicrosoftSQLServer)
            {
                var targetColumnType = new Models.ColumnTypeModel();

                targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Replace("UNSIGNED", "").Trim();

                if (targetColumnType.ColumnType.Contains("(") &&
                targetColumnType.ColumnType.EndsWith(")"))
                {
                    if (targetColumnType.ColumnType == "TINYINT(1)")
                    {
                        targetColumnType.ColumnType = "BIT";
                    }
                    else
                    {
                        targetColumnType.ColumnType = targetColumnType.ColumnType.Substring(0, targetColumnType.ColumnType.IndexOf("("));
                    }
                }

                targetColumnType.Precision = sourceColumnType.Precision;
                targetColumnType.Scale = sourceColumnType.Scale;

                switch (targetColumnType.ColumnType)
                {
                    case "LONG VARCHAR":
                    case "TEXT":
                    case "LONGTEXT":
                        targetColumnType.ColumnType = "NVARCHAR";
                        if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                        {
                            targetColumnType.Precision = int.MaxValue;
                        }
                        break;
                    case "VARCHAR":
                    case "STRING":
                        targetColumnType.ColumnType = "NVARCHAR";
                        break;
                    case "INTEGER":
                    case "INT32":
                    case "MEDIUMINT":
                        targetColumnType.ColumnType = "INT";
                        break;
                    case "INT16":
                    case "TINYINT":
                        targetColumnType.ColumnType = "SMALLINT";
                        break;
                    case "NUMERIC":
                    case "DOUBLE":
                    case "SINGLE":
                    case "DEC":
                        targetColumnType.ColumnType = "DECIMAL";
                        break;
                    case "TIMESTAMP":
                        targetColumnType.ColumnType = "DATETIME";
                        break;
                    case "DATETIME":
                        targetColumnType.ColumnType = "DATETIME2";
                        break;
                    case "CHAR":
                        targetColumnType.ColumnType = "NCHAR";
                        break;
                    case "BOOLEAN":
                        targetColumnType.ColumnType = "BIT";
                        break;
                    case "BYTE[]":
                    case "BLOB":
                    case "LONGBLOB":
                        targetColumnType.ColumnType = "VARBINARY";
                        break;
                }

                switch (targetColumnType.ColumnType)
                {
                    case "NVARCHAR":
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
