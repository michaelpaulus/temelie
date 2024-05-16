-- ProductVendor

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductVendor' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[ProductVendor]
    (
        [ProductID] INT NOT NULL,
        [BusinessEntityID] INT NOT NULL,
        [AverageLeadTime] INT NOT NULL,
        [StandardPrice] MONEY NOT NULL,
        [LastReceiptCost] MONEY NULL,
        [LastReceiptDate] DATETIME NULL,
        [MinOrderQty] INT NOT NULL,
        [MaxOrderQty] INT NOT NULL,
        [OnOrderQty] INT NULL,
        [UnitMeasureCode] NCHAR(3) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
