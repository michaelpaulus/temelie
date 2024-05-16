-- SalesTerritoryHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesTerritoryHistory' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesTerritoryHistory]
    (
        [BusinessEntityID] INT NOT NULL,
        [TerritoryID] INT NOT NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
