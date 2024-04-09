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
            indexes.name = 'PK_SalesTaxRate_SalesTaxRateID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesTaxRate] ADD CONSTRAINT [PK_SalesTaxRate_SalesTaxRateID] PRIMARY KEY CLUSTERED
    (
    [SalesTaxRateID]
    )
GO
