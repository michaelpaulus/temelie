-- Store

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Store' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[Store]
    (
        [BusinessEntityID] INT NOT NULL,
        [Name] NAME NOT NULL,
        [SalesPersonID] INT NULL,
        [Demographics] XML NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
