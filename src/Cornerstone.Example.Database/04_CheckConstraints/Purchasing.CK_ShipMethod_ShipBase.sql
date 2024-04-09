
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
        check_constraints.name = 'CK_ShipMethod_ShipBase' AND
        schemas.name = 'Purchasing'
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
            check_constraints.name = 'CK_ShipMethod_ShipBase' AND
            schemas.name = 'Purchasing' AND
            check_constraints.definition = '([ShipBase]>(0.00))'
    )
BEGIN
ALTER TABLE [Purchasing].[ShipMethod] DROP CONSTRAINT [CK_ShipMethod_ShipBase]
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
        check_constraints.name = 'CK_ShipMethod_ShipBase' AND
        schemas.name = 'Purchasing'
)
ALTER TABLE [Purchasing].[ShipMethod] WITH CHECK ADD CONSTRAINT [CK_ShipMethod_ShipBase] CHECK (([ShipBase]>(0.00)))
GO
