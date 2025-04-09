
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_CountryRegionCurrency_Currency_CurrencyCode' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[CountryRegionCurrency] WITH CHECK ADD CONSTRAINT [FK_CountryRegionCurrency_Currency_CurrencyCode] FOREIGN KEY ([CurrencyCode])
    REFERENCES [Sales].[Currency] ([CurrencyCode])
GO
