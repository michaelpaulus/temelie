-- CurrencyRate

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CurrencyRate' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CurrencyRate]
    (
        [CurrencyRateID] INT IDENTITY (1, 1) NOT NULL,
        [CurrencyRateDate] DATETIME NOT NULL,
        [FromCurrencyCode] NCHAR(3) NOT NULL,
        [ToCurrencyCode] NCHAR(3) NOT NULL,
        [AverageRate] MONEY NOT NULL,
        [EndOfDayRate] MONEY NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
