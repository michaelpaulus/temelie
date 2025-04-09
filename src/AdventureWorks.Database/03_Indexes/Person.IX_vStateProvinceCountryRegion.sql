IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.views ON
                indexes.object_id = views.object_id INNER JOIN
            sys.schemas ON
                views.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'IX_vStateProvinceCountryRegion' AND
            schemas.name = 'Person'
    )
    CREATE UNIQUE CLUSTERED INDEX [IX_vStateProvinceCountryRegion] ON [Person].[vStateProvinceCountryRegion]
    (
    [StateProvinceID],
    [CountryRegionCode]
    )
GO
