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
            indexes.name = 'PXML_Person_AddContact' AND
            schemas.name = 'Person'
    )
    CREATE XML INDEX [PXML_Person_AddContact] ON [Person].[Person]
    (
    [AdditionalContactInfo]
    )
GO
