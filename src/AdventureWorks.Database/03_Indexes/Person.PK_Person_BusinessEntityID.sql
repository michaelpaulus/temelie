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
            indexes.name = 'PK_Person_BusinessEntityID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[Person] ADD CONSTRAINT [PK_Person_BusinessEntityID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID]
    )
GO
