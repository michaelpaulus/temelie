
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesPersonQuotaHistory] WITH CHECK ADD CONSTRAINT [FK_SalesPersonQuotaHistory_SalesPerson_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
