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
            indexes.name = 'PK_ProductModelIllustration_ProductModelID_IllustrationID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductModelIllustration] ADD CONSTRAINT [PK_ProductModelIllustration_ProductModelID_IllustrationID] PRIMARY KEY CLUSTERED
    (
    [ProductModelID],
    [IllustrationID]
    )
GO
