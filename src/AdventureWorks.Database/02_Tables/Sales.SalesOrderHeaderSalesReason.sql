-- SalesOrderHeaderSalesReason

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesOrderHeaderSalesReason' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesOrderHeaderSalesReason]
    (
        [SalesOrderID] INT NOT NULL,
        [SalesReasonID] INT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
