
IF EXISTS
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
        check_constraints.name = 'CK_Location_CostRate' AND
        schemas.name = 'Production'
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
            check_constraints.name = 'CK_Location_CostRate' AND
            schemas.name = 'Production' AND
            check_constraints.definition = '([CostRate]>=(0.00))'
    )
BEGIN
ALTER TABLE [Production].[Location] DROP CONSTRAINT [CK_Location_CostRate]
END
END
GO

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
        check_constraints.name = 'CK_Location_CostRate' AND
        schemas.name = 'Production'
)
ALTER TABLE [Production].[Location] WITH CHECK ADD CONSTRAINT [CK_Location_CostRate] CHECK (([CostRate]>=(0.00)))
GO
