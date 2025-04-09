
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_BillOfMaterials_Product_ProductAssemblyID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[BillOfMaterials] WITH CHECK ADD CONSTRAINT [FK_BillOfMaterials_Product_ProductAssemblyID] FOREIGN KEY ([ProductAssemblyID])
    REFERENCES [Production].[Product] ([ProductID])
GO
