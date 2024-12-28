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
                        DateTime dt;
                        if (value is DateTime dateTime)
                        {
                            dt = dateTime;
                        }
                        else
                        {
                            dt = System.Convert.ToDateTime(value);
                        }

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
                        DateTime dt;
                        if (value is DateTime dateTime)
                        {
                            dt = dateTime;
                        }
                        else
                        {
                            dt = System.Convert.ToDateTime(value);
                        }
                        returnValue = dt;
                    }
                    catch
                    {
                        returnValue = DateTime.MinValue;
                    }
                    break;
                case System.Data.DbType.Time:
                    {
                        if (value is TimeSpan timeSpan)
                        {
                            returnValue = timeSpan;
                        }
                        else if (value is DateTime dateTime)
                        {
                            returnValue = dateTime.TimeOfDay;
                        }
                        else
                        {
                            returnValue = value;
                        }
                        break;
                    }
                case DbType.Boolean:
                    {
                        if (value is string stringValue)
                        {
                            if (stringValue == "1" ||
                                stringValue.Equals("yes", StringComparison.InvariantCultureIgnoreCase) ||
                                stringValue.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                            {
                                returnValue = true;
                            }
                            else if (stringValue == "0" ||
                                stringValue.Equals("no", StringComparison.InvariantCultureIgnoreCase) ||
                                stringValue.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                            {
                                returnValue = false;
                            }
                        }
                        break;
                    }
                case DbType.Guid:
                    {
                        if (value is string stringValue)
                        {
                            if (string.IsNullOrEmpty(stringValue) &&
                                targetColumn.IsNullable)
                            {
                                returnValue = DBNull.Value;
                            }
                            else if (string.IsNullOrEmpty(stringValue))
                            {
                                returnValue = Guid.Empty;
                            }
                            else
                            {
                                returnValue = new Guid(stringValue);
                            }
                        }
                        break;
                    }
                case System.Data.DbType.AnsiString:
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.String:
                case System.Data.DbType.StringFixedLength:
                    {
                        if (value is string stringValue)
                        {
                            returnValue = trimStrings ? stringValue.TrimEnd() : stringValue;
                        }
                        else
                        {
                            returnValue = trimStrings ? value.ToString().TrimEnd() : value.ToString();
                        }
                        break;
                    }
            }
        }

        return returnValue;
    }
}
