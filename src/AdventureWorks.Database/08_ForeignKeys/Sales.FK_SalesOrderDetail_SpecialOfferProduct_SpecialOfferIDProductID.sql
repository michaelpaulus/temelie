
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesOrderDetail] WITH CHECK ADD CONSTRAINT [FK_SalesOrderDetail_SpecialOfferProduct_SpecialOfferIDProductID] FOREIGN KEY ([SpecialOfferID], [ProductID])
    REFERENCES [Sales].[SpecialOfferProduct] ([SpecialOfferID], [ProductID])
GO
