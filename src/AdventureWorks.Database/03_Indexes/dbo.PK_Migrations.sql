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
            indexes.name = 'PK_Migrations' AND
            schemas.name = 'dbo'
    )
    ALTER TABLE [dbo].[Migrations] ADD CONSTRAINT [PK_Migrations] PRIMARY KEY CLUSTERED
    (
    [Id]
    )
GO
