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
            indexes.name = 'PK_BusinessEntity_BusinessEntityID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[BusinessEntity] ADD CONSTRAINT [PK_BusinessEntity_BusinessEntityID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID]
    )
GO
