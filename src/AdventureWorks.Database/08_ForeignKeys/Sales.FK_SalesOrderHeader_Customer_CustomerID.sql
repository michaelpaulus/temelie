
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesOrderHeader_Customer_CustomerID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesOrderHeader] WITH CHECK ADD CONSTRAINT [FK_SalesOrderHeader_Customer_CustomerID] FOREIGN KEY ([CustomerID])
    REFERENCES [Sales].[Customer] ([CustomerID])
GO
