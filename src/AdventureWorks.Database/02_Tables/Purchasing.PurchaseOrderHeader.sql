-- PurchaseOrderHeader

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PurchaseOrderHeader' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[PurchaseOrderHeader]
    (
        [PurchaseOrderID] INT IDENTITY (1, 1) NOT NULL,
        [RevisionNumber] TINYINT NOT NULL DEFAULT (0),
        [Status] TINYINT NOT NULL DEFAULT (1),
        [EmployeeID] INT NOT NULL,
        [VendorID] INT NOT NULL,
        [ShipMethodID] INT NOT NULL,
        [OrderDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [ShipDate] DATETIME NULL,
        [SubTotal] MONEY NOT NULL DEFAULT (0.00),
        [TaxAmt] MONEY NOT NULL DEFAULT (0.00),
        [Freight] MONEY NOT NULL DEFAULT (0.00),
        [TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))) PERSISTED,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
