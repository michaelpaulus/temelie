using System.Data;
using System.Text.Json.Serialization;

namespace Temelie.Database.Models;

public class ColumnModel : Model
{
    #region Properties

    private string _tableName;
    [JsonIgnore]
    public string TableName
    {
        get
        {
            return _tableName;
        }
        set
        {
            _tableName = value;
            this.OnPropertyChanged("TableName");
        }
    }

    public string SchemaName { get; set; }

    private string _columnName;
    public string ColumnName
    {
        get
        {
            return _columnName;
        }
        set
        {
            _columnName = value;
            this.OnPropertyChanged("ColumnName");
        }
    }

    [JsonIgnore]
    public string PropertyName
    {
        get
        {
            if (ExtendedProperties.TryGetValue("name", out var name))
            {
                return name;
            }
            if (ExtendedProperties.TryGetValue("dynamicName", out var dynamicName))
            {
                return dynamicName;
            }
            return NormalizePropertyName(ColumnName);
        }
    }

    private int _columnId;
    public int ColumnId
    {
        get
        {
            return _columnId;
        }
        set
        {
            if (!(int.Equals(this._columnId, value)))
            {
                _columnId = value;
                this.OnPropertyChanged("ColumnId");
            }
        }
    }

    public bool IsPrimaryKey { get; set; }

    private int _precision;
    public int Precision
    {
        get
        {
            return this._precision;
        }
        set
        {
            if (!(int.Equals(this._precision, value)))
            {
                this._precision = value;
                this.OnPropertyChanged("Precision");
            }
        }
    }

    private int _scale;
    public int Scale
    {
        get
        {
            return _scale;
        }
        set
        {
            if (!(int.Equals(this._scale, value)))
            {
                this._scale = value;
                this.OnPropertyChanged("Scale");
            }
        }
    }

    private bool _isNullable;
    public bool IsNullable
    {
        get
        {
            return _isNullable;
        }
        set
        {
            _isNullable = value;
            this.OnPropertyChanged("IsNullable");
        }
    }

    private bool _isIdentity;
    public bool IsIdentity
    {
        get
        {
            return _isIdentity;
        }
        set
        {
            _isIdentity = value;
            this.OnPropertyChanged("IsIdentity");
        }
    }

    private bool _isComputed;
    public bool IsComputed
    {
        get
        {
            return _isComputed;
        }
        set
        {
            _isComputed = value;
            this.OnPropertyChanged("IsComputed");
        }
    }

    private bool _isHidden;
    public bool IsHidden
    {
        get
        {
            return _isHidden;
        }
        set
        {
            _isHidden = value;
            this.OnPropertyChanged("IsHidden");
        }
    }

    public int GeneratedAlwaysType { get; set; }

    private string _computedDefinition;
    public string ComputedDefinition
    {
        get
        {
            return _computedDefinition;
        }
        set
        {
            if (value != null)
            {
                if (value.Contains("CONVERT") && value.Contains("(0)))"))
                {
                    value = value.Replace("(0)))", "0))");
                }
            }
            _computedDefinition = value;
            this.OnPropertyChanged("ComputedDefinition");
        }
    }

    private string _columnType;
    public string ColumnType
    {
        get
        {
            return _columnType;
        }
        set
        {
            if (!(string.Equals(this._columnType, value)))
            {
                this._columnType = value;
                this.OnPropertyChanged("ColumnType");
            }
        }
    }

    public string ColumnDefault { get; set; }

    [JsonIgnore]
    public DbType DbType
    {
        get
        {
            return GetDBType(this.ColumnType);
        }
    }

    public Dictionary<string, string> ExtendedProperties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Methods

    internal static string NormalizePropertyName(string columnName)
    {
        columnName = columnName.Replace(" ", "").Replace(".", "").Replace("-", "_");

        foreach (var item in Enumerable.Range(0, 9))
        {
            if (columnName.StartsWith(item.ToString()))
            {
                columnName = "n" + columnName;
            }
        }

        if (columnName == "ID")
        {
            columnName = "Id";
        }

        if (columnName.Contains("ID"))
        {
            var index = columnName.IndexOf("ID");
            if (index > 0)
            {
                var previous = columnName.Substring(index - 1, 1);
                if (previous.Equals(previous.ToLower()))
                {
                    var first = columnName.Substring(0, index);
                    var last = "";
                    if (index > columnName.Length)
                    {
                        last = columnName.Substring(index + 3);
                    }
                    columnName = $"{first}Id{last}";

                }
            }
        }

        return columnName;
    }

    public static Type GetSystemType(System.Data.DbType dbType)
    {
        switch (dbType)
        {
            case DbType.AnsiString:
            case DbType.AnsiStringFixedLength:
            case DbType.String:
            case DbType.StringFixedLength:
                return typeof(string);
            case DbType.Binary:
            case DbType.Byte:
                return typeof(byte);
            case DbType.Boolean:
                return typeof(bool);
            case DbType.Currency:
                return typeof(decimal);
            case DbType.Date:
            case DbType.DateTime:
            case DbType.DateTime2:
            case DbType.DateTimeOffset:
                return typeof(DateTime);
            case DbType.Time:
                return typeof(TimeSpan);
            case DbType.Decimal:
                return typeof(decimal);
            case DbType.Double:
                return typeof(double);
            case DbType.Guid:
                return typeof(Guid);
            case DbType.Int16:
            case DbType.UInt16:
                return typeof(short);
            case DbType.Int32:
            case DbType.UInt32:
                return typeof(int);
            case DbType.Int64:
            case DbType.UInt64:
                return typeof(long);
            case DbType.SByte:
                return typeof(byte);
            case DbType.Single:
                return typeof(float);
            case DbType.VarNumeric:
                return typeof(decimal);
            case DbType.Xml:
                return typeof(string);
        }
        return typeof(string);
    }

    public static string GetSystemTypeString(Type type)
    {
        var propertyType = type.FullName;
        if (propertyType.StartsWith("System.String"))
        {
            propertyType = "string";
        }
        else if (propertyType == "System.Int32")
        {
            propertyType = "int";
        }
        else if (propertyType == "System.In64")
        {
            propertyType = "long";
        }
        else if (propertyType == "System.Int16")
        {
            propertyType = "short";
        }
        return propertyType;
    }

    public static System.Data.DbType GetDBType(string typeName)
    {
        switch (typeName.ToUpper())
        {
            case "BIT":
                return System.Data.DbType.Boolean;
            case "CHAR":
                return System.Data.DbType.StringFixedLength;
            case "DATE":
                return DbType.Date;
            case "DATETIME":
                return System.Data.DbType.DateTime;
            case "DATETIME2":
                return System.Data.DbType.DateTime2;
            case "DECIMAL":
            case "FLOAT":
                return System.Data.DbType.Decimal;
            case "IMAGE":
            case "BINARY":
            case "VARBINARY":
                return System.Data.DbType.Binary;
            case "INT":
                return System.Data.DbType.Int32;
            case "SMALLINT":
                return System.Data.DbType.Int16;
            case "BIGINT":
                return DbType.Int64;
            case "UNIQUEIDENTIFIER":
                return System.Data.DbType.Guid;
            case "VARCHAR":
                return System.Data.DbType.String;
            case "TIME":
                return DbType.Time;
            case "HIERARCHYID":
                return DbType.Object;
        }
        return System.Data.DbType.String;
    }

    #endregion

}
