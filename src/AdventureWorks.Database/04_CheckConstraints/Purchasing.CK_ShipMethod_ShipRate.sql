
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
        check_constraints.name = 'CK_ShipMethod_ShipRate' AND
        schemas.name = 'Purchasing'
)
ALTER TABLE [Purchasing].[ShipMethod] WITH CHECK ADD CONSTRAINT [CK_ShipMethod_ShipRate] CHECK (([ShipRate]>(0.00)))
GO
