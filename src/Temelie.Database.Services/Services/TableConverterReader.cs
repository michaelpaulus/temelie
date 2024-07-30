using System.Data;
using Temelie.Database.Providers;

namespace Temelie.Database.Services;

public class TableConverterReader : System.Data.IDataReader
{

    private readonly IDatabaseProvider _databaseProvider;
    private readonly IDataReader _parent;
    private readonly IList<Models.ColumnModel> _sourceColumns;
    private readonly IList<Models.ColumnModel> _targetColumns;
    private readonly IEnumerable<ITableConverterReaderColumnValueProvider> _tableConverterReaderColumnValueProviders;

    public TableConverterReader(IDatabaseProvider databaseProvider,
        IDataReader parent,
        IEnumerable<ITableConverterReaderColumnValueProvider> tableConverterReaderColumnValueProviders,
        IEnumerable<Models.ColumnModel> sourceColumns,
        IEnumerable<Models.ColumnModel> targetColumns,
        bool trimStrings)
    {
        _databaseProvider = databaseProvider;
        _parent = parent;
        _sourceColumns = sourceColumns.ToList();
        _targetColumns = targetColumns.ToList();
        TrimStrings = trimStrings;
        _tableConverterReaderColumnValueProviders = tableConverterReaderColumnValueProviders;
    }

    public IDataReader Parent
    {
        get
        {
            return this._parent;
        }
    }

    public IEnumerable<Models.ColumnModel> SourceColumns
    {
        get
        {
            return this._sourceColumns;
        }
    }

    public IEnumerable<Models.ColumnModel> TargetColumns
    {
        get
        {
            return this._targetColumns;
        }
    }

    public bool TrimStrings { get; set; }

    public object this[string name]
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public object this[int i]
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public int Depth
    {
        get
        {
            return this.Parent.Depth;
        }
    }

    public int FieldCount
    {
        get
        {
            return this.Parent.FieldCount;
        }
    }

    public bool IsClosed
    {
        get
        {
            return this.Parent.IsClosed;
        }
    }

    public int RecordsAffected
    {
        get
        {
            return this.Parent.RecordsAffected;
        }
    }

    public void Close()
    {
        this.Parent.Close();
    }

    public void Dispose()
    {
        this.Parent.Dispose();
    }

    public string GetName(int i)
    {
        return this.Parent.GetName(i);
    }

    public int GetOrdinal(string name)
    {
        return this.Parent.GetOrdinal(name);
    }

    public DataTable GetSchemaTable()
    {
        return this.Parent.GetSchemaTable();
    }

    public object GetValue(int i)
    {
        object value;
        try
        {
            value = this.Parent.GetValue(i);
        }
        catch (Exception ex)
        {
            var sourceColumn = _sourceColumns[i];
            object newValue;
            if (_databaseProvider != null &&
                _databaseProvider.TryHandleColumnValueLoadException(ex, sourceColumn, out newValue))
            {
                value = newValue;
            }
            else
            {
                throw;
            }

        }

        var targetColumn = _targetColumns[i];

        value = this.GetColumnValue(targetColumn, value);

        return value;
    }

    public bool NextResult()
    {
        return this.Parent.NextResult();
    }

    public bool Read()
    {
        return this.Parent.Read();
    }

    public object GetColumnValue(Models.ColumnModel targetColumn, object value)
    {
        object returnValue = value;

        foreach (var provider in _tableConverterReaderColumnValueProviders)
        {
            returnValue = provider.GetColumnValue(targetColumn, returnValue, TrimStrings);
        }

        return returnValue;
    }

    public bool IsDBNull(int i)
    {
        return this.Parent.IsDBNull(i);
    }

    #region Not Implemented

    public bool GetBoolean(int i)
    {
        throw new NotImplementedException();
    }

    public byte GetByte(int i)
    {
        throw new NotImplementedException();
    }

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public char GetChar(int i)
    {
        throw new NotImplementedException();
    }

    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
        throw new NotImplementedException();
    }

    public IDataReader GetData(int i)
    {
        throw new NotImplementedException();
    }

    public string GetDataTypeName(int i)
    {
        throw new NotImplementedException();
    }

    public DateTime GetDateTime(int i)
    {
        throw new NotImplementedException();
    }

    public decimal GetDecimal(int i)
    {
        throw new NotImplementedException();
    }

    public double GetDouble(int i)
    {
        throw new NotImplementedException();
    }

    public Type GetFieldType(int i)
    {
        throw new NotImplementedException();
    }

    public float GetFloat(int i)
    {
        throw new NotImplementedException();
    }

    public Guid GetGuid(int i)
    {
        throw new NotImplementedException();
    }

    public short GetInt16(int i)
    {
        throw new NotImplementedException();
    }

    public int GetInt32(int i)
    {
        throw new NotImplementedException();
    }

    public long GetInt64(int i)
    {
        throw new NotImplementedException();
    }

    public string GetString(int i)
    {
        throw new NotImplementedException();
    }

    public int GetValues(object[] values)
    {
        for(var i = 0; i < values.Length; i++)
        {
            values[i] = this.GetValue(i);
        }
        return values.Length;
    }

    #endregion

}
