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
            indexes.name = 'PK_EmailAddress_BusinessEntityID_EmailAddressID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[EmailAddress] ADD CONSTRAINT [PK_EmailAddress_BusinessEntityID_EmailAddressID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [EmailAddressID]
    )
GO
