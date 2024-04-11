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
            indexes.name = 'PK_ContactType_ContactTypeID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[ContactType] ADD CONSTRAINT [PK_ContactType_ContactTypeID] PRIMARY KEY CLUSTERED
    (
    [ContactTypeID]
    )
GO
