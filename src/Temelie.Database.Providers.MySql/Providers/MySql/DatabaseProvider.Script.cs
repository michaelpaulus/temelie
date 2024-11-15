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

                    var indexType = "";

                    if (model.IndexType != "HASH")
                    {
                        indexType = $" USING {model.IndexType}";
                    }

                    sb.AppendLine();
                    sb.AppendLine($"){indexType};");
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

            var options = new StringBuilder();

            if (!string.IsNullOrEmpty(model.Engine))
            {
                options.Append($" ENGINE={model.Engine}");
            }

            if (!string.IsNullOrEmpty(model.CharacterSetName))
            {
                options.Append($" DEFAULT CHARSET={model.CharacterSetName}");
            }

            if (!string.IsNullOrEmpty(model.CollationName) && model.CharacterSetName != "latin1")
            {
                options.Append($" COLLATE={model.CollationName}");
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

            sb.AppendLine($"){options};");

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

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("-- {0}", model.DefinitionName));
            sb.AppendLine($"DROP {model.Type} IF EXISTS {QuoteCharacterStart}{model.DefinitionName}{QuoteCharacterEnd};");

            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine(model.Definition.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine));
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
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

        string strNull = "";

        if (!columnModel.IsNullable)
        {
            strNull = " NOT NULL";
        }

        string characterSet = "";

        if (!string.IsNullOrEmpty(columnModel.CharacterSetName) && !columnModel.IsPrimaryKey && columnModel.CharacterSetName != "latin1")
        {
            characterSet = $" CHARACTER SET {columnModel.CharacterSetName}";
        }

        string collate = "";

        if (!string.IsNullOrEmpty(columnModel.CollationName) && !columnModel.IsPrimaryKey && !string.IsNullOrEmpty(characterSet))
        {
            collate = $" COLLATE {columnModel.CollationName}";
        }

        string defaultValue = "";

        if (!string.IsNullOrEmpty(columnModel.ColumnDefault))
        {
            string columnDefault = columnModel.ColumnDefault.Trim();
            defaultValue = $" DEFAULT '{columnDefault}'";
        }
        else if (columnModel.IsNullable)
        {
            if (!strDataType.Contains("TEXT"))
            {
                defaultValue = " DEFAULT NULL";
            }
        }

        return $"{QuoteCharacterStart}{columnModel.ColumnName}{QuoteCharacterEnd} {strDataType.ToLower()}{characterSet}{collate}{strNull}{strIdentity}{defaultValue}".Trim();

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
