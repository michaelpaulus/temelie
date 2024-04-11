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
            indexes.name = 'PK_Culture_CultureID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[Culture] ADD CONSTRAINT [PK_Culture_CultureID] PRIMARY KEY CLUSTERED
    (
    [CultureID]
    )
GO
