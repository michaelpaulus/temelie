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
            indexes.name = 'PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[PersonPhone] ADD CONSTRAINT [PK_PersonPhone_BusinessEntityID_PhoneNumber_PhoneNumberTypeID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [PhoneNumber],
    [PhoneNumberTypeID]
    )
GO
