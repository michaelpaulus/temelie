
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ShoppingCartItem_Product_ProductID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[ShoppingCartItem] WITH CHECK ADD CONSTRAINT [FK_ShoppingCartItem_Product_ProductID] FOREIGN KEY ([ProductID])
    REFERENCES [Production].[Product] ([ProductID])
GO
