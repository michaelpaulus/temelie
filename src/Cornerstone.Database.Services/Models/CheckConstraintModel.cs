namespace Cornerstone.Database
{
    namespace Models
    {
        public class CheckConstraintModel : DatabaseObjectModel
        {
            #region Properties

            public string CheckConstraintName { get; set; }
            public string TableName { get; set; }
            public string SchemaName { get; set; }
            public string CheckConstraintDefinition { get; set; }

            #endregion

            #region Methods

            public override void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
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
        check_constraints.name = '{CheckConstraintName}' AND
        schemas.name = '{SchemaName}'
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
            check_constraints.name = '{CheckConstraintName}' AND
            schemas.name = '{SchemaName}' AND
            check_constraints.definition = '{CheckConstraintDefinition.Replace("'", "''")}'
    )");
                sb.AppendLine($@"BEGIN
ALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} DROP CONSTRAINT {quoteCharacterStart}{this.CheckConstraintName}{quoteCharacterEnd}
END
END
GO");

            }

            public override void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd)
            {
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
        check_constraints.name = '{CheckConstraintName}' AND
        schemas.name = '{SchemaName}'
)");
                sb.AppendLine($"ALTER TABLE {quoteCharacterStart}{this.SchemaName}{quoteCharacterEnd}.{quoteCharacterStart}{this.TableName}{quoteCharacterEnd} WITH CHECK ADD CONSTRAINT {quoteCharacterStart}{this.CheckConstraintName}{quoteCharacterEnd} CHECK ({this.CheckConstraintDefinition})");
                sb.AppendLine("GO");
            }

            #endregion

        }
    }

}
