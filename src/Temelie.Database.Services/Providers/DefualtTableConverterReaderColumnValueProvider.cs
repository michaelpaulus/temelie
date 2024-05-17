using System.Data;
using Temelie.Database.Models;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;

[ExportProvider(typeof(ITableConverterReaderColumnValueProvider))]
public class DefualtTableConverterReaderColumnValueProvider : ITableConverterReaderColumnValueProvider
{
    public object GetColumnValue(ColumnModel targetColumn, object value, bool trimStrings)
    {
        object returnValue = value;

        var dbType = targetColumn.DbType;

        if (value == DBNull.Value)
        {
            switch (dbType)
            {
                case System.Data.DbType.Boolean:
                    if (!targetColumn.IsNullable)
                    {
                        if (!string.IsNullOrEmpty(targetColumn.ColumnDefault))
                        {
                            if (targetColumn.ColumnDefault.Contains("1"))
                            {
                                returnValue = true;
                            }
                            else
                            {
                                returnValue = false;
                            }
                        }
                        else
                        {
                            returnValue = false;
                        }
                    }
                    break;
            }
        }
        else
        {
            switch (dbType)
            {
                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                    try
                    {
                        DateTime dt = System.Convert.ToDateTime(value);

                        if (dt <= new DateTime(1753, 1, 1))
                        {
                            returnValue = new DateTime(1753, 1, 1);
                        }
                        else if (dt > new DateTime(9999, 12, 31))
                        {
                            returnValue = new DateTime(9999, 12, 31);
                        }
                        else
                        {
                            returnValue = dt;
                        }
                    }
                    catch
                    {
                        returnValue = new DateTime(1753, 1, 1);
                    }
                    break;
                case System.Data.DbType.DateTime2:
                    try
                    {
                        DateTime dt = System.Convert.ToDateTime(value);
                        returnValue = dt;
                    }
                    catch
                    {
                        returnValue = DateTime.MinValue;
                    }
                    break;
                case System.Data.DbType.Time:
                    {
                        if ((value) is TimeSpan)
                        {
                            returnValue = value;
                        }
                        else if ((value) is DateTime)
                        {
                            var dt = System.Convert.ToDateTime(value);
                            returnValue = dt.TimeOfDay;
                        }
                        else
                        {
                            returnValue = value;
                        }
                        break;
                    }
                case DbType.Boolean:
                    if (value.GetType() == typeof(string))
                    {
                        if (value.ToString() == "1" ||
                            value.ToString().Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                            value.ToString().Equals("y", StringComparison.InvariantCultureIgnoreCase))
                        {
                            returnValue = true;
                        }
                        else if (value.ToString() == "0" ||
                            value.ToString().Equals("no", StringComparison.InvariantCultureIgnoreCase) ||
                            value.ToString().Equals("n", StringComparison.InvariantCultureIgnoreCase))
                        {
                            returnValue = false;
                        }
                    }
                    break;
                case DbType.Guid:
                    if (value.GetType() == typeof(string))
                    {
                        var valueAsString = System.Convert.ToString(value);

                        if (string.IsNullOrEmpty(valueAsString) &&
                            targetColumn.IsNullable)
                        {
                            returnValue = DBNull.Value;
                        }
                        else if (string.IsNullOrEmpty(valueAsString))
                        {
                            returnValue = Guid.Empty;
                        }
                        else
                        {
                            returnValue = new Guid(valueAsString);
                        }

                    }
                    break;
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                    {
                        if (trimStrings)
                        {
                            returnValue = System.Convert.ToString(value).TrimEnd();
                        }
                        else
                        {
                            returnValue = System.Convert.ToString(value);
                        }
                        break;
                    }
            }
        }

        return returnValue;
    }
}
