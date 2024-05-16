-- BusinessEntity

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'BusinessEntity' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[BusinessEntity]
    (
        [BusinessEntityID] INT IDENTITY (1, 1) NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
