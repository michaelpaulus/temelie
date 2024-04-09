namespace Cornerstone.Database.Services;

public class TableConverterException : Exception
{

    public TableConverterException(Models.TableModel table, string message) : base(message)
    {
        this.Table = table;
    }

    public TableConverterException(Models.TableModel table, string message, Exception innerException) : base(message, innerException)
    {
        this.Table = table;
    }

    public Models.TableModel Table { get; set; }

}
