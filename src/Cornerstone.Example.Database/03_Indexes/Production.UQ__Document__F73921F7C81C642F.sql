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
            indexes.name = 'UQ__Document__F73921F7C81C642F' AND
            schemas.name = 'Production'
    )
    CREATE UNIQUE NONCLUSTERED INDEX [UQ__Document__F73921F7C81C642F] ON [Production].[Document]
    (
    [rowguid]
    )
GO
