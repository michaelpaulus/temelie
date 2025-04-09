
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_StateProvince_CountryRegion_CountryRegionCode' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[StateProvince] WITH CHECK ADD CONSTRAINT [FK_StateProvince_CountryRegion_CountryRegionCode] FOREIGN KEY ([CountryRegionCode])
    REFERENCES [Person].[CountryRegion] ([CountryRegionCode])
GO
