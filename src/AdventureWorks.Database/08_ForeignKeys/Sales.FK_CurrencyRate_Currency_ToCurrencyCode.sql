
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_CurrencyRate_Currency_ToCurrencyCode' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[CurrencyRate] WITH CHECK ADD CONSTRAINT [FK_CurrencyRate_Currency_ToCurrencyCode] FOREIGN KEY ([ToCurrencyCode])
    REFERENCES [Sales].[Currency] ([CurrencyCode])
GO
