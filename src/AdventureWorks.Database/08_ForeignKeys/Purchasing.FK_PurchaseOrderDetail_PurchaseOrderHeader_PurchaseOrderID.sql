
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[PurchaseOrderDetail] WITH CHECK ADD CONSTRAINT [FK_PurchaseOrderDetail_PurchaseOrderHeader_PurchaseOrderID] FOREIGN KEY ([PurchaseOrderID])
    REFERENCES [Purchasing].[PurchaseOrderHeader] ([PurchaseOrderID])
GO
