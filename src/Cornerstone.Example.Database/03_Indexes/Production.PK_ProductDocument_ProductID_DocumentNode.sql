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
            indexes.name = 'PK_ProductDocument_ProductID_DocumentNode' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductDocument] ADD CONSTRAINT [PK_ProductDocument_ProductID_DocumentNode] PRIMARY KEY CLUSTERED
    (
    [ProductID],
    [DocumentNode]
    )
GO
