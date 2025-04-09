
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesOrderHeaderSalesReason] WITH CHECK ADD CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesOrderHeader_SalesOrderID] FOREIGN KEY ([SalesOrderID])
    REFERENCES [Sales].[SalesOrderHeader] ([SalesOrderID])
    ON DELETE CASCADE
GO
