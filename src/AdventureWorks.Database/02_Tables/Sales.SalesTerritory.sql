-- SalesTerritory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesTerritory' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesTerritory]
    (
        [TerritoryID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [Group] NVARCHAR(50) NOT NULL,
        [SalesYTD] MONEY NOT NULL DEFAULT (0.00),
        [SalesLastYear] MONEY NOT NULL DEFAULT (0.00),
        [CostYTD] MONEY NOT NULL DEFAULT (0.00),
        [CostLastYear] MONEY NOT NULL DEFAULT (0.00),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
