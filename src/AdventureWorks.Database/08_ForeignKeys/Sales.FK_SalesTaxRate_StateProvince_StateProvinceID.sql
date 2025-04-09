
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SalesTaxRate_StateProvince_StateProvinceID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesTaxRate] WITH CHECK ADD CONSTRAINT [FK_SalesTaxRate_StateProvince_StateProvinceID] FOREIGN KEY ([StateProvinceID])
    REFERENCES [Person].[StateProvince] ([StateProvinceID])
GO
