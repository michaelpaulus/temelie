-- BusinessEntityContact

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'BusinessEntityContact' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[BusinessEntityContact]
    (
        [BusinessEntityID] INT NOT NULL,
        [PersonID] INT NOT NULL,
        [ContactTypeID] INT NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
