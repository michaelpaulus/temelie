
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesOrderHeaderSalesReason] WITH CHECK ADD CONSTRAINT [FK_SalesOrderHeaderSalesReason_SalesReason_SalesReasonID] FOREIGN KEY ([SalesReasonID])
    REFERENCES [Sales].[SalesReason] ([SalesReasonID])
GO
