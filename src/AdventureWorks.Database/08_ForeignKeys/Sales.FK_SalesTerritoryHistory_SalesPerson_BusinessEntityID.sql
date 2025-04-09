
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesTerritoryHistory] WITH CHECK ADD CONSTRAINT [FK_SalesTerritoryHistory_SalesPerson_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
