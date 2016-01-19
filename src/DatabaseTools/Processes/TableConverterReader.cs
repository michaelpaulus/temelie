using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Processes
{
    public class TableConverterReader : System.Data.IDataReader
    {
        public TableConverterReader(IDataReader parent, IList<Models.ColumnModel> sourceColumns, IList<Models.ColumnModel> targetColumns, bool trimStrings)
        {
            this._parent = parent;
            this._sourceColumns = sourceColumns;
            this._targetColumns = targetColumns;
            this.TrimStrings = trimStrings;
        }

        private IDataReader _parent;
        public IDataReader Parent
        {
            get
            {
                return this._parent;
            }
        }

        private IList<Models.ColumnModel> _sourceColumns;
        public IList<Models.ColumnModel> SourceColumns
        {
            get
            {
                return this._sourceColumns;
            }
        }

        private IList<Models.ColumnModel> _targetColumns;
        public IList<Models.ColumnModel> TargetColumns
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
            object value = null;

            try
            {
                value = this.Parent.GetValue(i);
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (MySql.Data.Types.MySqlConversionException ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                var sourceColumn = this.SourceColumns[i];

                if (sourceColumn.DbType == System.Data.DbType.DateTime2 ||
                    sourceColumn.DbType == System.Data.DbType.Date ||
                    sourceColumn.DbType == System.Data.DbType.DateTime ||
                    sourceColumn.DbType == System.Data.DbType.DateTimeOffset)
                {
                    value = DateTime.MinValue;
                }
                else
                {
                    throw;
                }
            }

            var targetColumn = this.TargetColumns[i];

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

            if (value != DBNull.Value)
            {
                var dbType = targetColumn.DbType;

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
                    case System.Data.DbType.Boolean:
                        if (!targetColumn.IsNullable &&
                            (
                                value == null ||
                                value == DBNull.Value
                            ))
                        {
                            value = false;
                        }
                        break;  
                    case System.Data.DbType.Time:
                        {
                            if ((value) is TimeSpan)
                            {
                                returnValue = (new DateTime(1753, 1, 1)).Add((TimeSpan)value);
                            }
                            else if ((value) is DateTime)
                            {
                                DateTime dt = System.Convert.ToDateTime(value);

                                if (dt <= new DateTime(1753, 1, 1))
                                {
                                    returnValue = DBNull.Value;
                                }
                                else if (dt > new DateTime(9999, 12, 31))
                                {
                                    returnValue = DBNull.Value;
                                }
                                else
                                {
                                    returnValue = dt;
                                }
                            }
                            else
                            {
                                returnValue = value;
                            }
                            break;
                        }
                    case System.Data.DbType.AnsiString:
                    case System.Data.DbType.AnsiStringFixedLength:
                    case System.Data.DbType.String:
                    case System.Data.DbType.StringFixedLength:
                        {
                            if (this.TrimStrings)
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
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
