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
            indexes.name = 'PXML_Store_Demographics' AND
            schemas.name = 'Sales'
    )
    CREATE XML INDEX [PXML_Store_Demographics] ON [Sales].[Store]
    (
    [Demographics]
    )
GO
