using System.Data;
using System.Text;
using Temelie.Database.Models;

namespace Temelie.Database.Providers.MySql;

public partial class DatabaseProvider
{

    public override IDatabaseObjectScript GetScript(IndexModel model)
    {
        if (model.IsPrimaryKey)
        {
            return null;
        }

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd};");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            if (!model.IsPrimaryKey)
            {
                sb.AppendLine($"CREATE{(model.IsUnique ? " UNIQUE" : "")} INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");

                if (model.Columns.Any())
                {
                    sb.AppendLine("(");

                    bool blnHasColumns = false;

                    foreach (var column in model.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append($"    {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.SubPart.HasValue ? $"({column.SubPart})" : "")}{(column.IsDescending ? " DESC" : "")}");
                        blnHasColumns = true;
                    }

                    sb.AppendLine();
                    sb.AppendLine(");");
                }

            }
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(TableModel model)
    {
        if (string.IsNullOrEmpty(model.TableName))
        {
            return null;
        }

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TABLE IF EXISTS {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine(string.Format("-- {0}", model.TableName));

            sb.AppendLine($"CREATE TABLE IF NOT EXISTS {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            sb.AppendLine("(");

            int intColumnCount = 0;

            foreach (ColumnModel column in (
                from i in model.Columns
                orderby i.ColumnId
                select i))
            {
                if (intColumnCount != 0)
                {
                    sb.AppendLine(",");
                }

                sb.Append("    " + GetScript(column));

                intColumnCount += 1;
            }

            var pk = model.Columns.Where(i => i.IsPrimaryKey).ToList();
            if (pk.Any())
            {
                sb.AppendLine(",");
                sb.Append($"    PRIMARY KEY ({string.Join(", ", pk.Select(i => $"`{i.ColumnName}`"))})");
            }

            sb.AppendLine();

            sb.AppendLine(");");

            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override string GetRenameScript(TableModel model, string newTableName)
    {
        return $"RENAME TABLE {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} TO {QuoteCharacterStart}{newTableName}{QuoteCharacterEnd};";
    }

    public override IDatabaseObjectScript GetScript(CheckConstraintModel model)
    {
        return null;
    }

    public override IDatabaseObjectScript GetScript(DefinitionModel model)
    {
        return null;
    }
    public override IDatabaseObjectScript GetScript(ForeignKeyModel model)
    {
        return null;
    }

    public override IDatabaseObjectScript GetScript(SecurityPolicyModel model)
    {
        return null;
    }

    public override IDatabaseObjectScript GetScript(TriggerModel model)
    {
        return null;
    }

    public override IDatabaseObjectScript GetColumnScript(ColumnModel column)
    {
        return null;
    }

    private string GetScript(ColumnModel columnModel)
    {
        string strDataType = GetFullColumnType(columnModel);

        if (columnModel.IsComputed &&
            columnModel.GeneratedAlwaysType == 0)
        {
            strDataType = "AS " + columnModel.ComputedDefinition;
        }

        string strIdentity = string.Empty;

        if (columnModel.IsIdentity)
        {
            strIdentity = " AUTO_INCREMENT";
        }

        string strNull = " NULL";

        if (!columnModel.IsNullable)
        {
            strNull = " NOT NULL";
        }

        string defaultValue = "";

        if (!string.IsNullOrEmpty(columnModel.ColumnDefault))
        {
            string columnDefault = columnModel.ColumnDefault.Trim();

            if (!columnDefault.StartsWith("("))
            {
                if (!columnDefault.StartsWith("'"))
                {
                    switch (columnModel.ColumnType.ToUpper())
                    {
                        case "VARCHAR":
                        case "CHAR":
                        case "NVARCHAR":
                        case "NCHAR":
                            columnDefault = "'" + columnDefault + "'";
                            break;
                    }
                }
                columnDefault = "(" + columnDefault + ")";
            }
            else if (columnDefault.StartsWith("((") &&
                columnDefault.EndsWith("))"))
            {
                columnDefault = columnDefault.Substring(1);
                columnDefault = columnDefault.Substring(0, columnDefault.Length - 1);
            }
            columnDefault = columnDefault.Replace("getdate()", "GETDATE()").Replace("newid()", "NEWID()");
            defaultValue = $" DEFAULT {columnDefault}";
        }

        return $"{QuoteCharacterStart}{columnModel.ColumnName}{QuoteCharacterEnd} {strDataType}{strNull}{strIdentity}{defaultValue}".Trim();

    }

    private string GetFullColumnType(ColumnModel columnModel)
    {
        string strDataType = columnModel.ColumnType;

        switch (strDataType.ToUpper())
        {
            case "DECIMAL":
            case "NUMERIC":
                strDataType = string.Format("{0}({1}, {2})", columnModel.ColumnType, columnModel.Precision, columnModel.Scale);
                break;
            case "TINYINT":
                strDataType = string.Format("{0}({1})", columnModel.ColumnType, columnModel.Precision);
                break;
            case "BINARY":
            case "VARBINARY":
            case "VARCHAR":
            case "CHAR":
            case "NVARCHAR":
            case "NCHAR":
                string strPrecision = columnModel.Precision.ToString();
                if (columnModel.Precision == -1 || columnModel.Precision == Int32.MaxValue)
                {
                    return "TEXT";
                }
                strDataType = string.Format("{0}({1})", columnModel.ColumnType, strPrecision);
                break;
            case "TIME":
                strDataType = string.Format("{0}({1})", columnModel.ColumnType, columnModel.Scale);
                break;
            case "DATETIME2":
                if (columnModel.Scale != 7)
                {
                    strDataType = "DATETIME";
                }
                break;
        }

        return strDataType;
    }

}
