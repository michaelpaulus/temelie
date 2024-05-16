-- SalesOrderDetail

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesOrderDetail' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesOrderDetail]
    (
        [SalesOrderID] INT NOT NULL,
        [SalesOrderDetailID] INT IDENTITY (1, 1) NOT NULL,
        [CarrierTrackingNumber] NVARCHAR(25) NULL,
        [OrderQty] SMALLINT NOT NULL,
        [ProductID] INT NOT NULL,
        [SpecialOfferID] INT NOT NULL,
        [UnitPrice] MONEY NOT NULL,
        [UnitPriceDiscount] MONEY NOT NULL DEFAULT (0.0),
        [LineTotal] AS (isnull(([UnitPrice]*((1.0)-[UnitPriceDiscount]))*[OrderQty],(0.0))),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
