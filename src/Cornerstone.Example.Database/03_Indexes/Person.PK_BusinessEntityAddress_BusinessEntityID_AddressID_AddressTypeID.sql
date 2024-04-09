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
            indexes.name = 'PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[BusinessEntityAddress] ADD CONSTRAINT [PK_BusinessEntityAddress_BusinessEntityID_AddressID_AddressTypeID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [AddressID],
    [AddressTypeID]
    )
GO
