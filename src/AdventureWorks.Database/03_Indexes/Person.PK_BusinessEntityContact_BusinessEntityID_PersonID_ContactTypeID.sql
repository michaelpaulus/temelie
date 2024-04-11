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
            indexes.name = 'PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[BusinessEntityContact] ADD CONSTRAINT [PK_BusinessEntityContact_BusinessEntityID_PersonID_ContactTypeID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [PersonID],
    [ContactTypeID]
    )
GO
