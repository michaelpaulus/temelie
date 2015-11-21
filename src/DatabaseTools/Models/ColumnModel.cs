
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
    namespace Models
    {
        public class ColumnModel : Model
        {
            #region Properties

            private string _tableName;
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

            private int _columnID;
            public int ColumnID
            {
                get
                {
                    return _columnID;
                }
                set
                {
                    if (!(int.Equals(this._columnID, value)))
                    {
                        _columnID = value;
                        this.OnPropertyChanged("ColumnID");
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
                        this.ValidateColumnTypeAndPrecision();
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
                        value = value.ToUpper();

                        value = value.Replace(" UNSIGNED", "");

                        if (value.Contains("(") &&
                            value.EndsWith(")"))
                        {
                            if (value == "TINYINT(1)")
                            {
                                value = "BIT";
                            }
                            else
                            {
                                value = value.Substring(0, value.IndexOf("("));
                            }
                        }

                        switch (value)
                        {
                            case "LONG VARCHAR":
                            case "TEXT":
                            case "130":
                            case "STRING":
                            case "LONGTEXT":
                                value = "VARCHAR";
                                if (this._precision < 4000)
                                {
                                    this._precision = 4000;
                                }
                                break;
                            case "UNSIGNED INT":
                            case "INTEGER":
                            case "INT32":
                            case "MEDIUMINT":
                                value = "INT";
                                break;
                            case "INT16":
                            case "TINYINT":
                                value = "SMALLINT";
                                break;
                            case "NUMERIC":
                            case "DOUBLE":
                            case "SINGLE":
                                value = "DECIMAL";
                                break;
                            case "TIMESTAMP":
                            case "DATE":
                                value = "DATETIME";
                                break;
                            case "NVARCHAR":
                            case "NCHAR":
                                value = "NVARCHAR";
                                break;
                            case "BOOLEAN":
                                value = "BIT";
                                break;
                            case "BYTE[]":
                            case "128":
                            case "BLOB":
                                value = "VARBINARY";
                                break;
                            case "CHAR":
                                if (this.Precision > 0 && this.Precision != 1)
                                {
                                    value = "VARCHAR";
                                }
                                break;
                        }
                        this._columnType = value;
                        this.ValidateColumnTypeAndPrecision();
                        this.OnPropertyChanged("ColumnType");
                    }
                }
            }

            #endregion

            #region Methods

            public System.Data.DbType DbType
            {
                get
                {
                    return Processes.Database.GetDBType(this.ColumnType);
                }
            }

            private void ValidateColumnTypeAndPrecision()
            {
                if (this.ColumnType == "VARCHAR" && this.Precision > 4000)
                {
                    //MAX
                    this._precision = int.MaxValue;
                }

                if (this.ColumnType == "VARCHAR" && this.Precision == 0)
                {
                    this._precision = 4000;
                }

                if (this.ColumnType == "VARBINARY" && this.Precision > 4000)
                {
                    //MAX
                    this._precision = int.MaxValue;
                }

                if (this.ColumnType == "CHAR" && this.Precision != 0 && this.Precision > 1)
                {
                    this._columnType = "VARCHAR";
                }

                if (this.ColumnType.EqualsIgnoreCase("DECIMAL"))
                {
                    //When reading the foxpro database,
                    //   we get 8,0 for some reason
                    if (this.Precision == 8 && this.Scale == 0)
                    {
                        this._precision = 18;
                        this._scale = 6;
                    }

                    //In access we are getting 7,0 for single
                    if (this.Precision == 7 && this.Scale == 0)
                    {
                        this._precision = 18;
                        this._scale = 6;
                    }

                    //In access we are getting 15,0 for double
                    if (this.Precision == 15 && this.Scale == 0)
                    {
                        this._precision = 18;
                        this._scale = 6;
                    }

                    if (this.Precision == 0 && this.Scale == 0)
                    {
                        this._precision = 18;
                        this._scale = 6;
                    }
                }
            }

            public string ToString(string quoteCharacter)
            {
                string strDataType = this.ColumnType;

                switch (strDataType.ToUpper())
                {
                    case "DECIMAL":
                    case "NUMERIC":
                        strDataType = string.Format("{0}({1}, {2})", this.ColumnType, this.Precision, this.Scale);
                        break;
                    case "BINARY":
                    case "VARBINARY":
                    case "VARCHAR":
                    case "CHAR":
                    case "NVARCHAR":
                    case "NCHAR":
                        string strPrecision = this.Precision.ToString();
                        if (this.Precision == -1 || this.Precision == Int32.MaxValue)
                        {
                            strPrecision = "MAX";
                        }
                        strDataType = string.Format("{0}({1})", this.ColumnType, strPrecision);
                        break;
                }

                if (this.IsComputed)
                {
                    strDataType = "AS " + this.ComputedDefinition;
                }

                string strIdentity = string.Empty;

                if (this.IsIdentity)
                {
                    strIdentity = " IDENTITY(1,1)";
                }

                string strNull = "NULL";

                if (!this.IsNullable)
                {
                    strNull = "NOT " + strNull;
                }

                if (this.IsComputed)
                {
                    strNull = string.Empty;
                }

                return string.Format("{4}{0}{4} {1}{2} {3}", this.ColumnName, strDataType, strIdentity, strNull, quoteCharacter).Trim();
            }

            public override string ToString()
            {
                return this.ToString("");
            }

            public void Initialize(DataRow row)
            {
                //Do precision first so that the column type can be converted to 
                //   varchar for text
                this.TableName = this.GetStringValue(row, "table_name");
                this.ColumnName = this.GetStringValue(row, "column_name");
                this.Precision = this.GetInt32Value(row, "precision");
                this.Scale = this.GetInt32Value(row, "scale");
                this.ColumnType = this.GetStringValue(row, "column_type");


                this.IsNullable = this.GetBoolValue(row, "is_nullable");
                this.IsIdentity = this.GetBoolValue(row, "is_identity");
                this.IsComputed = this.GetBoolValue(row, "is_computed");
                this.ComputedDefinition = this.GetStringValue(row, "computed_definition");
                this.ColumnID = this.GetInt32Value(row, "column_id");
                this.IsPrimaryKey = this.GetBoolValue(row, "is_primary_key");
            }

            #endregion

        }
    }


}