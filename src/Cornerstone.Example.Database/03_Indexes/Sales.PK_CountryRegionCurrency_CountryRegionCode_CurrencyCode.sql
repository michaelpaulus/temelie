IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[CountryRegionCurrency] ADD CONSTRAINT [PK_CountryRegionCurrency_CountryRegionCode_CurrencyCode] PRIMARY KEY CLUSTERED
    (
    [CountryRegionCode],
    [CurrencyCode]
    )
GO
