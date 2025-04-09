
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
        check_constraints.name = 'CK_Product_SafetyStockLevel' AND
        schemas.name = 'Production'
)
ALTER TABLE [Production].[Product] WITH CHECK ADD CONSTRAINT [CK_Product_SafetyStockLevel] CHECK (([SafetyStockLevel]>(0)))
GO
