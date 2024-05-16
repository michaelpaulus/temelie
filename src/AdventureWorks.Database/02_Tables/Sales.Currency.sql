-- Currency

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Currency' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[Currency]
    (
        [CurrencyCode] NCHAR(3) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
