
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Customer_SalesTerritory_TerritoryID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[Customer] WITH CHECK ADD CONSTRAINT [FK_Customer_SalesTerritory_TerritoryID] FOREIGN KEY ([TerritoryID])
    REFERENCES [Sales].[SalesTerritory] ([TerritoryID])
GO
