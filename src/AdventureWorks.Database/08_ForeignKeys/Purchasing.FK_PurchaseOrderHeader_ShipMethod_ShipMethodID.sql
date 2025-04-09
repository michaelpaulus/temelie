
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_PurchaseOrderHeader_ShipMethod_ShipMethodID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[PurchaseOrderHeader] WITH CHECK ADD CONSTRAINT [FK_PurchaseOrderHeader_ShipMethod_ShipMethodID] FOREIGN KEY ([ShipMethodID])
    REFERENCES [Purchasing].[ShipMethod] ([ShipMethodID])
GO
