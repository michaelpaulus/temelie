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
            indexes.name = 'PXML_ProductModel_CatalogDescription' AND
            schemas.name = 'Production'
    )
    CREATE XML INDEX [PXML_ProductModel_CatalogDescription] ON [Production].[ProductModel]
    (
    [CatalogDescription]
    )
GO
