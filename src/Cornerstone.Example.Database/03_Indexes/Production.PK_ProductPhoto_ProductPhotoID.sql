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
            indexes.name = 'PK_ProductPhoto_ProductPhotoID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductPhoto] ADD CONSTRAINT [PK_ProductPhoto_ProductPhotoID] PRIMARY KEY CLUSTERED
    (
    [ProductPhotoID]
    )
GO
