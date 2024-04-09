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
            indexes.name = 'PK_ScrapReason_ScrapReasonID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ScrapReason] ADD CONSTRAINT [PK_ScrapReason_ScrapReasonID] PRIMARY KEY CLUSTERED
    (
    [ScrapReasonID]
    )
GO
