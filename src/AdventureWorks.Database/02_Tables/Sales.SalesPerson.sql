-- SalesPerson

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesPerson' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesPerson]
    (
        [BusinessEntityID] INT NOT NULL,
        [TerritoryID] INT NULL,
        [SalesQuota] MONEY NULL,
        [Bonus] MONEY NOT NULL DEFAULT (0.00),
        [CommissionPct] SMALLMONEY NOT NULL DEFAULT (0.00),
        [SalesYTD] MONEY NOT NULL DEFAULT (0.00),
        [SalesLastYear] MONEY NOT NULL DEFAULT (0.00),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
