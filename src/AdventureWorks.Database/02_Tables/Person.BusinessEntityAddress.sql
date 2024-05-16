-- BusinessEntityAddress

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'BusinessEntityAddress' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[BusinessEntityAddress]
    (
        [BusinessEntityID] INT NOT NULL,
        [AddressID] INT NOT NULL,
        [AddressTypeID] INT NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
