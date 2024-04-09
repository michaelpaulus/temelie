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
            indexes.name = 'PK_Illustration_IllustrationID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[Illustration] ADD CONSTRAINT [PK_Illustration_IllustrationID] PRIMARY KEY CLUSTERED
    (
    [IllustrationID]
    )
GO
