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
            indexes.name = 'PK_Document_DocumentNode' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[Document] ADD CONSTRAINT [PK_Document_DocumentNode] PRIMARY KEY CLUSTERED
    (
    [DocumentNode]
    )
GO
