
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Store_SalesPerson_SalesPersonID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[Store] WITH CHECK ADD CONSTRAINT [FK_Store_SalesPerson_SalesPersonID] FOREIGN KEY ([SalesPersonID])
    REFERENCES [Sales].[SalesPerson] ([BusinessEntityID])
GO
