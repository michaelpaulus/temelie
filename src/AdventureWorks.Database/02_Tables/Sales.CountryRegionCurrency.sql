-- CountryRegionCurrency

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CountryRegionCurrency' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CountryRegionCurrency]
    (
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [CurrencyCode] NCHAR(3) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
