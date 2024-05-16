-- SalesPersonQuotaHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesPersonQuotaHistory' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesPersonQuotaHistory]
    (
        [BusinessEntityID] INT NOT NULL,
        [QuotaDate] DATETIME NOT NULL,
        [SalesQuota] MONEY NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
