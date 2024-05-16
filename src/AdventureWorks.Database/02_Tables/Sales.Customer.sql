-- Customer

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Customer' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[Customer]
    (
        [CustomerID] INT IDENTITY (1, 1) NOT NULL,
        [PersonID] INT NULL,
        [StoreID] INT NULL,
        [TerritoryID] INT NULL,
        [AccountNumber] AS (isnull('AW'+[dbo].[ufnLeadingZeros]([CustomerID]),'')),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
