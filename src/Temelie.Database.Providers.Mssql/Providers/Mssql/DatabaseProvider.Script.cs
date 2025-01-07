using System.Data;
using System.Text;
using Temelie.Database.Extensions;
using Temelie.Database.Models;

namespace Temelie.Database.Providers.Mssql;

public partial class DatabaseProvider
{

    public override IDatabaseObjectScript GetScript(CheckConstraintModel model)
    {
        string generateDropScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();

            sb.AppendLine();

            sb.AppendLine($@"IF EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = '{model.CheckConstraintName}' AND
        schemas.name = '{schema}'
)
BEGIN
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.check_constraints INNER JOIN
            sys.tables ON
                check_constraints.parent_object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            check_constraints.name = '{model.CheckConstraintName}' AND
            schemas.name = '{schema}' AND
            check_constraints.definition = '{model.CheckConstraintDefinition.Replace("'", "''")}'
    )");
            sb.AppendLine($@"BEGIN
ALTER TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.CheckConstraintName}{QuoteCharacterEnd}
END
END
GO");

            return sb.ToString();

        }

        string generateCreateScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();

            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = '{model.CheckConstraintName}' AND
        schemas.name = '{schema}'
)");
            sb.AppendLine($"ALTER TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} WITH CHECK ADD CONSTRAINT {QuoteCharacterStart}{model.CheckConstraintName}{QuoteCharacterEnd} CHECK ({model.CheckConstraintDefinition})");
            sb.AppendLine("GO");

            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(DefinitionModel model)
    {

        string generateDropScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("-- {0}", model.DefinitionName));
            sb.AppendLine($"DROP {model.Type} IF EXISTS {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.DefinitionName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            sb.AppendLine();

            string strPattern = $"(CREATE\\s*{model.Type}\\s*[\\[]?)([\\[]?{schema}[\\.]?[\\]]?[\\.]?[\\[]?)?({model.DefinitionName})([\\]]?)";

            string strDefinitionReplacement = $"CREATE {model.Type} {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.DefinitionName}{QuoteCharacterEnd}";

            model.Definition = System.Text.RegularExpressions.Regex.Replace(model.Definition, strPattern, strDefinitionReplacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

            sb.AppendLine(model.Definition.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine));
            sb.AppendLine("GO");

            if (model.View != null)
            {
                AddExtendedProperties(model.View, sb);
            }

            return sb.ToString();

        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);

    }

    public override IDatabaseObjectScript GetScript(ForeignKeyModel model)
    {

        string generateDropScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            sb.Append($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = '{model.ForeignKeyName}' AND
            schemas.name = '{schema}'
    )
BEGIN
    IF NOT EXISTS
        (
            SELECT
                1
            FROM
                sys.foreign_keys INNER JOIN
                sys.schemas ON
                    foreign_keys.schema_id = schemas.schema_id
            WHERE
                foreign_keys.name = '{model.ForeignKeyName}' AND
                schemas.name = '{schema}' AND
                foreign_keys.delete_referential_action_desc = '{model.DeleteAction}' AND
                foreign_keys.update_referential_action_desc = '{model.UpdateAction}'");
            var i = 0;

            foreach (var detail in model.Detail)
            {
                i++;
                sb.Append($@" AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.parent_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.parent_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = '{schema}' AND
                        tables.name = '{model.TableName}' AND
                        columns.name = '{detail.Column}' AND
                        foreign_key_columns.constraint_column_id = {i}
                ) AND
                EXISTS
                (
                    SELECT
                        1
                    FROM
                        sys.foreign_key_columns INNER JOIN
                        sys.tables ON
                            foreign_key_columns.referenced_object_id = tables.object_id INNER JOIN
                        sys.columns ON
                            tables.object_id = columns.object_id AND
                            columns.column_id = foreign_key_columns.referenced_column_id INNER JOIN
                        sys.schemas ON
                            tables.schema_id = schemas.schema_id
                    WHERE
                        foreign_key_columns.constraint_object_id = foreign_keys.object_id AND
                        schemas.name = '{model.ReferencedSchemaName}' AND
                        tables.name = '{model.ReferencedTableName}' AND
                        columns.name = '{detail.ReferencedColumn}' AND
                        foreign_key_columns.constraint_column_id = {i}
                )");
            }

            sb.AppendLine();

            sb.AppendLine($@"        )
    BEGIN
        ALTER TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.ForeignKeyName}{QuoteCharacterEnd}
    END
END
GO");

            return sb.ToString();

        }

        string generateCreateScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();

            string strColumnNames = string.Empty;
            string strReferencedColumnNames = string.Empty;

            foreach (var item in model.Detail)
            {
                string strColumnName = item.Column;
                string strReferencedColumnName = item.ReferencedColumn;

                if (strColumnNames.Length > 0)
                {
                    strColumnNames += ", ";
                }

                strColumnNames += $"{QuoteCharacterStart}{strColumnName}{QuoteCharacterEnd}";

                if (strReferencedColumnNames.Length > 0)
                {
                    strReferencedColumnNames += ", ";
                }

                strReferencedColumnNames += $"{QuoteCharacterStart}{strReferencedColumnName}{QuoteCharacterEnd}"; ;
            }

            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = '{model.ForeignKeyName}' AND
            schemas.name = '{schema}'
    )");
            sb.AppendLine($"    ALTER TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} WITH CHECK ADD CONSTRAINT {QuoteCharacterStart}{model.ForeignKeyName}{QuoteCharacterEnd} FOREIGN KEY ({strColumnNames})");
            sb.AppendLine($"    REFERENCES {QuoteCharacterStart}{model.ReferencedSchemaName}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.ReferencedTableName}{QuoteCharacterEnd} ({strReferencedColumnNames})");

            if (model.UpdateAction != "NO_ACTION")
            {
                sb.AppendLine("    " + string.Format("ON UPDATE {0}", model.UpdateAction.Replace("_", " ")));
            }

            if (model.DeleteAction != "NO_ACTION")
            {
                sb.AppendLine("    " + string.Format("ON DELETE {0}", model.DeleteAction.Replace("_", " ")));
            }

            sb.AppendLine("GO");

            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);

    }

    public override IDatabaseObjectScript GetScript(IndexModel model, bool isView)
    {
        var objectType = isView ? "VIEW" : "TABLE";
        var sysTableName = isView ? "views" : "tables";

        string generateDropScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            if (model.IsPrimaryKey)
            {
                sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.{sysTableName} ON
                indexes.object_id = {sysTableName}.object_id INNER JOIN
            sys.schemas ON
                {sysTableName}.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine($"    ALTER {objectType} {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} DROP CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd}");
            }
            else
            {
                sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.{sysTableName} ON
                indexes.object_id = {sysTableName}.object_id INNER JOIN
            sys.schemas ON
                {sysTableName}.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine($"    DROP INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            }
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            if (model.IndexType.Contains("HASH"))
            {
                sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.{sysTableName} ON
                indexes.object_id = ta{sysTableName}bles.object_id INNER JOIN
            sys.schemas ON
                {sysTableName}.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine("    " + $"ALTER {objectType} {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} ADD INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {model.IndexType}");
                sb.AppendLine("    " + "(");

                int intColumnCount = 0;

                foreach (var column in model.Columns)
                {
                    if (intColumnCount > 0)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append($"        {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                    intColumnCount += 1;
                }
                sb.AppendLine();
                sb.AppendLine("    " + ")");

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            else if (model.IsPrimaryKey)
            {
                sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.{sysTableName} ON
                indexes.object_id = {sysTableName}.object_id INNER JOIN
            sys.schemas ON
                {sysTableName}.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine("    " + $"ALTER {objectType} {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd} ADD CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} PRIMARY KEY {model.IndexType}");
                sb.AppendLine("    " + "(");

                int intColumnCount = 0;

                foreach (var column in model.Columns)
                {
                    if (intColumnCount > 0)
                    {
                        sb.AppendLine(",");
                    }

                    sb.Append($"    {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                    intColumnCount += 1;
                }
                sb.AppendLine();
                sb.AppendLine("    " + ")");

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            else
            {
                string indexType = model.IndexType;

                if (model.IsUnique)
                {
                    indexType = "UNIQUE " + model.IndexType;
                }

                sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.{sysTableName} ON
                indexes.object_id = {sysTableName}.object_id INNER JOIN
            sys.schemas ON
                {sysTableName}.schema_id = schemas.schema_id
        WHERE
            indexes.name = '{model.IndexName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine("    " + $"CREATE {indexType} INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} ON {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");

                if (model.Columns.Any())
                {
                    sb.AppendLine("    " + "(");

                    bool blnHasColumns = false;

                    foreach (var column in model.Columns)
                    {
                        if (blnHasColumns)
                        {
                            sb.AppendLine(",");
                        }
                        sb.Append($"    {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                        blnHasColumns = true;
                    }

                    sb.AppendLine();
                    sb.AppendLine("    " + ")");

                    if (model.IncludeColumns.Any())
                    {
                        sb.Append("    INCLUDE (");

                        bool blnHasIncludeColumns = false;

                        foreach (var includeColumn in model.IncludeColumns)
                        {
                            if (blnHasIncludeColumns)
                            {
                                sb.Append(", ");
                            }
                            sb.Append($"{QuoteCharacterStart}{includeColumn.ColumnName}{QuoteCharacterEnd}");
                            blnHasIncludeColumns = true;
                        }

                        sb.AppendLine(")");
                    }
                }

                if (!string.IsNullOrEmpty(model.FilterDefinition))
                {
                    sb.AppendLine($"    WHERE {model.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
                }

                AddOptions(model, sb);

                sb.AppendLine("GO");
            }
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override IDatabaseObjectScript GetScript(SecurityPolicyModel model)
    {

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.security_policies INNER JOIN
            sys.schemas ON
                schemas.schema_id = security_policies.schema_id
        WHERE
            schemas.name = '{model.PolicySchema}' AND
            security_policies.name = '{model.PolicyName}'
    )");

            sb.AppendLine($"    CREATE SECURITY POLICY {QuoteCharacterStart}{model.PolicySchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.PolicyName}{QuoteCharacterEnd}");
            var predicateScripts = new List<StringBuilder>();
            foreach (var predicate in model.Predicates)
            {
                var predicateSql = new StringBuilder();
                predicateSql.AppendLine($"    ADD {predicate.PredicateType} PREDICATE {predicate.PredicateDefinition.Substring(1, predicate.PredicateDefinition.Length - 2)}");
                predicateSql.Append($"    ON {QuoteCharacterStart}{predicate.TargetSchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{predicate.TargetName}{QuoteCharacterEnd}");
                if (predicate.Operation != null && predicate.Operation.Length > 0)
                {
                    predicateSql.AppendLine();
                    predicateSql.Append(predicate.Operation);
                }
                predicateScripts.Add(predicateSql);
            }
            sb.AppendLine(string.Join(",", predicateScripts));
            var suffixes = new List<string>();

            if (!model.IsEnabled)
            {
                suffixes.Add("STATE = OFF");
            }

            if (!model.IsSchemaBound)
            {
                suffixes.Add("SCHEMABINDING = OFF");
            }

            if (suffixes.Count > 0)
            {
                sb.AppendLine("    WITH (" + string.Join(", ", suffixes) + ")");
            }

            sb.AppendLine();
            sb.AppendLine("GO");

            return sb.ToString();
        }

        string generateDropScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.security_policies INNER JOIN
            sys.schemas ON
                schemas.schema_id = security_policies.schema_id
        WHERE
            schemas.name = '{model.PolicySchema}' AND
            security_policies.name = '{model.PolicyName}'
    )");
            sb.AppendLine($"    DROP SECURITY POLICY {QuoteCharacterStart}{model.PolicySchema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.PolicyName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
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
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{model.TableName}' AND
            schemas.name = '{schema}'
    )");
            sb.AppendLine($"    DROP{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }

            sb.AppendLine(string.Format("-- {0}", model.TableName));

            if (model.TableName.StartsWith("default_", StringComparison.InvariantCultureIgnoreCase))
            {
                sb.AppendLine($@"IF EXISTS
    (
        SELECT
            1
        FROM 
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{model.TableName}' AND
            schemas.name = '{schema}'
    )");
                sb.AppendLine($"    DROP{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
                sb.AppendLine("GO");
            }
            sb.AppendLine();

            sb.AppendLine($@"IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = '{model.TableName}' AND
            schemas.name = '{schema}'
    )");

            sb.AppendLine($"    CREATE{(model.IsExternal ? " EXTERNAL " : " ")}TABLE {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TableName}{QuoteCharacterEnd}");
            sb.AppendLine("    " + "(");

            int intColumnCount = 0;

            foreach (Models.ColumnModel column in (
                from i in model.Columns
                orderby i.ColumnId
                select i))
            {
                if (intColumnCount != 0)
                {
                    sb.AppendLine(",");
                }

                sb.Append("    " + "    " + GetScript(column));

                intColumnCount += 1;
            }

            if (model.Columns.Where(c => c.GeneratedAlwaysType == 1).Any() &&
                model.Columns.Where(c => c.GeneratedAlwaysType == 2).Any())
            {
                sb.AppendLine(",");
                sb.Append($"        PERIOD FOR SYSTEM_TIME ({model.Columns.Where(c => c.GeneratedAlwaysType == 1).First().ColumnName}, {model.Columns.Where(c => c.GeneratedAlwaysType == 2).First().ColumnName})");
            }

            if (model.IsMemoryOptimized)
            {
                if (model.PrimaryKey != null)
                {
                    sb.AppendLine(",");
                    AppendTableInlineCreateScript(model.PrimaryKey, sb);
                }
            }
            else
            {
                sb.AppendLine();
            }

            sb.AppendLine("    )");

            AddOptions(sb);

            if (!string.IsNullOrEmpty(model.PartitionSchemeName))
            {
                sb.AppendLine($"    ON {model.PartitionSchemeName} ({model.PartitionSchemeColumns})");
            }

            sb.AppendLine("GO");

            AddExtendedProperties(model, sb);

            return sb.ToString();
        }

        void AddOptions(StringBuilder sb)
        {
            var options = model.Options;
            if (model.IsMemoryOptimized)
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"MEMORY_OPTIMIZED = ON, DURABILITY = {model.DurabilityDesc}";
            }
            if (!string.IsNullOrEmpty(model.DataSourceName))
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"DATA_SOURCE = {QuoteCharacterStart}{model.DataSourceName}{QuoteCharacterEnd}";
            }
            if (!string.IsNullOrEmpty(model.RemoteSchemaName))
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"SCHEMA_NAME = N'{model.RemoteSchemaName}'";
            }
            if (!string.IsNullOrEmpty(model.RemoteObjectName))
            {
                if (!string.IsNullOrEmpty(options))
                {
                    options += ", ";
                }
                options += $"OBJECT_NAME = N'{model.RemoteObjectName}'";
            }
            if (!string.IsNullOrEmpty(options))
            {
                sb.AppendLine($"    WITH ({options})");
            }
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    public override string GetRenameScript(TableModel model, string newTableName)
    {
        return $"EXEC sp_rename '{model.SchemaName}.{model.TableName}', '{newTableName}'";
    }

    public override IDatabaseObjectScript GetScript(TriggerModel model)
    {

        string generateDropScript()
        {
            var schema = string.IsNullOrEmpty(model.SchemaName) ? "dbo" : model.SchemaName;
            var sb = new StringBuilder();
            sb.AppendLine($"DROP TRIGGER IF EXISTS {QuoteCharacterStart}{schema}{QuoteCharacterEnd}.{QuoteCharacterStart}{model.TriggerName}{QuoteCharacterEnd}");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        string generateCreateScript()
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(model.Definition.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine));
            sb.AppendLine("GO");
            return sb.ToString();
        }

        return new DatabaseObjectScript(generateCreateScript, generateDropScript);
    }

    private void AddExtendedProperties(TableModel table, StringBuilder sb)
    {
        string type = table.IsView ? "view" : "table";

        foreach (var prop in table.ExtendedProperties.Where(i => !i.Key.EqualsIgnoreCase("dynamicName")))
        {
            sb.AppendLine($@"
IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('{prop.Key}', 'schema', '{table.SchemaName}', '{type}', '{table.TableName}', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'{prop.Key}',
                                     @level0type = N'schema',
                                     @level0name = '{table.SchemaName}',
                                     @level1type = N'{type}',
                                     @level1name = '{table.TableName}';
END
GO

EXEC sys.sp_addextendedproperty @name = N'{prop.Key}',
                                @value = N'{prop.Value}',
                                @level0type = N'schema',
                                @level0name = '{table.SchemaName}',
                                @level1type = N'{type}',
                                @level1name = '{table.TableName}';
GO
");
        }

        foreach (var column in table.Columns)
        {
            foreach (var prop in column.ExtendedProperties.Where(i => !i.Key.EqualsIgnoreCase("dynamicName")))
            {
                sb.AppendLine($@"
IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('{prop.Key}', 'schema', '{table.SchemaName}', '{type}', '{table.TableName}', 'column', '{column.ColumnName}')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'{prop.Key}',
                                     @level0type = N'schema',
                                     @level0name = '{table.SchemaName}',
                                     @level1type = N'{type}',
                                     @level1name = '{table.TableName}',
                                     @level2type = N'column',
                                     @level2name = '{column.ColumnName}';
END
GO

EXEC sys.sp_addextendedproperty @name = N'{prop.Key}',
                                @value = N'{prop.Value}',
                                @level0type = N'schema',
                                @level0name = '{table.SchemaName}',
                                @level1type = N'{type}',
                                @level1name = '{table.TableName}',
                                @level2type = N'column',
                                @level2name = '{column.ColumnName}';
GO
");
            }
        }

    }

    private void AppendTableInlineCreateScript(IndexModel model, System.Text.StringBuilder sb)
    {
        if (model.IsPrimaryKey)
        {
            sb.AppendLine($"        CONSTRAINT {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {(model.IsPrimaryKey ? "PRIMARY KEY" : "")} {model.IndexType}");
            sb.AppendLine("        (");
            int intColumnCount = 0;
            foreach (var column in model.Columns)
            {
                if (intColumnCount > 0)
                {
                    sb.AppendLine(",");
                }

                sb.Append($"            {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");

                intColumnCount += 1;
            }
            sb.AppendLine();
            sb.AppendLine("        )");

            AddOptions(model, sb, 2);
        }
        else
        {
            string indexType = model.IndexType;

            if (model.IsUnique)
            {
                indexType = "UNIQUE " + model.IndexType;
            }

            sb.AppendLine($"        INDEX {QuoteCharacterStart}{model.IndexName}{QuoteCharacterEnd} {indexType}");

            if (model.Columns.Any())
            {
                sb.AppendLine("        (");

                bool blnHasColumns = false;

                foreach (var column in model.Columns)
                {
                    if (blnHasColumns)
                    {
                        sb.AppendLine(",");
                    }
                    sb.Append($"            {QuoteCharacterStart}{column.ColumnName}{QuoteCharacterEnd}{(column.IsDescending ? " DESC" : "")}");
                    blnHasColumns = true;
                }

                sb.AppendLine();
                sb.AppendLine("        )");

                if (model.IncludeColumns.Any())
                {
                    sb.Append("        INCLUDE (");

                    bool blnHasIncludeColumns = false;

                    foreach (var includeColumn in model.IncludeColumns)
                    {
                        if (blnHasIncludeColumns)
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{QuoteCharacterStart}{includeColumn.ColumnName}{QuoteCharacterEnd}");
                        blnHasIncludeColumns = true;
                    }

                    sb.AppendLine("        )");
                }
            }

            if (!string.IsNullOrEmpty(model.FilterDefinition))
            {
                sb.AppendLine($"        WHERE {model.FilterDefinition.Replace("=", " = ").Replace("<>", " <> ")}");
            }

            AddOptions(model, sb, 2);
        }
    }

    private void AddOptions(IndexModel model, StringBuilder sb, int indentCount = 1)
    {
        if (model.FillFactor != 0 || model.TotalBucketCount != 0 || !string.IsNullOrEmpty(model.DataCompressionDesc))
        {
            var sbOptions = new StringBuilder();
            if (model.FillFactor != 0)
            {
                sbOptions.Append($"FILLFACTOR = {model.FillFactor}");
            }
            if (model.TotalBucketCount != 0)
            {
                if (sbOptions.Length > 0)
                {
                    sbOptions.Append(", ");
                }
                sbOptions.Append($"BUCKET_COUNT = {model.TotalBucketCount}");
            }
            if (!string.IsNullOrEmpty(model.DataCompressionDesc))
            {
                if (sbOptions.Length > 0)
                {
                    sbOptions.Append(", ");
                }
                sbOptions.Append($"DATA_COMPRESSION = {model.DataCompressionDesc}");
            }
            if (model.CompressionDelay.GetValueOrDefault() > 0)
            {
                if (sbOptions.Length > 0)
                {
                    sbOptions.Append(", ");
                }
                sbOptions.Append($"COMPRESSION_DELAY = {model.CompressionDelay} MINUTES");
            }
            sb.AppendLine($"{new string(' ', indentCount * 4)}WITH ({sbOptions.ToString()})");
        }

        if (!string.IsNullOrEmpty(model.PartitionSchemeName))
        {
            var partitionColumns = model.Columns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
            if (!partitionColumns.Any())
            {
                partitionColumns = model.IncludeColumns.Where(i => i.PartitionOrdinal > 0).OrderBy(i => i.PartitionOrdinal).Select(i => i.ColumnName).ToList();
            }
            if (partitionColumns.Any())
            {
                sb.AppendLine($"{new string(' ', indentCount * 4)}ON {model.PartitionSchemeName} ({string.Join(", ", partitionColumns)})");
            }
            else
            {
                sb.AppendLine($"{new string(' ', indentCount * 4)}ON {model.PartitionSchemeName}");
            }
        }

    }

    public override IDatabaseObjectScript GetColumnScript(ColumnModel column)
    {
        var script = new DatabaseObjectScript();

        script.CreateScript = $@"IF NOT EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = '{column.SchemaName}' AND
		tables.name = '{column.TableName}' AND
		columns.name = '{column.ColumnName}'
)
ALTER TABLE [{column.SchemaName}].[{column.TableName}] ADD {GetScript(column)}
GO";

        script.DropScript = $@"IF EXISTS
(
	SELECT
		1
	FROM
		sys.schemas INNER JOIN
		sys.tables ON
			schemas.schema_id = tables.schema_id INNER JOIN
		sys.columns ON
			tables.object_id = columns.object_id
	WHERE
        schemas.name = '{column.SchemaName}' AND
		tables.name = '{column.TableName}' AND
		columns.name = '{column.ColumnName}'
)
ALTER TABLE [{column.SchemaName}].[{column.TableName}] DROP COLUMN [{column.ColumnName}]
GO";

        return script;

    }

    private string GetScript(ColumnModel columnModel)
    {
        string strDataType = GetFullColumnType(columnModel);

        if (columnModel.IsComputed &&
            columnModel.GeneratedAlwaysType == 0)
        {
            strDataType = "AS " + columnModel.ComputedDefinition;
            if (columnModel.IsPersisted.GetValueOrDefault())
            {
                strDataType += " PERSISTED";
            }
        }

        string strIdentity = string.Empty;

        if (columnModel.IsIdentity)
        {
            strIdentity = " IDENTITY (1, 1)";
        }

        string generatedAlwaysType = "";

        if (columnModel.GeneratedAlwaysType > 0)
        {
            if (columnModel.GeneratedAlwaysType == 1)
            {
                generatedAlwaysType += " GENERATED ALWAYS AS ROW START";
            }
            else if (columnModel.GeneratedAlwaysType == 2)
            {
                generatedAlwaysType += " GENERATED ALWAYS AS ROW END";
            }
        }

        string strNull = " NULL";

        if (!columnModel.IsNullable)
        {
            strNull = " NOT NULL";
        }

        if (columnModel.IsComputed &&
            columnModel.GeneratedAlwaysType == 0)
        {
            strNull = string.Empty;
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

        var hiddentType = columnModel.IsHidden ? " HIDDEN" : "";

        return $"{QuoteCharacterStart}{columnModel.ColumnName}{QuoteCharacterEnd} {strDataType}{generatedAlwaysType}{strIdentity}{hiddentType}{strNull}{defaultValue}".Trim();

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
            case "BINARY":
            case "VARBINARY":
            case "VARCHAR":
            case "CHAR":
            case "NVARCHAR":
            case "NCHAR":
                string strPrecision = columnModel.Precision.ToString();
                if (columnModel.Precision == -1 || columnModel.Precision == Int32.MaxValue)
                {
                    strPrecision = "MAX";
                }
                strDataType = string.Format("{0}({1})", columnModel.ColumnType, strPrecision);
                break;
            case "TIME":
                strDataType = string.Format("{0}({1})", columnModel.ColumnType, columnModel.Scale);
                break;
            case "DATETIME2":
                if (columnModel.Scale != 7)
                {
                    strDataType = string.Format("{0}({1})", columnModel.ColumnType, columnModel.Scale);
                }
                break;
        }

        return strDataType;
    }

}
