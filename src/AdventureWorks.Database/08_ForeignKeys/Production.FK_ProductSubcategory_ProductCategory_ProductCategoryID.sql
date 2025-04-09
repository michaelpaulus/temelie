
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductSubcategory_ProductCategory_ProductCategoryID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductSubcategory] WITH CHECK ADD CONSTRAINT [FK_ProductSubcategory_ProductCategory_ProductCategoryID] FOREIGN KEY ([ProductCategoryID])
    REFERENCES [Production].[ProductCategory] ([ProductCategoryID])
GO
