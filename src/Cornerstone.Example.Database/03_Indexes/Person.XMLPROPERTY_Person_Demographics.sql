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
            indexes.name = 'XMLPROPERTY_Person_Demographics' AND
            schemas.name = 'Person'
    )
    CREATE XML INDEX [XMLPROPERTY_Person_Demographics] ON [Person].[Person]
    (
    [Demographics]
    )
GO
