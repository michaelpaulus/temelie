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
            indexes.name = 'PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductModelProductDescriptionCulture] ADD CONSTRAINT [PK_ProductModelProductDescriptionCulture_ProductModelID_ProductDescriptionID_CultureID] PRIMARY KEY CLUSTERED
    (
    [ProductModelID],
    [ProductDescriptionID],
    [CultureID]
    )
GO
