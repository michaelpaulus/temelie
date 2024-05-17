namespace Temelie.Database.Providers;
public interface ITableConverterReaderColumnValueProvider
{
    object GetColumnValue(Models.ColumnModel targetColumn, object value, bool trimStrings);
}
