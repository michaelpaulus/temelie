
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
        check_constraints.name = 'CK_ProductInventory_Bin' AND
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
            check_constraints.name = 'CK_ProductInventory_Bin' AND
            schemas.name = 'Production' AND
            check_constraints.definition = '([Bin]>=(0) AND [Bin]<=(100))'
    )
BEGIN
ALTER TABLE [Production].[ProductInventory] DROP CONSTRAINT [CK_ProductInventory_Bin]
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
        check_constraints.name = 'CK_ProductInventory_Bin' AND
        schemas.name = 'Production'
)
ALTER TABLE [Production].[ProductInventory] WITH CHECK ADD CONSTRAINT [CK_ProductInventory_Bin] CHECK (([Bin]>=(0) AND [Bin]<=(100)))
GO
