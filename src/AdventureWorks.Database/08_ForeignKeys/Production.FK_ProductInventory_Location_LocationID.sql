
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductInventory_Location_LocationID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductInventory] WITH CHECK ADD CONSTRAINT [FK_ProductInventory_Location_LocationID] FOREIGN KEY ([LocationID])
    REFERENCES [Production].[Location] ([LocationID])
GO
