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
            indexes.name = 'IX_Document_FileName_Revision' AND
            schemas.name = 'Production'
    )
    CREATE NONCLUSTERED INDEX [IX_Document_FileName_Revision] ON [Production].[Document]
    (
    [FileName],
    [Revision]
    )
GO
