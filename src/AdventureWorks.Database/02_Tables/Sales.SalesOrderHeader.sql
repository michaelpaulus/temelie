-- SalesOrderHeader

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesOrderHeader' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesOrderHeader]
    (
        [SalesOrderID] INT IDENTITY (1, 1) NOT NULL,
        [RevisionNumber] TINYINT NOT NULL DEFAULT (0),
        [OrderDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [DueDate] DATETIME NOT NULL,
        [ShipDate] DATETIME NULL,
        [Status] TINYINT NOT NULL DEFAULT (1),
        [OnlineOrderFlag] FLAG NOT NULL DEFAULT (1),
        [SalesOrderNumber] AS (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID]),N'*** ERROR ***')),
        [PurchaseOrderNumber] ORDERNUMBER NULL,
        [AccountNumber] ACCOUNTNUMBER NULL,
        [CustomerID] INT NOT NULL,
        [SalesPersonID] INT NULL,
        [TerritoryID] INT NULL,
        [BillToAddressID] INT NOT NULL,
        [ShipToAddressID] INT NOT NULL,
        [ShipMethodID] INT NOT NULL,
        [CreditCardID] INT NULL,
        [CreditCardApprovalCode] VARCHAR(15) NULL,
        [CurrencyRateID] INT NULL,
        [SubTotal] MONEY NOT NULL DEFAULT (0.00),
        [TaxAmt] MONEY NOT NULL DEFAULT (0.00),
        [Freight] MONEY NOT NULL DEFAULT (0.00),
        [TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
        [Comment] NVARCHAR(128) NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
