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
            indexes.name = 'PK_ProductProductPhoto_ProductID_ProductPhotoID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductProductPhoto] ADD CONSTRAINT [PK_ProductProductPhoto_ProductID_ProductPhotoID] PRIMARY KEY NONCLUSTERED
    (
    [ProductID],
    [ProductPhotoID]
    )
GO
