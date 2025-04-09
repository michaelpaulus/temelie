
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductVendor_Vendor_BusinessEntityID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[ProductVendor] WITH CHECK ADD CONSTRAINT [FK_ProductVendor_Vendor_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [Purchasing].[Vendor] ([BusinessEntityID])
GO
